using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using RazorSharp.Clr;
using RazorSharp.Memory.Calling.Sym.Attributes;
using RazorSharp.Native;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

namespace RazorSharp.Memory.Calling.Sym
{
	public static unsafe class Symcall
	{
		internal delegate int SetStableEntryPointInterlockedDelegate(void* __this, IntPtr pCode);
		internal delegate int SetStableEntryPointInterlockedDelegate32(int __this, int pCode);
		internal delegate IntPtr GetMultiCallableAddrOfCodeDelegate(void* __this, int x);

		private static SetStableEntryPointInterlockedDelegate _setStableEntryPointInterlocked;
		private static SetStableEntryPointInterlockedDelegate32 _setStableEntryPointInterlocked32;
		internal static Pointer<byte> GetClrFunctionAddress(string name)
		{
			return Symbolism.GetSymAddress(Symbolism.CLR_PDB, Clr.Clr.CLR_DLL, name);
		}

		internal static void Setup()
		{
			if (!Environment.Is64BitProcess) {
				const string opCodes = "55 8B EC 53 56 57 8B D9 E8 11 68 F8 FF 8B CB 8B F8 E8 57 41 F8 FF 8B 75 8 8B";
				var          ss      = new SigScanner();
				ss.SelectModule("clr.dll");
				Pointer<byte> ptr = ss.FindPattern(opCodes);
				Console.WriteLine(ptr);
				var ofs = ptr.Address.ToInt64() - Modules.GetModule("clr.dll").BaseAddress.ToInt64();

				_setStableEntryPointInterlocked32 =
					Marshal.GetDelegateForFunctionPointer<SetStableEntryPointInterlockedDelegate32>(ptr.Address);
			}
			else {
				var fn = GetClrFunctionAddress("MethodDesc::SetStableEntryPointInterlocked").Address;

				_setStableEntryPointInterlocked =
					Marshal.GetDelegateForFunctionPointer<SetStableEntryPointInterlockedDelegate>(fn);
			}
		}

		[HandleProcessCorruptedStateExceptions]
		internal static void SetStableEntryPoint(MethodInfo mi, IntPtr pCode)
		{
			try
			{
				var pMd = mi.MethodHandle.Value;
				if (!Environment.Is64BitProcess)
				{
					
					_setStableEntryPointInterlocked32(pMd.ToInt32(), pCode.ToInt32());
				}
				else
				{
					_setStableEntryPointInterlocked(pMd.ToPointer(), pCode);
				}
				
			}
			catch (SEHException x) {
				Global.Log.Error("SEHException");
				Global.Log.Error("Error code {Code}", x.ErrorCode);
				Global.Log.Debug("HResult {HR}", x.HResult.ToString("X"));
				Console.WriteLine(x.Source);
				Console.WriteLine(x.StackTrace);

				throw;
			}
		}

		private const string SCOPE_RESOLUTION_OPERATOR = "::";

		
		
		public static void BindQuick(Type t)
		{
			var methods = Runtime.GetMethods(t)
			                     .Where(x => x.GetCustomAttribute<SymcallAttribute>() != null)
			                     .ToArray();
			if (methods.Length == 0) {
				return;
			}
//			Global.Log.Debug("Detected {Count} decorated methods", methods.Length);
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
				SetStableEntryPoint(methods[i], addr);
			}

			sym.Dispose();
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