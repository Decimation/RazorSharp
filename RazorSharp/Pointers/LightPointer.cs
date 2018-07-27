using System;

namespace RazorSharp.Pointers
{

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
	using Memory = Memory.Memory;

	/// <summary>
	/// A lighter type of pointer, equal to the size of IntPtr.
	///
	/// - No bounds checking
	/// - No safety systems
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public unsafe struct LightPointer<T> : IPointer<T>
	{
		/// <summary>
		/// We want this to be the only field.
		/// </summary>
		private void* m_value;

		public T this[int index] {
			get => Memory.Read<T>(Unsafe.Offset<T>(m_value, index), 0);
			set => Memory.Write(Unsafe.Offset<T>(m_value, index), 0, value);
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

		public int ToInt32()
		{
			return (int) m_value;
		}

		public long ToInt64()
		{
			return (long) m_value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

}