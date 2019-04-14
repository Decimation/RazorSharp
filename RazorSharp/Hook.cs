using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorSharp.Native;
using RazorSharp.Native.Win32;

namespace RazorSharp
{
	public unsafe class CompilerHook
	{
		public Jit.CorJitCompiler.CompileMethodDel Compile = null;

		private          IntPtr                                  pJit;
		private          IntPtr                                  pVTable;
		private          bool                                    isHooked = false;
		private readonly Jit.CorJitCompiler.CorJitCompilerNative compiler;
		private          MemoryProtection                        lpflOldProtect;

		public CompilerHook()
		{
			if (pJit == IntPtr.Zero) pJit = Jit.CorJitCompiler.GetJit();
			Debug.Assert(pJit != null);
			compiler = Marshal.PtrToStructure<Jit.CorJitCompiler.CorJitCompilerNative>(Marshal.ReadIntPtr(pJit));
			Debug.Assert(compiler.CompileMethod != null);
			pVTable = Marshal.ReadIntPtr(pJit);

			RuntimeHelpers.PrepareMethod(GetType().GetMethod("RemoveHook").MethodHandle);
			RuntimeHelpers.PrepareMethod(GetType().GetMethod("LockpVTable",
			                                                 System.Reflection.BindingFlags.Instance |
			                                                 System.Reflection.BindingFlags.NonPublic).MethodHandle);
		}

		private bool UnlockpVTable()
		{
			if (!Kernel32.VirtualProtect(pVTable, (uint) IntPtr.Size, MemoryProtection.ExecuteReadWrite,
			                             out lpflOldProtect)) {
				Console.WriteLine(new Win32Exception(Marshal.GetLastWin32Error()).Message);
				return false;
			}

			return true;
		}

		private bool LockpVTable()
		{
			return Kernel32.VirtualProtect(pVTable, (uint) IntPtr.Size, lpflOldProtect, out lpflOldProtect);
		}

		public bool Hook(Jit.CorJitCompiler.CompileMethodDel hook)
		{
			if (!UnlockpVTable()) return false;

			Compile = compiler.CompileMethod;
			Debug.Assert(Compile != null);

			RuntimeHelpers.PrepareDelegate(hook);
			RuntimeHelpers.PrepareDelegate(Compile);

			Marshal.WriteIntPtr(pVTable, Marshal.GetFunctionPointerForDelegate(hook));

			return isHooked = LockpVTable();
		}

		public bool RemoveHook()
		{
			if (!isHooked) throw new InvalidOperationException("Impossible unhook not hooked compiler");
			if (!UnlockpVTable()) return false;

			Marshal.WriteIntPtr(pVTable, Marshal.GetFunctionPointerForDelegate(Compile));

			return LockpVTable();
		}
	}
}