#region

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Extensions;
using RazorSharp.Experimental;
using RazorSharp.Pointers.Ex;
using RazorSharp.Utilities;

#endregion

namespace RazorSharp.Pointers
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
	using MMemory = Memory.Memory;

	#endregion


	/// <summary>
	///     <para>A bare-bones, lighter type of <see cref="ExPointer{T}" />, equal to <see cref="IntPtr.Size" /></para>
	///     <para> Can be represented as a pointer in memory. </para>
	///     <list type="bullet">
	///         <item>
	///             <description>No bounds checking</description>
	///         </item>
	///         <item>
	///             <description>No safety systems</description>
	///         </item>
	///         <item>
	///             <description>No type safety</description>
	///         </item>
	///     </list>
	/// </summary>
	/// <typeparam name="T">Type to point to</typeparam>
	public unsafe struct Pointer<T> : IPointer<T>, IFormattable
	{
		/// <summary>
		///     <para>The address we're pointing to.</para>
		///     <para>We want this to be the only field so it can be represented as a pointer in memory.</para>
		/// </summary>
		private void* m_value;

		#region Properties

		/*public T this[int index] {
			get => MMemory.Read<T>(PointerUtils.Offset<T>(m_value, index));
			set => MMemory.Write(PointerUtils.Offset<T>(m_value, index), 0, value);
		}*/

		public ref T this[int index] => ref AsRef<T>(index);

		public ref T Reference => ref AsRef<T>();

		public T Value {
			get => Read<T>();
			set => Write(value);
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


		public PinHandle Pin()
		{
			RazorContract.Requires(!typeof(T).IsValueType, "Value types do not need to be pinned");
			return new ObjectPinHandle(Value);
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

		public ConsoleTable ToTable(int elemCnt)
		{
			ConsoleTable table;

			if (typeof(T).IsValueType) {
				table = new ConsoleTable("Address", "Offset", "Value");
			}
			else {
				table = new ConsoleTable("Address", "Offset", "Pointer", "Value");
			}

			for (int i = 0; i < elemCnt; i++) {
				table.AddRow(Hex.ToHex(Offset(i)), i, Hex.ToHex(Read<long>(i)), this[i]);
			}

			return table;
		}

		public void Init(params T[] values)
		{
			for (int i = 0; i < values.Length; i++) {
				this[i] = values[i];
			}
		}

		public IntPtr MoveDown()
		{
			IntPtr oldAddr = Address;
			Address = Marshal.ReadIntPtr(Address);
			return oldAddr;
		}

		/// <summary>
		///     Write to <see cref="Address" />
		/// </summary>
		/// <param name="t">Value to write</param>
		/// <param name="elemOffset">Element offset</param>
		/// <typeparam name="TType">Type to write</typeparam>
		public void Write<TType>(TType t, int elemOffset = 0)
		{
			MMemory.Write(Offset<TType>(elemOffset), 0, t);
		}

		/// <summary>
		///     Read from <see cref="Address" />
		/// </summary>
		/// <param name="elemOffset">Element offset</param>
		/// <typeparam name="TType">Type to read</typeparam>
		/// <returns>The value read from the offset <see cref="Address" /></returns>
		public TType Read<TType>(int elemOffset = 0)
		{
			return MMemory.Read<TType>(Offset<TType>(elemOffset));
		}


		public ref TType AsRef<TType>(int elemOffset = 0)
		{
			return ref MMemory.AsRef<TType>(Offset<TType>(elemOffset));
		}

		#region Methods

		public Pointer<TNew> Reinterpret<TNew>()
		{
			return new Pointer<TNew>(Address);
		}

		public void* ToPointer()
		{
			return m_value;
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
		///     Increment the <see cref="Address" /> by the specified number of bytes
		/// </summary>
		/// <param name="bytes">Number of bytes to add</param>
		public void Add(int bytes)
		{
			m_value = PointerUtils.Add(m_value, bytes).ToPointer();
		}

		/// <summary>
		///     Decrement <see cref="Address" /> by the specified number of bytes
		/// </summary>
		/// <param name="bytes">Number of bytes to subtract</param>
		public void Subtract(int bytes)
		{
			m_value = PointerUtils.Subtract(m_value, bytes).ToPointer();
		}

		/// <summary>
		///     Increment the <see cref="Address" /> by the specified number of elements
		/// </summary>
		/// <param name="elemCnt">Number of elements</param>
		public void Increment(int elemCnt = 1)
		{
			m_value = PointerUtils.Offset<T>(m_value, elemCnt).ToPointer();
		}

		/// <summary>
		///     Decrement the <see cref="Address" /> by the specified number of elements
		/// </summary>
		/// <param name="elemCnt">Number of elements</param>
		public void Decrement(int elemCnt = 1)
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

		/// <summary>
		///     Increments the <see cref="Address" /> by the specified number of elements.
		///     <remarks>
		///         Equal to <see cref="Pointer{T}.Increment" />
		///     </remarks>
		/// </summary>
		/// <param name="p">
		///     <see cref="Pointer{T}" />
		/// </param>
		/// <param name="i">Number of elements (<see cref="ElementSize" />)</param>
		public static Pointer<T> operator +(Pointer<T> p, int i)
		{
			p.Increment(i);
			return p;
		}

		/// <summary>
		///     Decrements the <see cref="Address" /> by the specified number of elements.
		///     <remarks>
		///         Equal to <see cref="Pointer{T}.Decrement" />
		///     </remarks>
		/// </summary>
		/// <param name="p">
		///     <see cref="Pointer{T}" />
		/// </param>
		/// <param name="i">Number of elements (<see cref="ElementSize" />)</param>
		public static Pointer<T> operator -(Pointer<T> p, int i)
		{
			p.Decrement(i);
			return p;
		}

		/// <summary>
		///     Increments the <see cref="Pointer{T}" /> by one element.
		/// </summary>
		/// <param name="p">
		///     <see cref="Pointer{T}" />
		/// </param>
		/// <returns>The offset <see cref="Address" /></returns>
		public static Pointer<T> operator ++(Pointer<T> p)
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
			if (ReferenceEquals(null, obj)) {
				return false;
			}

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

		/// <summary>
		/// </summary>
		/// <param name="format">
		///     <para><c>"O"</c>: Object (<see cref="Reference" />) </para>
		///     <para><c>"P"</c>: Pointer (<see cref="Address" />) </para>
		///     <para><c>"S"</c>: Safe <c>"O"</c> (when <see cref="Reference" /> or <see cref="Address" /> may be <c>null</c>) </para>
		/// </param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
//			if (String.IsNullOrEmpty(format)) {
//				format = "O";
//			}

//			if (formatProvider == null) {
//				formatProvider = CultureInfo.CurrentCulture;
//			}


			switch (format.ToUpperInvariant()) {
				case "O":
					if (typeof(T).IsIListType()) {
						return Collections.ListToString((IList) Reference);
					}

					return Reference.ToString();
				case "P":
					return Hex.ToHex(Address);
				case "S":
					if (Reference == null || IsNull) {
						return "(null)";
					}
					else {
						goto case "O";
					}
				default:
					goto case "P";
			}
		}

		/*[HandleProcessCorruptedStateExceptions]
		public T TryRead()
		{
			T t;
			try {
				t = Read<T>();
			}
			catch (AccessViolationException) {
				return default;
			}
			catch (NullReferenceException) {
				return default;
			}

			return t;
		}*/


		public override string ToString()
		{
			return ToString("O", null);
		}

		#endregion


	}

}