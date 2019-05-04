using System;

// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace RazorSharp.Memory.Pointers
{
	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion

	public unsafe struct FastPointer<T>
	{
		private void* m_value;

		#region Accessors

		public T Value {
			get => Read();
			set => Write(value);
		}

		public ref T Reference => ref AsRef();

		public ref T this[int index] => ref AsRef(index);

		public IntPtr Address {
			get => (IntPtr) m_value;
			set => m_value = (void*) value;
		}

		public int ElementSize => Unsafe.SizeOf<T>();

		#endregion

		#region Constructors

		public FastPointer(IntPtr value) : this(value.ToPointer()) { }

		public FastPointer(void* value)
		{
			m_value = value;
		}

		public FastPointer(long value) : this((void*) value) { }

		public FastPointer(ulong value) : this((void*) value) { }

		public FastPointer(ref T value) : this(Unsafe.AddressOf(ref value).Address) { }

		#endregion

		// (void*) (((long) m_value) + byteOffset)
		// (void*) (((long) m_value) + (elemOffset * ElementSize))

		#region Read

		public T Read()
		{
			return CSUnsafe.Read<T>(m_value);
		}

		public T Read(int elemOffset)
		{
			return CSUnsafe.Read<T>((void*) (((long) m_value) + (elemOffset * ElementSize)));
		}

		#endregion

		#region Write

		public void Write(T value)
		{
			CSUnsafe.Write(m_value, value);
		}

		public void Write(T value, int elemOffset)
		{
			CSUnsafe.Write<T>((void*) (((long) m_value) + (elemOffset * ElementSize)), value);
		}

		#endregion

		#region Reference

		public ref T AsRef()
		{
			return ref CSUnsafe.AsRef<T>(m_value);
		}

		public ref T AsRef(int elemOffset)
		{
			return ref CSUnsafe.AsRef<T>((void*) (((long) m_value) + (elemOffset * ElementSize)));
		}

		#endregion

		#region Other

		public FastPointer<TNew> Cast<TNew>() => new FastPointer<TNew>(Address);

		#endregion

		#region Arithmetic

		public FastPointer<T> Increment(int elemCnt = 1)
		{
			m_value = (void*) (((long) m_value) + (elemCnt * ElementSize));
			return this;
		}


		/// <summary>
		///     Decrement <see cref="Address" /> by the specified number of elements
		/// </summary>
		/// <param name="elemCnt">Number of elements</param>
		/// <returns>
		///     <c>this</c>
		/// </returns>
		public FastPointer<T> Decrement(int elemCnt = 1) => Increment(-elemCnt);

		#endregion

		#region Operators

		public static FastPointer<T> operator ++(FastPointer<T> p)
		{
			p.Increment();
			return p;
		}

		/// <summary>
		///     Decrements the <see cref="Pointer{T}" /> by one element.
		/// </summary>
		/// <param name="p">
		///     <see cref="Pointer{T}" />
		/// </param>
		/// <returns>The offset <see cref="Address" /></returns>
		public static FastPointer<T> operator --(FastPointer<T> p)
		{
			p.Decrement();
			return p;
		}

		#endregion
	}
}