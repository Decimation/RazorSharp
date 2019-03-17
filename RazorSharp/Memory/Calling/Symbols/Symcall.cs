using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using RazorSharp.CoreClr;
using RazorSharp.Memory.Calling.Symbols.Attributes;
using RazorSharp.Native;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

namespace RazorSharp.Memory.Calling.Symbols
{
	public static class Symcall
	{
		private static readonly ISet<Type> BoundTypes;

		private const string SCOPE_RESOLUTION_OPERATOR = "::";

		static Symcall()
		{
			BoundTypes = new HashSet<Type>();
		}


		private static Pointer<byte> GetClrFunctionAddress(string name)
		{
			return Native.Symbols.GetSymAddress(Clr.ClrPdb.FullName, Clr.CLR_DLL_SHORT, name);
		}

		public static bool IsBound(Type t)
		{
			return BoundTypes.Contains(t);
		}

		public static void BindQuick(Type t)
		{
			MethodInfo[] methods = t.GetAnnotatedMethods<SymcallAttribute>();

			if (methods.Length == 0 || IsBound(t)) {
				return;
			}

			var baseAttr = methods[0].GetCustomAttribute<SymcallAttribute>();
			var sym      = new Native.Symbols(baseAttr.Image);
			var contexts = new List<string>();

			foreach (var method in methods) {
				var attr = method.GetCustomAttribute<SymcallAttribute>();

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


				Conditions.RequiresNotNull(fullSym, nameof(fullSym));
				//Global.Log.Debug("Sym {Name}", fullSym);
				contexts.Add(fullSym);
			}

			var offsets = sym.GetSymOffsets(contexts.ToArray());


			var addresses = Modules.GetAddresses(baseAttr.Module, offsets).ToArray();
			Conditions.Requires(addresses.Length == methods.Length);

			for (int i = 0; i < methods.Length; i++) {
//				Global.Log.Debug("Binding {Name} to {Addr}", methods[i].Name,
//				                 addresses[i].ToString("P"));
				var addr = addresses[i].Address;
				ClrFunctions.SetStableEntryPoint(methods[i], addr);
			}

			sym.Dispose();
			BoundTypes.Add(t);
		}

		public static void Bind(Type t)
		{
			throw new NotImplementedException();
			var methods = t.GetAllMethods()
			                     .Where(x => x.GetCustomAttribute<SymcallAttribute>() != null)
			                     .ToArray();

			foreach (var method in methods) {
				var attr = method.GetCustomAttribute<SymcallAttribute>();
			}
		}
	}
}