#region

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RazorSharp.Native;
using RazorSharp.Native.Enums;

#endregion

namespace RazorSharp
{
	public class AssemblyHandle : IDisposable
	{
		private readonly IntPtr m_ptr;
		private readonly int    m_size;

		public AssemblyHandle(int size)
		{
			m_size = size;

			m_ptr = Kernel32.VirtualAlloc(IntPtr.Zero, (UIntPtr) m_size, AllocationType.Commit,
			                              MemoryProtection.ExecuteReadWrite);

			IsAllocated = true;
		}

		public bool IsAllocated { get; private set; }

		public void Dispose()
		{
			Debug.Assert(IsAllocated);
			Debug.Assert(Kernel32.VirtualFree(m_ptr, (uint) m_size, FreeTypes.Decommit));
			IsAllocated = false;
		}

		public TDelegate Write<TDelegate>(params byte[] opCodes) where TDelegate : Delegate
		{
			Debug.Assert(opCodes.Length <= m_size);
			Marshal.Copy(opCodes, 0, m_ptr, opCodes.Length);
			return Marshal.GetDelegateForFunctionPointer<TDelegate>(m_ptr);
		}
	}
}