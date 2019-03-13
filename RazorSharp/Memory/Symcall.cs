using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using RazorSharp.Clr;
using RazorSharp.Clr.Structures;
using RazorSharp.Memory.Attributes;
using RazorSharp.Native;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

namespace RazorSharp.Memory
{
	public static unsafe class Symcall
	{
		internal delegate long SetStableEntryPointInterlockedDelegate(MethodDesc* __this, ulong pCode);

		private static SetStableEntryPointInterlockedDelegate _setStableEntryPointInterlocked;

		internal static Pointer<byte> GetClrFunctionAddress(string name)
		{
			return Symbolism.GetFuncAddr(Symbolism.CLR_PDB, Clr.Clr.CLR_DLL, name);
		}

		internal static void Setup()
		{
			var fn = GetClrFunctionAddress("MethodDesc::SetStableEntryPointInterlocked").Address;

			_setStableEntryPointInterlocked =
				Marshal.GetDelegateForFunctionPointer<SetStableEntryPointInterlockedDelegate>(fn);
		}

		[HandleProcessCorruptedStateExceptions]
		internal static void SetStableEntryPoint(MethodInfo mi, IntPtr pCode)
		{
			var pMd = (MethodDesc*) mi.MethodHandle.Value;
			try {
				_setStableEntryPointInterlocked(pMd, (ulong) pCode);
			}
			catch (SEHException e) {
				Console.WriteLine(e);
				Console.WriteLine("HResult: {0:X}",e.HResult);
				foreach (DictionaryEntry dic in e.Data) {
					Console.WriteLine("> [{0}, {1}]",dic.Key, dic.Value);
				}
				throw;
			}
		}

		public static void BindQuick(Type t)
		{
			var methods = Runtime.GetMethods(t)
			                     .Where(x => x.GetCustomAttribute<SymcallAttribute>() != null)
			                     .ToArray();

			Global.Log.Debug("Detected {Count} decorated methods", methods.Length);
			var baseAttr = methods[0].GetCustomAttribute<SymcallAttribute>();
			var sym      = new Symbolism(baseAttr.Image);
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
					fullSym = declaringName + "::" + attr.Symbol;
				}
				else if (attr.Symbol == null) {
					// Auto resolve
					fullSym = declaringName + "::" + method.Name;
				}


				Conditions.RequiresNotNull(fullSym, nameof(fullSym));
				Global.Log.Debug("Symbol: {Sym}", fullSym);
				contexts.Add(fullSym);
			}

			Global.Log.Debug("SymCollect");
			var offsets = sym.SymCollect(contexts.ToArray());
			Global.Log.Debug("Offsets: {Count}", offsets);
			

			var addresses = Modules.GetFuncAddr(baseAttr.Module, offsets).ToArray();
			Conditions.Requires(addresses.Length == methods.Length);

			for (int i = 0; i < methods.Length; i++) {
				SetStableEntryPoint(methods[i], addresses[i].Address);
				Global.Log.Debug("Binding {Name} to {Addr}", methods[i].Name,
				                 addresses[i].ToString("P"));
			}
			
			sym.Dispose();
			Global.Log.Debug("Disposed");
		}

		public static void Bind(Type t)
		{
			throw new NotImplementedException();
			var methods = Runtime.GetMethods(t)
			                     .Where(x => x.GetCustomAttribute<SymcallAttribute>() != null)
			                     .ToArray();

			foreach (var method in methods) {
				var attr = method.GetCustomAttribute<SymcallAttribute>();
			}
		}
	}
}