#region

using System;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Extensions;

#endregion

namespace RazorSharp.Pointers
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
	using MMemory = Memory.Memory;

	#endregion


	///  <summary>
	///  A bare-bones, lighter type of ExPointer, equal to the size of IntPtr.<para></para>
	///  Can be represented as a pointer in memory. <para></para>
	///  - No bounds checking<para></para>
	///  - No safety systems<para></para>
	///  - No type safety <para></para>
	///  </summary>
	///  <typeparam name="T">Type to point to</typeparam>
	public unsafe struct Pointer<T> : IPointer<T>, IFormattable
	{
		/// <summary>
		/// The address we're pointing to.<para></para>
		/// We want this to be the only field so it can be represented as a pointer in memory.
		/// </summary>
		private void* m_value;

		#region Properties

		/*public T this[int index] {
			get => MMemory.Read<T>(PointerUtils.Offset<T>(m_value, index));
			set => MMemory.Write(PointerUtils.Offset<T>(m_value, index), 0, value);
		}*/

		public ref T this[int index] {
			get { return ref MMemory.AsRef<T>(PointerUtils.Offset<T>(Address, index)); }
		}

		public ref T Reference => ref MMemory.AsRef<T>(Address);

		public T Value {
			get => MMemory.Read<T>((IntPtr) m_value, 0);
			set => MMemory.Write((IntPtr) m_value, 0, value);
		}

		public IntPtr Address {
			get => (IntPtr) m_value;
			set => m_value = (void*) value;
		}

		public int ElementSize => Unsafe.SizeOf<T>();

		public bool IsNull => m_value == null;

		public bool IsAligned => MMemory.IsAligned(Address);

		#endregion

		#region Constructors

		public Pointer(IntPtr p) : this(p.ToPointer()) { }

		public Pointer(void* v)
		{
			m_value = v;
		}

		public Pointer(long v)
		{
			m_value = (void*) v;
		}

		public Pointer(ref T t)
		{
			m_value = Unsafe.AddressOf(ref t).ToPointer();
		}

		#endregion

		public IntPtr MoveDown()
		{
			var oldAddr = Address;
			Address = Marshal.ReadIntPtr(Address);
			return oldAddr;
		}

		public void Write<TType>(TType t, int byteOffset = 0)
		{
			MMemory.Write(Address, byteOffset, t);
		}

		public TType Read<TType>(int byteOffset = 0)
		{
			return MMemory.Read<TType>(Address, byteOffset);
		}

		#region Methods

		public Pointer<TNew> Reinterpret<TNew>()
		{
			return new Pointer<TNew>(Address);
		}

		public int ToInt32()
		{
			return (int) m_value;
		}

		public long ToInt64()
		{
			return (long) m_value;
		}

		/// <summary>
		/// Add the specified number of bytes to the address
		/// </summary>
		/// <param name="bytes">Number of bytes to add</param>
		public void Add(int bytes)
		{
			m_value = PointerUtils.Add(m_value, bytes).ToPointer();
		}

		/// <summary>
		/// Subtract the specified number of bytes from the address
		/// </summary>
		/// <param name="bytes">Number of bytes to subtract</param>
		public void Subtract(int bytes)
		{
			m_value = PointerUtils.Subtract(m_value, bytes).ToPointer();
		}

		/// <summary>
		/// Increment the address by the specified number of elements
		/// </summary>
		/// <param name="elemCnt">Number of elements</param>
		private void Increment(int elemCnt = 1)
		{
			m_value = PointerUtils.Offset<T>(m_value, elemCnt).ToPointer();
		}

		/// <summary>
		/// Decrement the address by the specified number of elements
		/// </summary>
		/// <param name="elemCnt">Number of elements</param>
		private void Decrement(int elemCnt = 1)
		{
			m_value = PointerUtils.Offset<T>(m_value, -elemCnt).ToPointer();
		}

		#endregion

		#region Operators

		public static implicit operator Pointer<T>(void* v)
		{
			return new Pointer<T>(v);
		}

		public static implicit operator Pointer<T>(IntPtr p)
		{
			return new Pointer<T>(p.ToPointer());
		}

		public static explicit operator void*(Pointer<T> ptr)
		{
			return ptr.Address.ToPointer();
		}


		#region Arithmetic

		public static Pointer<T> operator +(Pointer<T> p, int i)
		{
			p.Increment(i);
			return p;
		}

		public static Pointer<T> operator -(Pointer<T> p, int i)
		{
			p.Decrement(i);
			return p;
		}

		public static Pointer<T> operator ++(Pointer<T> p)
		{
			p.Increment();
			return p;
		}

		public static Pointer<T> operator --(Pointer<T> p)
		{
			p.Decrement();
			return p;
		}

		#endregion

		#endregion

		#region Equality operators

		public bool Equals(Pointer<T> other)
		{
			return m_value == other.m_value;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Pointer<T> && Equals((Pointer<T>) obj);
		}

		public override int GetHashCode()
		{
			return unchecked((int) (long) m_value);
		}

		public static bool operator ==(Pointer<T> left, Pointer<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Pointer<T> left, Pointer<T> right)
		{
			return !left.Equals(right);
		}

		#endregion

		#region Overrides

		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrEmpty(format)) format   = "O";
			if (formatProvider == null) formatProvider = CultureInfo.CurrentCulture;


			/**
			 * @O	Object
			 * @P	Pointer
			 */
			switch (format.ToUpperInvariant()) {
				case "O":
					if (typeof(T).IsIListType()) {
						return Collections.ListToString((IList) Reference);
					}

					return Reference.ToString();
				case "P":
					return Hex.ToHex(Address);
				default:
					goto case "O";
			}
		}

		public override string ToString()
		{
			return Reference.ToString();
		}

		#endregion


	}

}