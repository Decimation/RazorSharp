#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using RazorCommon.Diagnostics;
using RazorSharp.CoreClr;
using RazorSharp.Memory.Extern.Symbols.Attributes;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Symbols;
using RazorSharp.Utilities;

#endregion

namespace RazorSharp.Memory.Extern.Symbols
{
	/// <summary>
	/// Provides operations for working with <see cref="SymImportAttribute"/>
	/// </summary>
	public static class Symload
	{
		private const           string     SCOPE_RESOLUTION_OPERATOR = "::";
		private static readonly ISet<Type> BoundTypes;

		static Symload()
		{
			BoundTypes = new HashSet<Type>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBound(Type t) => BoundTypes.Contains(t);

		

		private static string GetSymbolName(SymImportAttribute attr, [NotNull] MemberInfo member)
		{
			Conditions.NotNull(member.DeclaringType, nameof(member.DeclaringType));

			// Resolve the symbol
			string fullSym       = null;
			string declaringName = member.DeclaringType.Name;

			if (attr.FullyQualified && attr.Symbol != null && !attr.UseMethodNameOnly) {
				fullSym = attr.Symbol;
			}
			else if (attr.UseMethodNameOnly && attr.Symbol == null) {
				fullSym = member.Name;
			}
			else if (attr.Symbol != null && !attr.UseMethodNameOnly && !attr.FullyQualified) {
				fullSym = declaringName + SCOPE_RESOLUTION_OPERATOR + attr.Symbol;
			}
			else if (attr.Symbol == null) {
				// Auto resolve
				fullSym = declaringName + SCOPE_RESOLUTION_OPERATOR + member.Name;
			}


			Conditions.NotNull(fullSym, nameof(fullSym));

			return fullSym;
		}

		private static ModuleInfo GetInfo(SymNamespaceAttribute attr)
		{
			if (attr.Image == Clr.ClrPdb.FullName && attr.Module == Clr.CLR_DLL_SHORT) {
				return Clr.ClrSymbols;
			}

			return new ModuleInfo(new FileInfo(attr.Image), Modules.GetBaseAddress(attr.Module));
		}

		private static void LoadField(object             inst,
		                              ModuleInfo         module,
		                              string             fullSym,
		                              MemberInfo         field,
		                              SymImportAttribute sym)
		{
			// todo: use FieldDescs and pointers

			var symField  = (SymFieldAttribute) sym;
			var fieldInfo = (FieldInfo) field;

			var addr = module.GetSymAddress(fullSym);
			var fieldType = symField.LoadAs ?? fieldInfo.FieldType;
			
			// todo: also add special support for strings and other native types
			
			var val  = addr.ReadAnyEx(fieldType);
			fieldInfo.SetValue(inst, val);
		}

		public static void Load(Type t, object inst = null)
		{
			if (IsBound(t)) {
				return;
			}

			// For now, only one image can be used per type
			var    nameSpaceAttr = t.GetCustomAttribute<SymNamespaceAttribute>();
			string nameSpace     = nameSpaceAttr?.Namespace;
			var mi = GetInfo(nameSpaceAttr);
			
			(MemberInfo[] members, SymImportAttribute[] attributes) = t.GetAnnotated<SymImportAttribute>();


			int lim = attributes.Length;

			if (lim == 0) {
				return;
			}

			Global.Log.Information("Binding type {Name}", t.Name);

			for (int i = 0; i < lim; i++) {
				var attr = attributes[i];
				var mem  = members[i];

				// Resolve the symbol

				string fullSym = GetSymbolName(attr, mem);

				if (nameSpace != null && !attr.IgnoreNamespace) {
					fullSym = nameSpace + SCOPE_RESOLUTION_OPERATOR + fullSym;
				}

				var addr = mi.GetSymAddress(fullSym);


				switch (mem.MemberType) {
					case MemberTypes.Constructor:
						// The import is a function (ctor)
						Functions.SetStableEntryPoint((MethodInfo) mem, addr.Address);
						break;
					case MemberTypes.Field:
						LoadField(inst, mi, fullSym, mem, attr);
						break;
					case MemberTypes.Method:
						// The import is a function
						Functions.SetStableEntryPoint((MethodInfo) mem, addr.Address);
						break;
					case MemberTypes.Property:
						break;
					case MemberTypes.All:
					case MemberTypes.Custom:
					case MemberTypes.NestedType:
					case MemberTypes.TypeInfo:
					case MemberTypes.Event:
					default:
						throw new ArgumentOutOfRangeException();
				}
			}


			BoundTypes.Add(t);

			// Don't dispose ClrSymbols - we need it for the life of the program
			if (!ReferenceEquals(mi, Clr.ClrSymbols))
				mi.Dispose();
		}

		
	}
}