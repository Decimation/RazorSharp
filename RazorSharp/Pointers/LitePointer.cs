using System;
using System.Collections;
using System.Globalization;
using RazorCommon;
using RazorCommon.Extensions;

namespace RazorSharp.Pointers
{

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
	using Memory = Memory.Memory;


	///  <summary>
	///  A bare-bones, lighter type of Pointer, equal to the size of IntPtr.<para></para>
	///  Can be represented as a pointer in memory. <para></para>
	///  - No bounds checking<para></para>
	///  - No safety systems<para></para>
	///  - No type safety <para></para>
	///  </summary>
	///  <typeparam name="T">Type to point to</typeparam>
	public unsafe struct LitePointer<T> : IPointer<T>, IFormattable
	{
		/// <summary>
		/// The address we're pointing to.<para></para>
		/// We want this to be the only field so it can be represented as a pointer in memory.
		/// </summary>
		private void* m_value;

		#region Properties

		public T this[int index] {
			get => Memory.Read<T>(PointerUtils.Offset<T>(m_value, index), 0);
			set => Memory.Write(PointerUtils.Offset<T>(m_value, index), 0, value);
		}

		public ref T Reference {
			get => ref Memory.AsRef<T>(Address);
		}

//		public IntPtr __this {
//			get => Unsafe.AddressOf(ref this);
//		}

		public T Value {
			get => Memory.Read<T>((IntPtr) m_value, 0);
			set => Memory.Write((IntPtr) m_value, 0, value);
		}

		public TNew As<TNew>()
		{
			return Memory.Read<TNew>(Address);
		}

		public IntPtr Address {
			get => (IntPtr) m_value;
			set => m_value = (void*) value;
		}

		public int ElementSize => Unsafe.SizeOf<T>();

		#endregion

		#region Constructors

		public LitePointer(void* v)
		{
			m_value = v;
		}

		public LitePointer(long v)
		{
			m_value = (void*) v;
		}

		public LitePointer(ref T t)
		{
			m_value = Unsafe.AddressOf(ref t).ToPointer();
		}

		#endregion

		#region Methods

		public int ToInt32()
		{
			return (int) m_value;
		}

		public long ToInt64()
		{
			return (long) m_value;
		}

		private void Increment(int cnt = 1)
		{
			m_value = PointerUtils.Offset<T>(m_value, cnt).ToPointer();
		}

		private void Decrement(int cnt = 1)
		{
			m_value = PointerUtils.Offset<T>(m_value, -cnt).ToPointer();
		}

		#endregion


		#region Operators

		public static implicit operator LitePointer<T>(void* v)
		{
			return new LitePointer<T>(v);
		}

		public static implicit operator LitePointer<T>(IntPtr p)
		{
			return new LitePointer<T>(p.ToPointer());
		}

		#region Arithmetic

		public static LitePointer<T> operator +(LitePointer<T> p, int i)
		{
			p.Increment(i);
			return p;
		}

		public static LitePointer<T> operator -(LitePointer<T> p, int i)
		{
			p.Decrement(i);
			return p;
		}

		public static LitePointer<T> operator ++(LitePointer<T> p)
		{
			p.Increment();
			return p;
		}

		public static LitePointer<T> operator --(LitePointer<T> p)
		{
			p.Decrement();
			return p;
		}

		#endregion

		#endregion

		#region Equality operators

		public bool Equals(LitePointer<T> other)
		{
			return m_value == other.m_value;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LitePointer<T> && Equals((LitePointer<T>) obj);
		}

		public override int GetHashCode()
		{
			return unchecked((int) (long) m_value);
		}

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
						return Collections.ListToString((IList) Value);
					}
					return Value.ToString();
				case "P":
					return Hex.ToHex(Address);
				default:
					goto case "O";
			}
		}

		public static bool operator ==(LitePointer<T> left, LitePointer<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LitePointer<T> left, LitePointer<T> right)
		{
			return !left.Equals(right);
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return Value.ToString();
		}

		#endregion


	}

}