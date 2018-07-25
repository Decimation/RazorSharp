using System;
using System.Runtime.InteropServices;

namespace RazorSharp.Pointers
{

	public class StackArrayPointer<T> : Pointer<T>
	{
		/// <summary>
		/// Address of the type on the stack.
		/// </summary>
		private readonly IntPtr m_stack;

		private IntPtr Heap {
			get => Marshal.ReadIntPtr(m_stack);
		}



		private void UpdateHeap()
		{

		}




		public StackArrayPointer(ref T t) : base(ref t)
		{
			m_stack = Unsafe.AddressOf(ref t);
		}
	}

}