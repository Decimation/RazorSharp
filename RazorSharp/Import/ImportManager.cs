using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using RazorSharp.Core;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Metadata.Enums;
using RazorSharp.Import.Attributes;
using RazorSharp.Import.Enums;
using RazorSharp.Interop;
using RazorSharp.Interop.Utilities;
using RazorSharp.Memory;
using RazorSharp.Memory.Enums;
using RazorSharp.Memory.Pointers;
using RazorSharp.Model;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Security;
using RazorSharp.Utilities.Security.Exceptions;
using SimpleSharp.Diagnostics;
using SimpleSharp.Strings;

// ReSharper disable ParameterTypeCanBeEnumerable.Global

namespace RazorSharp.Import
{
	

	public sealed class ImportManager : Releasable
	{
		#region Constants

		private const string GET_PROPERTY_PREFIX      = "get_";
		private const string GET_PROPERTY_REPLACEMENT = "Get";

		protected override string Id => nameof(ImportManager);

		private static readonly string MapError =
			$"Map must static, readonly, and of type {typeof(ImportMap)}";

		private static readonly string NamespaceError = 
			$"Type must be decorated with \"{nameof(ImportNamespaceAttribute)}\"";
		
		private delegate void LoadMethodFunction(ImportAttribute attr, MethodInfo memberInfo, Pointer<byte> ptr);

		private delegate void LoadFieldFunction<T>(ref T     value, IImportProvider ip, string id,
		                                           MetaField field, ImportAttribute attr);

		#endregion

		#region Singleton

		/// <summary>
		///     Gets an instance of <see cref="ImportManager" />
		/// </summary>
		public static ImportManager Value { get; private set; } = new ImportManager();

		private ImportManager()
		{
			Setup();
		}

		#endregion

		#region Override

		public override void Close()
		{
			UnloadAll();

			// Sanity check
			if (m_boundTypes.Count != 0 || m_typeImportMaps.Count != 0) {
				throw Guard.ImportFail();
			}

			// Delete instance
			Value = null;

			base.Close();
		}

		#endregion

		#region Fields

		private readonly ISet<Type> m_boundTypes = new HashSet<Type>();

		private readonly Dictionary<Type, ImportMap> m_typeImportMaps = new Dictionary<Type, ImportMap>();

		#endregion

		#region Helper

		internal static string Combine(params string[] args)
		{
			const string SCOPE_RESOLUTION_OPERATOR = "::";

			var sb = new StringBuilder();

			for (int i = 0; i < args.Length; i++) {
				sb.Append(args[i]);

				if (i + 1 != args.Length) {
					sb.Append(SCOPE_RESOLUTION_OPERATOR);
				}
			}

			return sb.ToString();
		}

		private bool IsBound(Type t) => m_boundTypes.Contains(t);

		private void VerifyImport(ImportAttribute attr, MemberInfo member)
		{
			switch (member.MemberType) {
				case MemberTypes.Constructor:
				case MemberTypes.Property:
				case MemberTypes.Method:

					if (!(attr is ImportCallAttribute)) {
						throw Guard.ImportFail();
					}

					break;
				case MemberTypes.Field:

					if (!(attr is ImportFieldAttribute)) {
						throw Guard.ImportFail();
					}

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static bool IsAnnotated(Type t, out ImportNamespaceAttribute attr)
		{
			attr = t.GetCustomAttribute<ImportNamespaceAttribute>();

			return attr != null;
		}

		

		private static string ResolveIdentifier(ImportAttribute attr, [NotNull] MemberInfo member)
		{
			return ResolveIdentifier(attr, member, out _);
		}

		private static string ResolveIdentifier(ImportAttribute attr, [NotNull] MemberInfo member,
		                                        out string      resolvedId)
		{
			Conditions.NotNull(member.DeclaringType, nameof(member.DeclaringType));
			
			if (!IsAnnotated(member.DeclaringType, out var nameSpaceAttr)) {
				throw Guard.ImportFail(NamespaceError);
			}

			// Resolve the symbol

			resolvedId = attr.Identifier ?? member.Name;

			string nameSpace          = nameSpaceAttr.Namespace;
			string enclosingNamespace = member.DeclaringType.Name;

			var options = attr.Options;

			if (member.MemberType == MemberTypes.Method
			    && attr is ImportCallAttribute callAttr
			    && callAttr.CallOptions.HasFlagFast(ImportCallOptions.Constructor)) {
				if (!options.HasFlagFast(IdentifierOptions.FullyQualified)) {
					throw Guard.ImportFail(
						$"\"{nameof(IdentifierOptions)}\" must be \"{nameof(IdentifierOptions.FullyQualified)}\"");
				}

				// return enclosingNamespace + SCOPE_RESOLUTION_OPERATOR + enclosingNamespace;
				return Combine(enclosingNamespace, enclosingNamespace);
			}


			if (!options.HasFlagFast(IdentifierOptions.IgnoreEnclosingNamespace)) {
				// resolvedId = enclosingNamespace + SCOPE_RESOLUTION_OPERATOR + resolvedId;
				resolvedId = Combine(enclosingNamespace, resolvedId);
			}

			if (!options.HasFlagFast(IdentifierOptions.IgnoreNamespace)) {
				if (nameSpace != null) {
					// resolvedId = nameSpace + SCOPE_RESOLUTION_OPERATOR + resolvedId;
					resolvedId = Combine(nameSpace, resolvedId);
				}
			}

			if (options.HasFlagFast(IdentifierOptions.UseAccessorName)) {
				Conditions.Require(member.MemberType == MemberTypes.Method);
				resolvedId = resolvedId.Replace(GET_PROPERTY_PREFIX, GET_PROPERTY_REPLACEMENT);
			}

			Conditions.NotNull(resolvedId, nameof(resolvedId));

			return resolvedId;
		}

		#endregion

		#region Unload

		private void UnloadMap(Type type, FieldInfo mapField)
		{
			var map = m_typeImportMaps[type];

			map.Clear();

			m_typeImportMaps.Remove(type);

			mapField.SetValue(null, null);

			// Sanity check
			Conditions.AssertDebug(!m_typeImportMaps.ContainsKey(type));
			Conditions.AssertDebug(mapField.GetValue(null) == null);
		}

		/// <summary>
		/// Root unload function. Unloads and restores the type <paramref name="type"/>.
		/// </summary>
		public void Unload(Type type)
		{
			if (!IsBound(type)) {
				return;
			}

			if (UsingMap(type, out var mapField)) {
				UnloadMap(type, mapField);

//				Global.Value.Log.Verbose("Unloaded map in {Name}", type.Name);
			}


			(MemberInfo[] members, ImportAttribute[] attributes) = type.GetAnnotated<ImportAttribute>();

			int lim = attributes.Length;

			for (int i = 0; i < lim; i++) {
				var mem  = members[i];
				var attr = attributes[i];

				bool wasBound = attr is ImportCallAttribute callAttr &&
				                callAttr.CallOptions.HasFlagFast(ImportCallOptions.Bind);


				switch (mem.MemberType) {
					case MemberTypes.Property:
						var propInfo = (PropertyInfo) mem;
						var get      = propInfo.GetMethod;

						// Calling the function will now result in an access violation
						if (wasBound) {
							FunctionFactory.Managed.Restore(get);
						}

						break;
					case MemberTypes.Field:
						// The field will be deleted later
						var fi = (FieldInfo) mem;
						if (fi.IsStatic) {
							fi.SetValue(null, default);
						}

						break;
					case MemberTypes.Method:

						// Calling the function will now result in an access violation
						if (wasBound) {
							FunctionFactory.Managed.Restore((MethodInfo) mem);
						}


						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

//				Global.Value.Log.Verbose("Unloaded member {Name}", mem.Name);
			}

			m_boundTypes.Remove(type);

			Global.Value.WriteVerbose(Id, "Unloaded {Name}", type.Name);
		}

		public void Unload<T>(ref T value)
		{
			var type = value.GetType();

			Unload(type);

			Mem.Destroy(ref value);
		}

		public void UnloadAll(Type[] t)
		{
			foreach (var type in t) {
				Unload(type);
			}
		}

		public void UnloadAll() => UnloadAll(m_boundTypes.ToArray());

		#endregion

		#region Load

		#region Map

		private void LoadMap(Type t, FieldInfo field)
		{
			if (!field.IsStatic || field.FieldType != typeof(ImportMap)) {
				throw Guard.ImportFail(MapError);
			}

			var map = (ImportMap) field.GetValue(null);
			m_typeImportMaps.Add(t, map);
		}


		private FieldInfo FindMapField(Type type)
		{
			var mapField = type.GetAnyField(ImportMap.FIELD_NAME);

			if (mapField != null && mapField.GetCustomAttribute<ImportMapDesignationAttribute>() == null) {
				throw Guard.ImportFail(
					$"Map field should be annotated with {nameof(ImportMapDesignationAttribute)}");
			}

			if (mapField == null) {
				var (member, _) = type.GetFirstAnnotated<ImportMapDesignationAttribute>();

				if (member != null) {
					mapField = (FieldInfo) member;
				}
			}

			return mapField;
		}

		private static bool CheckImportMap(FieldInfo mapField)
		{
			return mapField.IsStatic							// Must be static
			       && mapField.IsInitOnly 						// Must be readonly
			       && mapField.FieldType == typeof(ImportMap); 	// Must be of type ImportMap
		}

		private bool UsingMap(Type type, out FieldInfo mapField)
		{
			mapField = FindMapField(type);

			bool exists = mapField != null;

			if (exists) {
				if (!CheckImportMap(mapField)) {
					throw Guard.ImportFail(MapError);
				}

				if (mapField.GetValue(null) == null) {
					throw new ImportException($"{typeof(ImportMap)} is null");
				}
			}

			return exists;
		}

		#endregion

		/// <summary>
		///     Root load function. Loads <paramref name="value" /> of type <paramref name="type" /> using the
		///     specified <see cref="IImportProvider" /> <paramref name="ip" />.
		/// </summary>
		/// <param name="value">Value of type <paramref name="type" /> to load</param>
		/// <param name="type"><see cref="MetaType" /> of <paramref name="value" /></param>
		/// <param name="ip"><see cref="IImportProvider" /> to use to load components</param>
		/// <typeparam name="T">Type of <paramref name="value" /></typeparam>
		/// <returns><paramref name="value" />, fully loaded</returns>
		private T Load<T>(T value, Type type, IImportProvider ip)
		{
			if (IsBound(type)) {
				return value;
			}

			if (!IsAnnotated(type, out _)) {
				throw Guard.ImportFail(NamespaceError);
			}

			if (UsingMap(type, out var mapField)) {
				LoadMap(type, mapField);
				value = LoadComponents(value, type, ip, LoadMethod);
			}
			else {
				value = LoadComponents(value, type, ip);
			}

			m_boundTypes.Add(type);

			Global.Value.WriteVerbose(Id, "Completed loading {Name}", type.Name);

			return value;
		}

		/// <summary>
		///     Loads <paramref name="value" /> using <paramref name="ip" />.
		/// </summary>
		/// <param name="value">Value to load</param>
		/// <param name="ip"><see cref="IImportProvider" /> to use</param>
		/// <typeparam name="T">Type of <paramref name="value" /></typeparam>
		/// <returns><paramref name="value" />, fully loaded</returns>
		public T Load<T>(T value, IImportProvider ip) => Load(value, value.GetType(), ip);

		/// <summary>
		///     Loads any non-instance components of type <paramref name="t" />.
		/// </summary>
		/// <param name="t"><see cref="Type" /> to load</param>
		/// <param name="ip"><see cref="IImportProvider" /> to use</param>
		/// <returns>A <c>default</c> object of type <paramref name="t" /></returns>
		public object Load(Type t, IImportProvider ip) => Load(default(object), t, ip);

		public void LoadAll(Type[] t, IImportProvider ip)
		{
			foreach (var type in t) {
				Load(type, ip);
			}
		}

		// Shortcut
//		internal void LoadClr(Type t) => Load(t, Clr.Value.ClrSymbols);

		// Shortcut
//		internal void LoadAllClr(Type[] t) => LoadAll(t, Clr.Value.ClrSymbols);

		#region Load field

		private static object CopyInField(ImportFieldAttribute ifld, MetaField field, Pointer<byte> ptr)
		{
			var type = field.FieldType.RuntimeType;

			int size = ifld.SizeConst;

			if (size == Constants.INVALID_VALUE) {
				size = Unsafe.SizeOf(type, SizeOfOptions.BaseData);
			}

			byte[] mem = ptr.CopyBytes(size);

			return Converter.AllocRaw(mem, type);
		}

		private static object ProxyLoadField(ImportFieldAttribute ifld, MetaField field, Pointer<byte> ptr)
		{
			var fieldLoadType = (MetaType) (ifld.LoadAs ?? field.FieldType.RuntimeType);

			return fieldLoadType.IsAnyPointer ? ptr : ptr.ReadAny(fieldLoadType.RuntimeType);
		}

		private static void FastLoadField(MetaField fieldInfo, Pointer<byte> addr, Pointer<byte> fieldAddr)
		{
			int    fieldSize = fieldInfo.Size;
			byte[] memCpy    = addr.CopyBytes(fieldSize);
			fieldAddr.WriteAll(memCpy);
		}

		private void LoadField<T>(ref T           value,
		                          IImportProvider ip,
		                          string          identifier,
		                          MetaField       field,
		                          ImportAttribute attr)
		{
			var           ifld      = (ImportFieldAttribute) attr;
			Pointer<byte> ptr       = ip.GetAddress(identifier);
			var           options   = ifld.FieldOptions;
			Pointer<byte> fieldAddr = field.GetValueAddress(ref value);

			object fieldValue;

			Global.Value.WriteDebug(Id, "Loading field {Id} with {Option}",
			                        field.Name, options);

			switch (options) {
				case ImportFieldOptions.CopyIn:
					fieldValue = CopyInField(ifld, field, ptr);
					break;
				case ImportFieldOptions.Proxy:
					fieldValue = ProxyLoadField(ifld, field, ptr);
					break;
				case ImportFieldOptions.Fast:
					FastLoadField(field, ptr, fieldAddr);
					return;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (field.FieldType.IsAnyPointer) {
				ptr.WritePointer((Pointer<byte>) fieldValue);
			}
			else {
				ptr.WriteAny(field.FieldType.RuntimeType, fieldValue);
			}
		}

		#endregion


		private void LoadMethod(ImportAttribute attr, MethodInfo method, Pointer<byte> addr)
		{
			var callAttr = attr as ImportCallAttribute;
			Conditions.NotNull(callAttr, nameof(callAttr));
			var options = callAttr.CallOptions;

			if (options == ImportCallOptions.None) {
				throw Guard.ImportFail("You must specify an option");
			}
			
			bool bind     = options.HasFlagFast(ImportCallOptions.Bind);
			bool addToMap = options.HasFlagFast(ImportCallOptions.Map);

			if (bind && addToMap) {
				throw Guard.ImportFail(
					$"The option {ImportCallOptions.Bind} cannot be used with {ImportCallOptions.Map}");
			}

			if (bind) {
//				Global.Value.Log.Warning("Binding {Name}", method.Name);
				FunctionFactory.Managed.SetEntryPoint(method, addr);
			}

			if (addToMap) {
				var enclosing = method.DeclaringType;

				if (enclosing == null) {
					throw Guard.AmbiguousFail();
				}

				var name = method.Name;

				if (name.StartsWith(GET_PROPERTY_PREFIX)) {
					// The nameof operator does not return the name with the get prefix
					name = name.Erase(GET_PROPERTY_PREFIX);
				}


				m_typeImportMaps[enclosing].Add(name, addr);
			}
		}


		private static T LoadComponents<T>(T                    value,
		                                   Type                 type,
		                                   IImportProvider      ip,
		                                   LoadMethodFunction   methodFn,
		                                   LoadFieldFunction<T> fieldFn = null)
		{
			(MemberInfo[] members, ImportAttribute[] attributes) = type.GetAnnotated<ImportAttribute>();

			int lim = attributes.Length;

			if (lim == default) {
				return value;
			}

			for (int i = 0; i < lim; i++) {
				var attr = attributes[i];
				var mem  = members[i];

				// Resolve the symbol

				string        id   = ResolveIdentifier(attr, mem);
				Pointer<byte> addr = ip.GetAddress(id);

				switch (mem.MemberType) {
					case MemberTypes.Property:
						var propInfo = (PropertyInfo) mem;
						var get      = propInfo.GetMethod;
						methodFn(attr, get, addr);
						break;
					case MemberTypes.Method:
						// The import is a function or (ctor)
						methodFn(attr, (MethodInfo) mem, addr);
						break;
					case MemberTypes.Field:
						fieldFn?.Invoke(ref value, ip, id, (MetaField) mem, attr);
						break;
					default:
						throw Guard.NotSupportedMemberFail(mem);
				}

				Global.Value.WriteVerbose(null, "Loaded member {Id} @ {Addr}", id, addr);
			}

			return value;
		}

		private T LoadComponents<T>(T value, Type type, IImportProvider ip)
		{
			return LoadComponents(value, type, ip, LoadMethod, LoadField);
		}

		#endregion
	}
}