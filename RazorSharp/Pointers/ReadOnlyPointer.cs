#region

using System;
using System.Runtime.CompilerServices;
using RazorSharp.Memory;

#endregion

namespace RazorSharp.Pointers
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion

	// todo: WIP
	// A lot of methods and properties are just copied from Pointer because
	// we can only use interfaces with structs
	internal unsafe struct ReadOnlyPointer<T> : IPointer<T>
	{
		private readonly void* m_pValue;

		public string ToString(string format, IFormatProvider formatProvider)
		{
			throw new NotImplementedException();
		}

		#region Properties

		public ref T this[int index] => ref AsRef<T>(index);

		public ref T Reference => ref AsRef<T>();

		public T Value {
			get => Read<T>();
			set => Write(value);
		}

		public IntPtr Address {
			get => (IntPtr) m_pValue;
			set { }
		}

		public int ElementSize => Unsafe.SizeOf<T>();

		public bool IsNull => m_pValue == null;

		public bool IsAligned => Mem.IsAligned(Address);

		#endregion

		public ReadOnlyPointer(void* pValue)
		{
			m_pValue = pValue;
		}

		public ReadOnlyPointer(IntPtr p) : this(p.ToPointer()) { }

		public ReadOnlyPointer(long l) : this((IntPtr) l) { }

		public ReadOnlyPointer(ref T t) : this(Unsafe.AddressOf(ref t).Address) { }

		public int ToInt32()
		{
			return Address.ToInt32();
		}

		public long ToInt64()
		{
			return Address.ToInt64();
		}

		public uint ToUInt32()
		{
			return (uint) Address.ToInt32();
		}

		public ulong ToUInt64()
		{
			return (ulong) Address.ToInt64();
		}

		public void* ToPointer()
		{
			return m_pValue;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IntPtr Offset(int elemCnt)
		{
			return PointerUtils.Offset<T>(Address, elemCnt);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IntPtr Offset<TType>(int elemCnt)
		{
			return PointerUtils.Offset<TType>(Address, elemCnt);
		}

		public void Write<TType>(TType t, int elemOffset = 0)
		{
			Mem.Write(Offset<TType>(elemOffset), 0, t);
		}

		public void ForceWrite<TType>(TType t, int elemOffset = 0)
		{
			Mem.ForceWrite(Offset<TType>(elemOffset), 0, t);
		}

		public TType Read<TType>(int elemOffset = 0)
		{
			return Mem.Read<TType>(Offset<TType>(elemOffset));
		}


		public ref TType AsRef<TType>(int elemOffset = 0)
		{
			return ref Mem.AsRef<TType>(Offset<TType>(elemOffset));
		}

		public bool Equals(Pointer<T> other)
		{
			throw new NotImplementedException();
		}

		public bool Equals(IPointer<T> other)
		{
			return other.Address == Address;
		}

		public ReadOnlyPointer<TNew> Reinterpret<TNew>()
		{
			return new ReadOnlyPointer<TNew>(Address);
		}
	}

}