using System;

namespace RazorSharp.Pointers
{

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
	using Memory = Memory.Memory;

	/// <summary>
	/// A bare-bones, lighter type of Pointer, equal to the size of IntPtr.<para></para>
	/// Can be represented as a pointer in memory. <para></para>
	///
	///
	/// - No bounds checking<para></para>
	/// - No safety systems<para></para>
	/// - No type safety <para></para>
	/// </summary>
	/// <typeparam name="T">Type to point to</typeparam>
	public unsafe struct LightPointer<T> : IPointer<T>
	{
		/// <summary>
		/// We want this to be the only field so it can be represented as a pointer in memory.
		/// </summary>
		private void* m_value;

		#region Properties

		public T this[int index] {
			get => Memory.Read<T>(PointerUtils.Offset<T>(m_value, index), 0);
			set => Memory.Write(PointerUtils.Offset<T>(m_value, index), 0, value);
		}

		public T Value {
			get => Memory.Read<T>((IntPtr) m_value, 0);
			set => Memory.Write((IntPtr) m_value, 0, value);
		}

		public IntPtr Address {
			get => (IntPtr) m_value;
			set => m_value = (void*) value;
		}

		public int ElementSize => Unsafe.SizeOf<T>();

		#endregion

		#region Constructors

		public LightPointer(void* v)
		{
			m_value = v;
		}

		public LightPointer(long v)
		{
			m_value = (void*) v;
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

		#endregion


		#region Operators

		public static implicit operator LightPointer<T>(void* v)
		{
			return new LightPointer<T>(v);
		}

		public static implicit operator LightPointer<T>(IntPtr p)
		{
			return new LightPointer<T>(p.ToPointer());
		}

		#endregion

		#region Equality operators

		public bool Equals(LightPointer<T> other)
		{
			return m_value == other.m_value;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is LightPointer<T> && Equals((LightPointer<T>) obj);
		}

		public override int GetHashCode()
		{
			return unchecked((int) (long) m_value);
		}

		public static bool operator ==(LightPointer<T> left, LightPointer<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LightPointer<T> left, LightPointer<T> right)
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