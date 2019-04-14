using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorSharp.CorJit;
using RazorSharp.Native;
using RazorSharp.Native.Win32;

namespace RazorSharp
{
	public unsafe class CompilerHook
	{
		public CorJitCompiler.CompileMethodDel Compile = null;

		private readonly IntPtr               pJit;
		private readonly IntPtr               pVTable;
		private          bool                 isHooked = false;
		private readonly CorJitCompilerNative compiler;
		private          MemoryProtection     lpflOldProtect;

		public CompilerHook()
		{
			if (pJit == IntPtr.Zero) pJit = CorJitCompiler.GetJit();
			Debug.Assert(pJit != null);
			compiler = Marshal.PtrToStructure<CorJitCompilerNative>(Marshal.ReadIntPtr(pJit));
			Debug.Assert(compiler.CompileMethod != null);
			pVTable = Marshal.ReadIntPtr(pJit);

			RuntimeHelpers.PrepareMethod(GetType().GetMethod(nameof(RemoveHook)).MethodHandle);
			RuntimeHelpers.PrepareMethod(
				GetType().GetMethod(nameof(LockpVTable), BindingFlags.Instance | BindingFlags.NonPublic).MethodHandle);
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

		public bool Hook(CorJitCompiler.CompileMethodDel hook)
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