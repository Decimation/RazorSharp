#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using RazorSharp.Import.Attributes;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Symbols;
using RazorSharp.Utilities;
using SimpleSharp.Diagnostics;
using SimpleSharp.Strings;
using Unsafe = RazorSharp.Memory.Unsafe;

#endregion

namespace RazorSharp.Import
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
		private static bool IsBound(Type t)
		{
			return BoundTypes.Contains(t);
		}

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

		private static void AssemblyWork(Assembly asm, Action<Type> fn)
		{
			foreach (var type in asm.GetTypes()) {
				if (Attribute.IsDefined(type, typeof(SymNamespaceAttribute))) {
					fn(type);
				}
			}
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

		// ReSharper disable once SuggestBaseTypeForParameter
		private static ModuleInfo GetModuleInfo(Type type)
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

		/// <summary>
		/// Base function for binding and loading symbol imports.
		/// </summary>
		public static T Load<T>(Type type, T value)
		{
			// todo: prevent binding during unloading
			
			if (IsBound(type)) {
				return value;
			}

			// For now, only one image can be used per type
			var mi = GetModuleInfo(type);

			LoadComponents(ref value, type, mi);

			BoundTypes.Add(type);

			Global.Log.Verbose("[{Status}] Done loading {Name}",  
			                   StringConstants.CHECK_MARK, type.Name);

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
			var fieldInfo = new MetaField((FieldInfo) field);

			var addr = module.GetSymAddress(fullSym);

			if (addr.IsNull) {
				string msg = String.Format("Could not find the address of the symbol \"{0}\"", fullSym);
				throw new NullReferenceException(msg);
			}

			// fieldInfo.DebugAddresses(ref value);

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

					loadedValue = Unsafe.LoadFrom(fieldType, mem);

					break;
				case SymFieldOptions.LoadAs:
					var fieldLoadType = symField.LoadAs ?? fieldType;

					if (Runtime.IsPointer(fieldLoadType)) {
						loadedValue = addr;
					}
					else {
						loadedValue = addr.ReadAnyEx(fieldLoadType);
					}

					break;
				case SymFieldOptions.LoadFast:
					var fieldSize = fieldInfo.Size;
					var memCpy    = addr.CopyOutBytes(fieldSize);
					var fieldAddr = fieldInfo.GetValueAddress(ref value);
					fieldAddr.WriteAll(memCpy);
					return;
				default:
					throw new ArgumentOutOfRangeException();
			}


			Pointer<byte> ptr = fieldInfo.GetValueAddress(ref value);

			if (Runtime.IsPointer(fieldInfo.FieldType)) {
				ptr.WritePointer((Pointer<byte>) loadedValue);
			}
			else {
				ptr.WriteAnyEx(fieldType, loadedValue);
			}
		}


		public static void LoadAll(Assembly asm) => AssemblyWork(asm, Load);


		private static void LoadComponents<T>(ref T value, Type type, ModuleInfo mi)
		{
			var (members, attributes) = type.GetAnnotated<SymImportAttribute>();

			int lim = attributes.Length;

			for (int i = 0; i < lim; i++) {
				var attr = attributes[i];
				var mem  = members[i];

				// Resolve the symbol

				string        name = ResolveSymbolName(attr, mem);
				Pointer<byte> addr = mi.GetSymAddress(name);

				switch (mem.MemberType) {
					case MemberTypes.Method:
					case MemberTypes.Constructor:
						// The import is a function or (ctor)
						Functions.SetEntryPoint((MethodInfo) mem, addr.Address);
						break;
					case MemberTypes.Field:
						LoadField(ref value, mi, name, mem, attr);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		#endregion

		#region Unload

		public static void UnloadAll(Assembly asm) => AssemblyWork(asm, Unload);

		public static void Unload(Type type) => Unload(type, false);

		public static void Unload(Type type, bool unloadModule)
		{
			if (!IsBound(type)) {
				return;
			}

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
						var fi = (FieldInfo) mem;
						if (fi.IsStatic) {
							fi.SetValue(null, default);
						}

						break;
					case MemberTypes.Method:
						// Calling the function will now result in an access violation
						MetaMethod metaMethod = (MethodInfo) mem;
						metaMethod.Reset();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			BoundTypes.Remove(type);
		}

		public static void Unload<T>(ref T value, bool unloadModule = false)
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