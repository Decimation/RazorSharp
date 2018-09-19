#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using RazorCommon;
using RazorCommon.Extensions;
using RazorCommon.Strings;
using RazorSharp.CLR.Fixed;
using RazorSharp.Memory;
using RazorSharp.Utilities;

#endregion

namespace RazorSharp.Pointers
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion


	/// <summary>
	///     <para>Represents a native pointer. Equals the size of <see cref="IntPtr.Size" />.</para>
	///     <para>Can be represented as a native pointer in memory. </para>
	///     <para>Has identical or better performance than native pointers.</para>
	///     <para>
	///         Supports pointer arithmetic, reading/writing different types other than type <typeparamref name="T" />, and
	///         bitwise operations.
	///     </para>
	///     <list type="bullet">
	///         <item>
	///             <description>No bounds checking</description>
	///         </item>
	///         <item>
	///             <description>No safety systems</description>
	///         </item>
	///         <item>
	///             <description>Minimum type safety</description>
	///         </item>
	///     </list>
	///     <remarks>
	///         Note: <c>Pointer&lt;byte&gt;</c> is used as an opaque pointer where applicable.
	///     </remarks>
	/// </summary>
	/// <typeparam name="T">Element type to point to</typeparam>
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

		public bool IsAligned => Mem.IsAligned(Address);

		#endregion

		#region Constructors

		/// <summary>
		///     Creates a new <see cref="T:RazorSharp.Pointers.Pointer`1" /> pointing to the address <paramref name="p" />
		/// </summary>
		/// <param name="p">Address to point to</param>
		public Pointer(IntPtr p) : this(p.ToPointer()) { }

		/// <summary>
		///     Creates a new <see cref="Pointer{T}" /> pointing to the address <paramref name="v" />
		/// </summary>
		/// <param name="v">Address to point to</param>
		public Pointer(void* v)
		{
			// Root constructor
			m_pValue = v;
		}

		/// <summary>
		///     Creates a new <see cref="T:RazorSharp.Pointers.Pointer`1" /> pointing to the address <paramref name="v" />
		///     represented as an
		///     <see cref="T:System.Int64" />
		/// </summary>
		/// <param name="v">Address to point to</param>
		public Pointer(long v) : this((void*) v) { }

		public Pointer(ulong ul) : this((void*) ul) { }

		/// <summary>
		///     Creates a new <see cref="T:RazorSharp.Pointers.Pointer`1" /> pointing to the address of <paramref name="t" />
		/// </summary>
		/// <param name="t">Variable whose address will be pointed to</param>
		public Pointer(ref T t) : this(Unsafe.AddressOf(ref t)) { }

		#endregion

		#region Collection-esque operations

		/// <summary>
		///     Retrieves the index of the specified element <paramref name="value" />
		/// </summary>
		/// <param name="value">Value to retrieve the index of</param>
		/// <param name="searchLength">How many elements to search, starting from the current index</param>
		/// <returns>The index of the element if it was found; <c>-1</c> if the element was not found</returns>
		public int IndexOf(T value, int searchLength)
		{
			return IndexOf(value, 0, searchLength);
		}

		/// <summary>
		///     Retrieves the index of the specified element <paramref name="value" />
		/// </summary>
		/// <param name="value">Value to retrieve the index of</param>
		/// <param name="startIndex">Index to start searching from</param>
		/// <param name="searchLength">How many elements to search, starting from the current index</param>
		/// <returns>The index of the element if it was found; <c>-1</c> if the element was not found</returns>
		public int IndexOf(T value, int startIndex, int searchLength)
		{
			for (int i = startIndex; i < searchLength; i++) {
				if (this[i].Equals(value)) {
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		///     Writes all elements of <paramref name="enumerable" /> to the current pointer.
		/// </summary>
		/// <param name="enumerable">Values to write</param>
		public void Init(IEnumerable<T> enumerable)
		{
			int            i          = 0;
			IEnumerator<T> enumerator = enumerable.GetEnumerator();
			while (enumerator.MoveNext()) {
				this[i++] = enumerator.Current;
			}

			enumerator.Dispose();
		}

		/// <summary>
		///     Writes all elements of <paramref name="values" /> to the current pointer.
		/// </summary>
		/// <param name="values">Values to write</param>
		public void Init(params T[] values)
		{
			for (int i = 0; i < values.Length; i++) {
				this[i] = values[i];
			}
		}

		/// <summary>
		///     Determines whether the pointer contains <paramref name="value" /> from the range specified.
		/// </summary>
		/// <param name="value">Value to search for</param>
		/// <param name="searchLength">Number of elements to search (range)</param>
		/// <returns><c>true</c> if the value was found within the range specified, <c>false</c> otherwise</returns>
		public bool Contains(T value, int searchLength)
		{
			return IndexOf(value, searchLength) != -1;
		}

		public bool SequenceEqual(T[] values)
		{
			return CopyOut(values.Length).SequenceEqual(values);
		}

		public bool SequenceEqual(IEnumerable<T> enumerable)
		{
			IEnumerator<T> enumerator = enumerable.GetEnumerator();
			int            i          = 0;
			while (enumerator.MoveNext()) {
				if (!enumerator.Current.Equals(this[i++])) {
					enumerator.Dispose();
					return false;
				}
			}

			enumerator.Dispose();
			return true;
		}

		#endregion


		#region Bitwise operations

		/// <summary>
		///     Performs the bitwise AND (<c>&</c>) operation on <see cref="ToInt64" /> and
		///     sets <see cref="Address" /> as the result
		/// </summary>
		/// <param name="l">Operand</param>
		public void And(long l)
		{
			long newAddr = ToInt64() & l;
			Address = new IntPtr(newAddr);
		}

		/// <summary>
		///     Performs the bitwise OR (<c>|</c>) operation on <see cref="ToInt64" /> and
		///     sets <see cref="Address" /> as the result
		/// </summary>
		/// <param name="l">Operand</param>
		public void Or(long l)
		{
			long newAddr = ToInt64() | l;
			Address = new IntPtr(newAddr);
		}

		#endregion


		#region Read / write

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

		/// <summary>
		///     Copies <paramref name="elemCnt" /> elements into an array of type <typeparamref name="T" />, starting
		///     from index 0.
		/// </summary>
		/// <param name="elemCnt">Number of elements to copy</param>
		/// <returns>
		///     An array of length <paramref name="elemCnt" /> of type <typeparamref name="T" /> copied from
		///     the current pointer
		/// </returns>
		public T[] CopyOut(int elemCnt)
		{
			return CopyOut(0, elemCnt);
		}

		/// <summary>
		///     Copies <paramref name="elemCnt" /> elements into an array of type <typeparamref name="T" />,
		///     starting from index <paramref name="startIndex" />
		/// </summary>
		/// <param name="startIndex">Index to begin copying from</param>
		/// <param name="elemCnt">Number of elements to copy</param>
		/// <returns>
		///     An array of length <paramref name="elemCnt" /> of type <typeparamref name="T" /> copied from
		///     the current pointer
		/// </returns>
		public T[] CopyOut(int startIndex, int elemCnt)
		{
			return CopyOut<T>(startIndex, elemCnt);
		}

		/// <summary>
		///     Copies <paramref name="elemCnt" /> elements into an array of type <typeparamref name="TType" />,
		///     starting from index <paramref name="startIndex" />
		/// </summary>
		/// <param name="startIndex">Index to begin copying from</param>
		/// <param name="elemCnt">Number of elements to copy</param>
		/// <returns>
		///     An array of length <paramref name="elemCnt" /> of type <typeparamref name="TType" /> copied from
		///     the current pointer
		/// </returns>
		public TType[] CopyOut<TType>(int startIndex, int elemCnt)
		{
			TType[] rg = new TType[elemCnt];
			for (int i = startIndex; i < elemCnt; i++) {
				rg[i] = Read<TType>(i);
			}

			return rg;
		}

		/// <summary>
		///     Copies <paramref name="elemCnt" /> elements into an array of type <typeparamref name="TType" />,
		///     starting from index 0.
		/// </summary>
		/// <param name="elemCnt">Number of elements to copy</param>
		/// <returns>
		///     An array of length <paramref name="elemCnt" /> of type <typeparamref name="TType" /> copied from
		///     the current pointer
		/// </returns>
		public TType[] CopyOut<TType>(int elemCnt)
		{
			return CopyOut<TType>(0, elemCnt);
		}

		#endregion


		#region Other methods

		public void Zero(int byteCnt)
		{
			Mem.Zero(m_pValue, byteCnt);
		}

		public PinHandle TryPin()
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
				table.AddRow(Hex.ToHex(m_pValue), Reference, IsAligned.Prettify(),
					IsNull.Prettify(), ElementSize);
			}
			else {
				table.AddRow(Hex.ToHex(m_pValue), Hex.ToHex(Read<long>()), Reference,
					IsAligned.Prettify(), IsNull.Prettify(), ElementSize);
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

		/// <summary>
		///     Creates a new <see cref="Pointer{T}" /> of type <typeparamref name="TNew" />, pointing to <see cref="Address" />
		/// </summary>
		/// <typeparam name="TNew">Type to point to</typeparam>
		/// <returns>A new <see cref="Pointer{T}" /> of type <typeparamref name="TNew" /></returns>
		public Pointer<TNew> Reinterpret<TNew>()
		{
			return new Pointer<TNew>(Address);
		}

		public void* ToPointer()
		{
			return m_pValue;
		}

		#region Integer conversions

		public int ToInt32()
		{
			return (int) m_pValue;
		}

		public long ToInt64()
		{
			return (long) m_pValue;
		}

		public ulong ToUInt64()
		{
			return (ulong) m_pValue;
		}

		public uint ToUInt32()
		{
			return (uint) m_pValue;
		}

		#endregion

		#endregion

		#region Operators

		#region Bitwise operators

		public static Pointer<T> operator &(Pointer<T> ptr, long l)
		{
			ptr.And(l);
			return ptr;
		}

		public static Pointer<T> operator |(Pointer<T> ptr, long l)
		{
			ptr.Or(l);
			return ptr;
		}

		#endregion

		#region Implicit and explicit conversions

		public static implicit operator Pointer<T>(void* v)
		{
			return new Pointer<T>(v);
		}

		public static implicit operator Pointer<T>(IntPtr p)
		{
			return new Pointer<T>(p.ToPointer());
		}

		public static implicit operator Pointer<T>(long l)
		{
			return new Pointer<T>(l);
		}

		public static implicit operator Pointer<T>(ulong ul)
		{
			return new Pointer<T>(ul);
		}

		public static explicit operator void*(Pointer<T> ptr)
		{
			return ptr.ToPointer();
		}

		public static explicit operator long(Pointer<T> ptr)
		{
			return ptr.ToInt64();
		}

		#endregion


		#region Arithmetic

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IntPtr Offset(int elemCnt)
		{
			return PointerUtils.Offset<T>(m_pValue, elemCnt);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IntPtr Offset<TType>(int elemCnt)
		{
			return PointerUtils.Offset<TType>(m_pValue, elemCnt);
		}


		/// <summary>
		///     Increment <see cref="Address" /> by the specified number of bytes
		/// </summary>
		/// <param name="bytes">Number of bytes to add</param>
		public void Add(long bytes)
		{
			m_pValue = PointerUtils.Add(m_pValue, bytes).ToPointer();
		}


		/// <summary>
		///     Decrement <see cref="Address" /> by the specified number of bytes
		/// </summary>
		/// <param name="bytes">Number of bytes to subtract</param>
		public void Subtract(long bytes)
		{
			m_pValue = PointerUtils.Subtract(m_pValue, bytes).ToPointer();
		}


		/// <summary>
		///     Increment <see cref="Address" /> by the specified number of elements
		/// </summary>
		/// <param name="elemCnt">Number of elements</param>
		public void Increment(int elemCnt = 1)
		{
			m_pValue = PointerUtils.Offset<T>(m_pValue, elemCnt).ToPointer();
		}


		/// <summary>
		///     Decrement <see cref="Address" /> by the specified number of elements
		/// </summary>
		/// <param name="elemCnt">Number of elements</param>
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

		/// <summary>
		///     Checks if <paramref name="left" /> <see cref="Address" /> is higher than <paramref name="right" />'s.
		/// </summary>
		/// <param name="left">Left operand</param>
		/// <param name="right">Right operand</param>
		/// <returns>
		///     <c>true</c> if <paramref name="left" /> points to a higher address than <paramref name="right" />;
		///     <c>false</c> otherwise
		/// </returns>
		public static bool operator >(Pointer<T> left, Pointer<T> right)
		{
			return left.ToInt64() > right.ToInt64();
		}

		/// <summary>
		///     Checks if <paramref name="left" /> <see cref="Address" /> is lower than <paramref name="right" />'s.
		/// </summary>
		/// <param name="left">Left operand</param>
		/// <param name="right">Right operand</param>
		/// <returns>
		///     <c>true</c> if <paramref name="left" /> points to a lower address than <paramref name="right" />; <c>false</c>
		///     otherwise
		/// </returns>
		public static bool operator <(Pointer<T> left, Pointer<T> right)
		{
			return left.ToInt64() < right.ToInt64();
		}

		#endregion

		#endregion

		#region Equality operators

		public bool Equals(Pointer<T> other)
		{
			return Address == other.Address;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) {
				return false;
			}

			return obj is Pointer<T> pointer && Equals(pointer);
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

		/// <inheritdoc />
		/// <summary>
		/// </summary>
		/// <param name="format">
		///     <para>
		///         <c>"O"</c>: Object (<see cref="P:RazorSharp.Pointers.Pointer`1.Reference" />). If <typeparamref name="T" />
		///         is <see cref="Char" />, it will be printed as a C-string.
		///     </para>
		///     <para><c>"P"</c>: Pointer (<see cref="P:RazorSharp.Pointers.Pointer`1.Address" />) </para>
		///     <para>
		///         <c>"S"</c>: Safe <c>"O"</c> (when <see cref="P:RazorSharp.Pointers.Pointer`1.Reference" /> or
		///         <see cref="P:RazorSharp.Pointers.Pointer`1.Address" /> may be <c>null</c>)
		///     </para>
		///     <para><c>"I"</c>: Table of information </para>
		///     <para>
		///         <c>"B"</c>: Both <see cref="P:RazorSharp.Pointers.Pointer`1.Address" /> and
		///         <see cref="P:RazorSharp.Pointers.Pointer`1.Reference" />
		///     </para>
		/// </param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrEmpty(format)) {
				format = PointerSettings.FMT_O;
			}

			if (formatProvider == null) {
				formatProvider = CultureInfo.CurrentCulture;
			}


			switch (format.ToUpperInvariant()) {
				case PointerSettings.FMT_O:
					if (typeof(T).IsIListType()) {
						return Collections.ListToString((IList) Reference);
					}

					/* Special support for C-string */
					if (typeof(T) == typeof(char)) {
						return new string((char*) m_pValue);
					}

					return Reference.ToString();
				case PointerSettings.FMT_I:
					return ToInfoTable().ToMarkDownString();
				case PointerSettings.FMT_P:
					return Hex.ToHex(Address);
				case PointerSettings.FMT_S:
					if (Reference == null || IsNull) {
						return PointerSettings.NULLPTR;
					}
					else {
						goto case PointerSettings.FMT_O;
					}
				case PointerSettings.FMT_B:
					return String.Format("Value @ {0}:\n{1}", Hex.ToHex(Address), Reference.ToString());
				default:
					goto case PointerSettings.FMT_O;
			}
		}

		public string ToString(string format)
		{
			return ToString(format, CultureInfo.CurrentCulture);
		}

		public override string ToString()
		{
			return ToString(PointerSettings.FMT_O, null);
		}

		#endregion


	}

}