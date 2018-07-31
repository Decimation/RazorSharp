using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RazorCommon;

namespace RazorSharp.Experimental
{

	public struct UnmanagedAllocated<T> where T : class
	{
		private readonly IntPtr m_unmanaged;

		private T m_dummy;

		public T Value {
			get => m_dummy;
			set => m_dummy = RewriteUnmanaged(value);
		}

		private UnmanagedAllocated(IntPtr p, T value)
		{
			m_unmanaged = p;
			m_dummy     = value;
			m_dummy     = RewriteUnmanaged(value);
		}

		public static UnmanagedAllocated<T> Alloc()
		{
			UnmanagedAllocated<T> unmanaged =
				new UnmanagedAllocated<T>(Marshal.AllocHGlobal(Unsafe.BaseInstanceSize<T>()),
					Activator.CreateInstance<T>());


			return unmanaged;
		}

		private T RewriteUnmanaged(T value)
		{
			// Get the memory of the managed object
			var refMem = Unsafe.MemoryOf(ref value);

			// Make sure it's the correct size
			Debug.Assert(refMem.Length == Unsafe.BaseInstanceSize<T>());

			// Write the copied memory into unmanaged memory
			Memory.Memory.WriteBytes(m_unmanaged, refMem);

			// Set the reference to unmanaged memory (+IntPtr.Size to skip over the object header)
			Unsafe.WriteReference(ref value, m_unmanaged + IntPtr.Size);
			return value;
		}

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Value", m_dummy);
			table.AddRow("Unmanaged", Hex.ToHex(m_unmanaged));
			table.AddRow("Dummy heap pointer", Hex.ToHex(Unsafe.AddressOfHeap(ref m_dummy)));
			return table.ToMarkDownString();
		}

		public void Free()
		{
			Marshal.FreeHGlobal(m_unmanaged);
		}
	}

}