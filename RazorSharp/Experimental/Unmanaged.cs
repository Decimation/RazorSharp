using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorSharp.Pointers;
using RazorSharp.Runtime.CLRTypes;

namespace RazorSharp.Experimental
{
	using Memory = Memory.Memory;
	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	//todo: WIP
	public unsafe struct Unmanaged<T> where T : class
	{
		/// <summary>
		/// Pointer to the reference
		/// </summary>
		private IntPtr m_stack;

		/// <summary>
		/// Pointer to the heap object
		/// </summary>
		private IntPtr m_heap;

		private Unmanaged(IntPtr stack, IntPtr heap)
		{
			m_stack = stack;
			m_heap = heap;
		}

		public void Free()
		{
			Marshal.FreeHGlobal(m_heap);
			Marshal.FreeHGlobal(m_stack);
		}

		public T Value {
			get => Memory.Read<T>(m_stack, 0);
			set => Memory.Write(m_stack, 0, value);
		}

		public static Unmanaged<T> Allocate()
		{
			// The minimum heap size of this type
			int size = Unsafe.BaseInstanceSize<T>();

			// Allocate the size of a pointer
			IntPtr unmanagedStackPtr = Marshal.AllocHGlobal(IntPtr.Size);

			IntPtr unmanagedHeapPtr = Marshal.AllocHGlobal(size);
			Memory.Zero(unmanagedHeapPtr,size);



			Memory.Write(unmanagedHeapPtr, -1, new ObjHeader());
			Memory.Write(unmanagedHeapPtr, 0, (long) Runtime.Runtime.MethodTableOf<T>());


			Memory.Write(unmanagedStackPtr,0, unmanagedHeapPtr);

			typeof(T).TypeInitializer.Invoke(Memory.Read<T>(unmanagedStackPtr,0), null);


			Debug.Assert(unmanagedStackPtr != IntPtr.Zero);
			Debug.Assert(unmanagedHeapPtr != IntPtr.Zero);
			return new Unmanaged<T>(unmanagedStackPtr, unmanagedHeapPtr);
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

}