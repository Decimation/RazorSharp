#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SimpleSharp.Diagnostics;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using RazorSharp.Memory.Extern.Symbols.Attributes;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native;
using RazorSharp.Native.Symbols;
using RazorSharp.Native.Win32;
using RazorSharp.Utilities;
using SimpleSharp.Extensions;

#endregion

namespace RazorSharp.Memory.Extern.Symbols
{
	/// <summary>
	/// Provides operations for working with <see cref="SymImportAttribute"/>
	/// </summary>
	public static class Symload
	{
		private const string SCOPE_RESOLUTION_OPERATOR = "::";
		private const string GET_PROPERTY_PREFIX       = "get_";
		private const string GET_PROPERTY_REPLACEMENT  = "Get";

		private static readonly ISet<Type> BoundTypes = new HashSet<Type>();


		private static bool HasFlagFast(this SymImportOptions value, SymImportOptions flag)
		{
			return (value & flag) == flag;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsBound(Type t) => BoundTypes.Contains(t);

		private static string ResolveSymbolName(SymImportAttribute attr, [NotNull] MemberInfo member)
		{
			Conditions.NotNull(member.DeclaringType, nameof(member.DeclaringType));

			//var attr          = member.GetCustomAttribute<SymImportAttribute>();
			var nameSpaceAttr = member.DeclaringType.GetCustomAttribute<SymNamespaceAttribute>();

			// Resolve the symbol

			string resolvedName       = attr.Symbol ?? member.Name;
			string nameSpace          = nameSpaceAttr.Namespace;
			string enclosingNamespace = member.DeclaringType.Name;

			var options = attr.Options;

			if (!options.HasFlagFast(SymImportOptions.IgnoreEnclosingNamespace)) {
				resolvedName = enclosingNamespace + SCOPE_RESOLUTION_OPERATOR + resolvedName;
			}

			if (!options.HasFlagFast(SymImportOptions.IgnoreNamespace)) {
				if (nameSpace != null) {
					resolvedName = nameSpace + SCOPE_RESOLUTION_OPERATOR + resolvedName;
				}
			}

			if (options.HasFlagFast(SymImportOptions.UseAccessorName)) {
				Conditions.Require(member.MemberType == MemberTypes.Method);
				resolvedName = resolvedName.Replace(GET_PROPERTY_PREFIX, GET_PROPERTY_REPLACEMENT);
			}

			Conditions.NotNull(resolvedName, nameof(resolvedName));

			return resolvedName;
		}

		#region Get ModuleInfo

		private static ModuleInfo GetInfo(SymNamespaceAttribute attr, Pointer<byte> baseAddr)
		{
			if (attr.Image == Clr.ClrPdb.FullName && attr.Module == Clr.CLR_DLL_SHORT) {
				return Clr.ClrSymbols;
			}


			return new ModuleInfo(new FileInfo(attr.Image), baseAddr);
		}

		private static ModuleInfo GetInfo(SymNamespaceAttribute attr)
			=> GetInfo(attr, Modules.GetBaseAddress(attr.ShortModuleName));

		private static ModuleInfo GetModuleInfo(MemberInfo type)
		{
			var nameSpaceAttr = type.GetCustomAttribute<SymNamespaceAttribute>();
			Conditions.NotNull(nameSpaceAttr, nameof(nameSpaceAttr));

			Pointer<byte> baseAddr = null;

			string shortName = nameSpaceAttr.ShortModuleName;

			if (!Modules.IsLoaded(shortName)) {
//				throw new Exception(String.Format("Module \"{0}\" is not loaded", nameSpaceAttr.Module));
				Global.Log.Debug("Module {Name} is not loaded, loading", shortName);
				var mod = Modules.LoadModule(nameSpaceAttr.Module);
				baseAddr = mod.BaseAddress;
			}

			var mi = !baseAddr.IsNull ? GetInfo(nameSpaceAttr, baseAddr) : GetInfo(nameSpaceAttr);

			return mi;
		}

		#endregion

		#region Load

		public static T Load<T>(Type type, T value)
		{
			if (IsBound(type)) {
				return value;
			}

			// For now, only one image can be used per type
			var mi = GetModuleInfo(type);

			LoadComponents(ref value, type, mi);

			BoundTypes.Add(type);

//			Global.Log.Debug("Done");

			return value;
		}

		public static void Load(Type type) => Load(type, default(object));

		public static T Load<T>(T value) => Load(value.GetType(), value);

		private static void LoadField<T>(ref T              value,
		                                 ModuleInfo         module,
		                                 string             fullSym,
		                                 MemberInfo         field,
		                                 SymImportAttribute sym)
		{
			// todo: use FieldDescs and pointers

			var symField  = (SymFieldAttribute) sym;
			var fieldInfo = new MetaField(((FieldInfo) field));

			var addr = module.GetSymAddress(fullSym);
//			Console.WriteLine(addr);
//			Console.WriteLine("{0:X}",ProcessApi.GetProcAddress(module.BaseAddress.Address,"g_int").ToInt64());

			if (addr.IsNull) {
				string msg = String.Format("Could not find the address of the symbol \"{0}\"", fullSym);
				throw new NullReferenceException(msg);
			}

			var options   = symField.FieldOptions;
			var fieldType = fieldInfo.FieldType;

			object loadedValue;

			// todo: also add special support for strings and other native types

			switch (options) {
				case SymFieldOptions.LoadDirect:
					var size = symField.SizeConst;

					if (size == Constants.INVALID_VALUE) {
						size = Unsafe.BaseSizeOfData(fieldType);
					}

					var mem = addr.CopyOutBytes(size);

					loadedValue = Unsafe.LoadFromEx(fieldType, mem);

					break;
				case SymFieldOptions.LoadAs:
					var fieldLoadType = symField.LoadAs ?? fieldType;

					loadedValue = addr.ReadAnyEx(fieldLoadType);

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}


			if (!fieldInfo.IsStatic) {
				var ptr = fieldInfo.GetAddress(ref value);


				ptr.WriteAnyEx(fieldType, loadedValue);
			}
			else {
				fieldInfo.SetValue(value, loadedValue);
			}
		}

		public static void LoadAll(Assembly asm)
		{
			foreach (var type in asm.GetTypes()) {
				if (Attribute.IsDefined(type, typeof(SymNamespaceAttribute))) {
					// Global.Log.Debug("Binding {Name}", type.Name);
					Load(type);
				}
			}
		}

		private static void LoadComponents<T>(ref T value, Type type, ModuleInfo mi)
		{
			(MemberInfo[] members, SymImportAttribute[] attributes) = type.GetAnnotated<SymImportAttribute>();

			int lim = attributes.Length;

			for (int i = 0; i < lim; i++) {
				var attr = attributes[i];
				var mem  = members[i];

				// Resolve the symbol

				string        name = ResolveSymbolName(attr, mem);
				Pointer<byte> addr = mi.GetSymAddress(name);

				switch (mem.MemberType) {
					case MemberTypes.Constructor:
						// The import is a function (ctor)
						Functions.SetEntryPoint((MethodInfo) mem, addr.Address);
						break;
					case MemberTypes.Field:
						LoadField(ref value, mi, name, mem, attr);
						break;
					case MemberTypes.Method:
						// The import is a function
						Functions.SetEntryPoint((MethodInfo) mem, addr.Address);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		#endregion

		#region Unload

		private static void UnloadStaticField(MemberInfo field)
		{
			var fi = (FieldInfo) field;
			fi.SetValue(null, default);
		}

		public static void Unload(Type type, bool unloadModule = true)
		{
			Conditions.Require(IsBound(type));

			if (unloadModule) {
				var    nameSpaceAttr = type.GetCustomAttribute<SymNamespaceAttribute>();
				string shortName     = nameSpaceAttr.ShortModuleName;
				Modules.UnloadIfLoaded(shortName);
			}


			(MemberInfo[] members, SymImportAttribute[] attributes) = type.GetAnnotated<SymImportAttribute>();

			int lim = attributes.Length;

			for (int i = 0; i < lim; i++) {
				var mem = members[i];

				switch (mem.MemberType) {
					case MemberTypes.Field:
						// The field will be deleted later
						if (((FieldInfo) mem).IsStatic) {
							UnloadStaticField(mem);
						}

						break;
					case MemberTypes.Method:
						// Calling the function will now result in an access violation
						var metaMethod = new MetaMethod((MethodInfo) mem);
						metaMethod.Reset();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			BoundTypes.Remove(type);
		}

		public static void Unload<T>(ref T value, bool unloadModule = true)
		{
			var type = value.GetType();

			Unload(type, unloadModule);

			Mem.Destroy(ref value);
		}

		#endregion


		public static void Reload<T>(ref T value)
		{
			var type = value.GetType();
			Conditions.Require(IsBound(type));

			var mi = GetModuleInfo(type);

			LoadComponents(ref value, type, mi);
		}
	}
}