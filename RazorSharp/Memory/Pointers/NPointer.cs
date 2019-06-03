using System;
using System.Runtime.CompilerServices;
using RazorSharp.CoreClr;

namespace RazorSharp.Memory.Pointers
{
	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion

	public unsafe class NPointer<T> : IFormattable
	{
		private void* m_value;

		#region Accessors

		public IntPtr Address {
			get => (IntPtr) m_value;
			set => m_value = (void*) value;
		}

		/// <summary>
		///     Indexes <see cref="Address" /> as a reference.
		/// </summary>
		public ref T this[int index] => ref AsRef(index);

		/// <summary>
		///     Returns the value as a reference.
		/// </summary>
		public ref T Reference => ref AsRef();

		/// <summary>
		///     Dereferences the pointer as the specified type.
		/// </summary>
		public T Value {
			get => Read();
			set => Write(value);
		}

		/// <summary>
		///     Size of type <typeparamref name="T" />.
		/// </summary>
		public int ElementSize => Unsafe.SizeOf<T>();

		/// <summary>
		///     Whether <see cref="Address" /> is <c>null</c> (<see cref="IntPtr.Zero" />).
		/// </summary>
		public bool IsNull => m_value == null;

		/// <summary>
		///     Whether the value being pointed to is <c>default</c> or <c>null</c> bytes,
		/// or this pointer is <c>null</c>.
		/// </summary>
		public bool IsNil => RtInfo.IsNil(Reference);
		
		#endregion

		#region Read / write

		#region AsRef

		public ref T AsRef() => ref CSUnsafe.AsRef<T>(m_value);

		public ref T AsRef(int elemCnt)
		{
			return ref CSUnsafe.AsRef<T>(OffsetFast(elemCnt));
		}

		#endregion

		#region Read

		public T Read() => CSUnsafe.Read<T>(m_value);

		public T Read(int elemCnt)
		{
			return CSUnsafe.Read<T>(OffsetFast(elemCnt));
		}

		#endregion

		#region Write

		public void Write(T value) => CSUnsafe.Write<T>(m_value, value);

		public void Write(T value, int elemCnt)
		{
			CSUnsafe.Write<T>(OffsetFast(elemCnt), value);
		}

		#endregion

		#endregion

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void* OffsetFast(int elemCnt) => (void*) ((long) m_value + Mem.CompleteSize<T>(elemCnt));

		public string ToString(string format, IFormatProvider formatProvider)
		{
			throw new NotImplementedException();
		}
	}
}