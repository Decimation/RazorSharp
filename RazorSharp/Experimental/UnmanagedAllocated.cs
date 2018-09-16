#region

using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorSharp.Memory;
using RazorSharp.Utilities.Exceptions;

#endregion

namespace RazorSharp.Experimental
{

	/// <summary>
	///     Creates types in unmanaged memory. AllocExPointer can also be used.
	///     <para></para>
	///     Types that cannot be created in unmanaged memory:
	///     <para></para>
	///     - String
	///     <para></para>
	///     - IList
	///     <para></para>
	///     For that, use ExAllocExPointer.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal struct UnmanagedAllocated<T> where T : class
	{
		/// <summary>
		///     Types that can't be created in stack memory
		///     (out of the types that have been tested)
		/// </summary>
		private static readonly Type[] DisallowedTypes =
		{
			typeof(string),
			typeof(IList),
		};

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
			if (DisallowedTypes.Contains(typeof(T))) {
				throw new TypeException($"Type {typeof(T).Name} cannot be created in unmanaged memory.");
			}

			UnmanagedAllocated<T> unmanaged =
				new UnmanagedAllocated<T>(Marshal.AllocHGlobal(Unsafe.BaseInstanceSize<T>()),
					Activator.CreateInstance<T>());


			return unmanaged;
		}

		private T RewriteUnmanaged(T value)
		{
			// Get the memory of the managed object
			byte[] refMem = Unsafe.MemoryOf(ref value);

			// Make sure it's the correct size
			Debug.Assert(refMem.Length == Unsafe.BaseInstanceSize<T>());

			// Write the copied memory into unmanaged memory
			Mem.WriteBytes(m_unmanaged, refMem);

			// Set the reference to unmanaged memory (+IntPtr.Size to skip over the object header)
			Unsafe.WriteReference(ref value, m_unmanaged + IntPtr.Size);
			return value;
		}

		public override string ToString()
		{
			ConsoleTable table = new ConsoleTable("Field", "Value");
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