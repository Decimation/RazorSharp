#region

using System;
using System.Collections.Generic;
using System.IO;
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
	public static class Symcall
	{
		private const           string     SCOPE_RESOLUTION_OPERATOR = "::";
		private static readonly ISet<Type> BoundTypes;

		static Symcall()
		{
			BoundTypes = new HashSet<Type>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBound(Type t) => BoundTypes.Contains(t);

		public static void BindQuick(Type t, string method)
		{
			var    methodInfo = t.GetAnyMethod(method);
			var    attr       = methodInfo.GetCustomAttribute<SymcallAttribute>();
			string fullSym    = GetSymbolName(attr, methodInfo);

			using (var sym = new SymbolEnvironment(attr.Image)) {
				long          offset  = sym.GetSymOffset(fullSym);
				Pointer<byte> address = Modules.GetAddress(attr.Module, offset);
				Functions.SetStableEntryPoint(methodInfo, address.Address);
			}
		}

		private static string GetSymbolName(SymcallAttribute attr, [NotNull] MethodInfo method)
		{
			Conditions.NotNull(method.DeclaringType, nameof(method.DeclaringType));

			// Resolve the symbol
			string fullSym       = null;
			string declaringName = method.DeclaringType.Name;

			if (attr.FullyQualified && attr.Symbol != null && !attr.UseMethodNameOnly) {
				fullSym = attr.Symbol;
			}
			else if (attr.UseMethodNameOnly && attr.Symbol == null) {
				fullSym = method.Name;
			}
			else if (attr.Symbol != null && !attr.UseMethodNameOnly && !attr.FullyQualified) {
				fullSym = declaringName + SCOPE_RESOLUTION_OPERATOR + attr.Symbol;
			}
			else if (attr.Symbol == null) {
				// Auto resolve
				fullSym = declaringName + SCOPE_RESOLUTION_OPERATOR + method.Name;
			}


			Conditions.NotNull(fullSym, nameof(fullSym));

			return fullSym;
		}

		private static ModuleInfo GetInfo(SymcallAttribute attr)
		{
			if (attr.Image == Clr.ClrPdb.FullName && attr.Module == Clr.CLR_DLL_SHORT) {
				return Clr.ClrSymbols;
			}

			return new ModuleInfo(new FileInfo(attr.Image), Modules.GetModule(attr.Module));
		}


		private static void LoadMethods()
		{
			
		}
		
		public static void BindQuick(Type t)
		{
			if (IsBound(t)) {
				return;
			}

			var    nameSpaceAttr = t.GetCustomAttribute<SymNamespaceAttribute>();
			string nameSpace     = nameSpaceAttr?.Namespace;


			(MethodInfo[] methods, SymcallAttribute[] attributes) = t.GetAnnotatedMethods<SymcallAttribute>();

			int lim = methods.Length;

			if (lim == 0) {
				return;
			}

			Global.Log.Information("Binding type {Name}", t.Name);


			// For now, only one image can be used per type
			var baseAttr = attributes[0];
			var sym      = GetInfo(baseAttr);
			var contexts = new string[lim];


			for (int i = 0; i < lim; i++) {
				var attr   = attributes[i];
				var method = methods[i];

				// Resolve the symbol

				string fullSym = GetSymbolName(attr, method);

				if (nameSpace != null && !attr.IgnoreNamespace) {
					fullSym = nameSpace + SCOPE_RESOLUTION_OPERATOR + fullSym;
				}

//				Global.Log.Debug("Binding {Name} -> {Orig}", fullSym,
//				                 method.Name);

				contexts[i] = fullSym;
			}


			var addresses = sym.GetSymAddresses(contexts);

			if (addresses.Length != lim) { }

			Conditions.Assert(addresses.Length == lim);

			for (int i = 0; i < lim; i++) {
				// .text	0000000180001000	000000018070E000	R	.	X	.	L	para	0001	public	CODE	64	0000	0000	0003	FFFFFFFFFFFFFFFF	FFFFFFFFFFFFFFFF

//				Global.Log.Debug("Binding {Name} to {Addr} (offset: {Offset})", methods[i].Name,
//				                 addresses[i].ToString("P"), offsets[i].ToString("X"));

				var addr = addresses[i].Address;
				Functions.SetStableEntryPoint(methods[i], addr);
			}

			BoundTypes.Add(t);

			// Don't dispose ClrSymbols - we need it for the life of the program
			if (!ReferenceEquals(sym, Clr.ClrSymbols))
				sym.Dispose();
		}
	}
}