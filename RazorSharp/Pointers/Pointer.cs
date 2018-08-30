#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using RazorCommon;
using RazorCommon.Extensions;
using RazorCommon.Strings;
using RazorSharp.CLR.Fixed;
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
	///     <para>Has identical or better performance than native pointers.</para>
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
	public unsafe struct Pointer<T> : IPointer<T>
	{
		/// <summary>
		///     <para>The address we're pointing to.</para>
		///     <para>We want this to be the only field so it can be represented as a pointer in memory.</para>
		/// </summary>
		private void* m_pValue;

		#region Properties

		public ref T this[int index] => ref AsRef<T>(index);

		public ref T Reference => ref AsRef<T>();

		public T Value {
			get => Read<T>();
			set => Write(value);
		}

		public IntPtr Address {
			get => (IntPtr) m_pValue;
			set => m_pValue = (void*) value;
		}

		public int ElementSize => Unsafe.SizeOf<T>();

		public bool IsNull => m_pValue == null;

		public bool IsAligned => MMemory.IsAligned(Address);

		#endregion

		#region Constructors

		public Pointer(IntPtr p) : this(p.ToPointer()) { }

		public Pointer(void* v)
		{
			m_pValue = v;
		}

		public Pointer(long v) : this((void*) v) { }

		public Pointer(ref T t) : this(Unsafe.AddressOf(ref t)) { }

		#endregion

		#region Collection-esque operations

		public int IndexOf(T value, int length)
		{
			return IndexOf(value, 0, length);
		}

		public int IndexOf(T value, int startIndex, int elemCnt)
		{
			for (int i = startIndex; i < elemCnt; i++) {
				if (this[i].Equals(value)) {
					return i;
				}
			}

			return -1;
		}

		public void Init(IEnumerable<T> enumerable)
		{
			int            i          = 0;
			IEnumerator<T> enumerator = enumerable.GetEnumerator();
			while (enumerator.MoveNext()) {
				this[i++] = enumerator.Current;
			}

			enumerator.Dispose();
		}

		public void Init(params T[] values)
		{
			for (int i = 0; i < values.Length; i++) {
				this[i] = values[i];
			}
		}

		#endregion


		#region Read / write

		public void Write<TType>(TType t, int elemOffset = 0)
		{
			MMemory.Write(Offset<TType>(elemOffset), 0, t);
		}

		public void ForceWrite<TType>(TType t, int elemOffset = 0)
		{
			MMemory.ForceWrite(Offset<TType>(elemOffset), 0, t);
		}

		public TType Read<TType>(int elemOffset = 0)
		{
			return MMemory.Read<TType>(Offset<TType>(elemOffset));
		}


		public ref TType AsRef<TType>(int elemOffset = 0)
		{
			return ref MMemory.AsRef<TType>(Offset<TType>(elemOffset));
		}

		public T[] Copy(int startIndex, int elemCnt)
		{
			return Copy<T>(startIndex, elemCnt);
		}

		public TType[] Copy<TType>(int startIndex, int elemCnt)
		{
			TType[] rg = new TType[elemCnt];
			for (int i = startIndex; i < elemCnt; i++) {
				rg[i] = Read<TType>(i);
			}

			return rg;
		}

		#endregion


		#region Other methods

		public PinHandle Pin()
		{
			RazorContract.Requires(!typeof(T).IsValueType, "Value types do not need to be pinned");
			return new ObjectPinHandle(Value);
		}


		public ConsoleTable ToInfoTable()
		{
			ConsoleTable table = typeof(T).IsValueType
				? new ConsoleTable("Address", "Value", "Aligned", "Null", "Element size")
				: new ConsoleTable("Address", "Pointer", "Value", "Aligned", "Null", "Element size");

			if (typeof(T).IsValueType) {
				table.AddRow(Hex.ToHex(m_pValue), Reference, IsAligned ? StringUtils.Check : StringUtils.BallotX,
					IsNull ? StringUtils.Check : StringUtils.BallotX, ElementSize);
			}
			else {
				table.AddRow(Hex.ToHex(m_pValue), Hex.ToHex(Read<long>()), Reference,
					IsAligned ? StringUtils.Check : StringUtils.BallotX,
					IsNull ? StringUtils.Check : StringUtils.BallotX, ElementSize);
			}

			return table;
		}

		public ConsoleTable ToTable(int elemCnt)
		{
			ConsoleTable table = typeof(T).IsValueType
				? new ConsoleTable("Address", "Offset", "Value")
				: new ConsoleTable("Address", "Offset", "Pointer", "Value");

			for (int i = 0; i < elemCnt; i++) {
				if (!typeof(T).IsValueType) {
					table.AddRow(Hex.ToHex(Offset(i)), i, Hex.ToHex(Read<long>(i)), this[i]);
				}
				else {
					table.AddRow(Hex.ToHex(Offset(i)), i, this[i]);
				}
			}

			return table;
		}

		public Pointer<TNew> Reinterpret<TNew>()
		{
			return new Pointer<TNew>(Address);
		}

		public void* ToPointer()
		{
			return m_pValue;
		}

		public int ToInt32()
		{
			return (int) m_pValue;
		}

		public long ToInt64()
		{
			return (long) m_pValue;
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

		public static explicit operator long(Pointer<T> ptr)
		{
			return ptr.ToInt64();
		}


		#region Arithmetic

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


		public void Add(int bytes)
		{
			m_pValue = PointerUtils.Add(m_pValue, bytes).ToPointer();
		}


		public void Subtract(int bytes)
		{
			m_pValue = PointerUtils.Subtract(m_pValue, bytes).ToPointer();
		}


		public void Increment(int elemCnt = 1)
		{
			m_pValue = PointerUtils.Offset<T>(m_pValue, elemCnt).ToPointer();
		}


		public void Decrement(int elemCnt = 1)
		{
			m_pValue = PointerUtils.Offset<T>(m_pValue, -elemCnt).ToPointer();
		}

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

		public bool Equals(IPointer<T> other)
		{
			return Address == other.Address;
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
			// ReSharper disable once NonReadonlyMemberInGetHashCode
			return unchecked((int) (long) m_pValue);
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
		///     <para><c>"I"</c>: Table of information </para>
		///     <para><c>"B"</c>: Both <see cref="Address" /> and <see cref="Reference" /></para>
		/// </param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrEmpty(format)) {
				format = "O";
			}

			if (formatProvider == null) {
				formatProvider = CultureInfo.CurrentCulture;
			}


			switch (format.ToUpperInvariant()) {
				case "O":
					if (typeof(T).IsIListType()) {
						return Collections.ListToString((IList) Reference);
					}

					/* C-string */
					if (typeof(T) == typeof(char)) {
						return new string((char*) ToPointer());
					}

					return Reference.ToString();
				case "I":
					return ToInfoTable().ToMarkDownString();
				case "P":
					return Hex.ToHex(Address);
				case "S":
					if (Reference == null || IsNull) {
						return "(null)";
					}
					else {
						goto case "O";
					}
				case "B":
					return String.Format("Value @ {0}:\n{1}", Hex.ToHex(Address), Reference.ToString());
				default:
					goto case "P";
			}
		}


		public override string ToString()
		{
			return ToString("O", null);
		}

		#endregion


	}

}