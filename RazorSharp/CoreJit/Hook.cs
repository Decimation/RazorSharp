#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorSharp.Native;
using RazorSharp.Native.Win32;

#endregion

namespace RazorSharp.CoreJit
{
	internal class CompilerHook
	{
		private readonly CorJitCompilerNative m_compiler;

		private readonly IntPtr                          m_pJit;
		private readonly IntPtr                          m_pVTable;
		internal         CorJitCompiler.CompileMethod Compile;
		private          bool                            m_isHooked;
		private          MemoryProtection                m_lpflOldProtect;

		internal CompilerHook()
		{
			if (m_pJit == IntPtr.Zero) m_pJit = CorJitCompiler.GetJit();
			Debug.Assert(m_pJit != null);
			m_compiler = Marshal.PtrToStructure<CorJitCompilerNative>(Marshal.ReadIntPtr(m_pJit));
			Debug.Assert(m_compiler.CompileMethod != null);
			m_pVTable = Marshal.ReadIntPtr(m_pJit);

			RuntimeHelpers.PrepareMethod(GetType().GetMethod(nameof(RemoveHook), BindingFlags.NonPublic | BindingFlags.Instance).MethodHandle);
			RuntimeHelpers.PrepareMethod(
				GetType().GetMethod(nameof(LockVTable), BindingFlags.Instance | BindingFlags.NonPublic).MethodHandle);
		}

		private bool UnlockVTable()
		{
			if (!Kernel32.VirtualProtect(m_pVTable, (uint) IntPtr.Size, MemoryProtection.ExecuteReadWrite,
			                             out m_lpflOldProtect)) {
				Console.WriteLine(new Win32Exception(Marshal.GetLastWin32Error()).Message);
				return false;
			}

			return true;
		}

		private bool LockVTable()
		{
			return Kernel32.VirtualProtect(m_pVTable, (uint) IntPtr.Size, m_lpflOldProtect, out m_lpflOldProtect);
		}

		internal bool Hook(CorJitCompiler.CompileMethod hook)
		{
			if (!UnlockVTable()) return false;

			Compile = m_compiler.CompileMethod;
			Debug.Assert(Compile != null);

			RuntimeHelpers.PrepareDelegate(hook);
			RuntimeHelpers.PrepareDelegate(Compile);

			Marshal.WriteIntPtr(m_pVTable, Marshal.GetFunctionPointerForDelegate(hook));

			return m_isHooked = LockVTable();
		}

		internal bool RemoveHook()
		{
			if (!m_isHooked) throw new InvalidOperationException("Impossible unhook not hooked compiler");
			if (!UnlockVTable()) return false;

			Marshal.WriteIntPtr(m_pVTable, Marshal.GetFunctionPointerForDelegate(Compile));

			return LockVTable();
		}
	}
}