using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using RazorCommon;

namespace RazorSharp.Pointers
{

	/// <summary>
	/// Represents a C/C++ style array using dynamic unmanaged memory allocation
	///
	/// - No bounds checking
	/// - Resizable
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed unsafe class AllocPointer<T> : Pointer<T>, IDisposable
	{
		private class AllocPointerMetadata : PointerMetadata
		{
			protected internal bool IsAllocated { get; set; }

			protected internal int AllocatedSize { get; set; }


			protected internal AllocPointerMetadata(int elementSize, bool allocated, int allocSize) : base(elementSize)
			{
				IsAllocated   = allocated;
				AllocatedSize = allocSize;
			}
		}

		#region Accessors

		public bool IsAllocated {
			get => Metadata.IsAllocated;
			private set => Metadata.IsAllocated = value;
		}

		private AllocPointerMetadata Metadata {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ((AllocPointerMetadata) m_metadata);
		}

		public int AllocatedSize {
			get => Metadata.AllocatedSize;
			set {
				Address                = Marshal.ReAllocHGlobal(Address, (IntPtr) value);
				Metadata.AllocatedSize = value;
			}
		}

		public override IntPtr Address {
			get => IsAllocated ? base.Address : IntPtr.Zero;
			set {
				if (IsAllocated) {
					base.Address = value;
				}
			}
		}

		public override T Value {
			get { return IsAllocated ? base.Value : default; }
			set {
				if (IsAllocated) {
					base.Value = value;
				}
			}
		}

		public override T this[int index] {
			get => IsAllocated ? base[index] : default;
			set {
				if (IsAllocated) {
					base[index] = value;
				}
			}
		}

		public int Count {
			get => AllocatedSize / Metadata.ElementSize;
			set => AllocatedSize = value * Metadata.ElementSize;
		}

		#endregion

		#region Constructors

		public AllocPointer(int elements) : this(Marshal.AllocHGlobal(elements * Unsafe.SizeOf<T>()),
			elements * Unsafe.SizeOf<T>()) { }

		public AllocPointer(IntPtr p) : this(p, Unsafe.SizeOf<T>())
		{

		}

		private AllocPointer(IntPtr p, int bytesAlloc) : base(p,
			new AllocPointerMetadata(Unsafe.SizeOf<T>(), true, bytesAlloc))
		{
			//Zero();
		}

		#endregion

		#region Operators

		#region Arithmetic

		public static AllocPointer<T> operator +(AllocPointer<T> p, int i)
		{
			p.Increment(i);
			return p;
		}

		public static AllocPointer<T> operator -(AllocPointer<T> p, int i)
		{
			p.Decrement(i);
			return p;
		}

		public static AllocPointer<T> operator ++(AllocPointer<T> p)
		{
			p.Increment();
			return p;
		}

		public static AllocPointer<T> operator --(AllocPointer<T> p)
		{
			p.Decrement();
			return p;
		}

		#endregion

		#endregion

		#region Overrides and methods

		protected override ConsoleTable ToTable()
		{
			var table = base.ToTable();
			table.AddRow("Allocated", IsAllocated);
			table.AddRow("Allocated bytes", AllocatedSize);
			table.AddRow("Count", Count);
			return table;
		}

		public void Zero()
		{
			Memory.Memory.Zero(Address, AllocatedSize);
		}

		private void ReleaseUnmanagedResources()
		{
			Marshal.FreeHGlobal(Address);
			Metadata.AllocatedSize = 0;
			IsAllocated            = false;
			base.Address           = IntPtr.Zero;
		}

		public void Dispose()
		{
			Logger.Log(Flags.Memory, "Freeing {0} bytes @ {1:P}", AllocatedSize, Address);
			ReleaseUnmanagedResources();
			GC.SuppressFinalize(this);
		}

		public override string ToString()
		{
			return base.ToString();
		}

		~AllocPointer()
		{
			//ReleaseUnmanagedResources();
		}



		#endregion

	}

}