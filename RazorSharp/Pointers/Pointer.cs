#region

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using RazorSharp.CLR.Fixed;
using RazorSharp.Common;
using RazorSharp.Memory;
using RazorSharp.Native;
using RazorSharp.Native.Enums;
using RazorSharp.Native.Structures;
using RazorSharp.Utilities;

#endregion

// ReSharper disable UseStringInterpolation

#endregion

namespace RazorSharp.Pointers
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion

	public enum StringTypes
	{
		/// <summary>
		///     <see cref="sbyte" /> (1-byte) string
		/// </summary>
		AnsiStr,

		/// <summary>
		///     <see cref="char" /> (2-byte) string
		/// </summary>
		UniStr
	}


	/// <summary>
	///     <para>Represents a native pointer. Equals the size of <see cref="IntPtr.Size" />.</para>
	///     <para>Can be represented as a native pointer in memory. </para>
	///     <para>Has identical or better performance than native pointers.</para>
	///     <para>Type safety is not enforced in methods suffixed with "Any" for accessibility</para>
	///     <para>
	///         Supports pointer arithmetic, reading/writing different types other than
	///         type <typeparamref name="T" /> (in select methods), and bitwise operations.
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
	[DebuggerDisplay("{" + nameof(DbgToString) + "()}")]
	public unsafe struct Pointer<T> : IPointer<T>
	{
		/// <summary>
		///     <para>The address we're pointing to.</para>
		///     <para>We want this to be the only field so it can be represented as a pointer in memory.</para>
		/// </summary>
		private void* m_value;

		#region Properties

		public ref T this[int index] => ref AsRef(index);

		public ref T Reference => ref AsRef();

		public T Value {
			get => Read();
			set => Write(value);
		}

		public IntPtr Address {
			get => (IntPtr) m_value;
			set => m_value = (void*) value;
		}

		public int ElementSize => Unsafe.SizeOf<T>();

		public bool IsNull => m_value == null;

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
			m_value = v;
		}

		/// <summary>
		///     Creates a new <see cref="T:RazorSharp.Pointers.Pointer`1" /> pointing to the address <paramref name="v" />
		///     represented as an <see cref="T:System.Int64" />
		/// </summary>
		/// <param name="v">Address to point to</param>
		public Pointer(long v) : this((void*) v) { }

		public Pointer(ulong ul) : this((void*) ul) { }

		/// <summary>
		///     Creates a new <see cref="T:RazorSharp.Pointers.Pointer`1" /> pointing to the address of <paramref name="t" />
		/// </summary>
		/// <param name="t">Variable whose address will be pointed to</param>
		public Pointer(ref T t) : this(Unsafe.AddressOf(ref t).Address) { }

		#endregion

		#region Collection-esque operations

		#region IndexOf

		/// <summary>
		///     Retrieves the index of the specified element <paramref name="value" />.
		///     <remarks>
		///         Indexes are in terms of <typeparamref name="T" />
		///     </remarks>
		/// </summary>
		/// <param name="value">Value to retrieve the index of</param>
		/// <param name="startIndex">Index to start searching from</param>
		/// <param name="searchLength">How many elements to search, starting from the current index</param>
		/// <returns>The index of the element if it was found; <see cref="Unsafe.INVALID_VALUE" /> if the element was not found</returns>
		public int IndexOf(T value, int startIndex, int searchLength)
		{
			for (int i = startIndex; i < searchLength + startIndex; i++) {
				if (Read(i).Equals(value)) {
					return i;
				}
			}

			return Unsafe.INVALID_VALUE;
		}

		/// <summary>
		///     Retrieves the index of the specified element <paramref name="value" />.
		///     <remarks>
		///         Indexes are in terms of <typeparamref name="T" />
		///     </remarks>
		/// </summary>
		/// <param name="value">Value to retrieve the index of</param>
		/// <param name="searchLength">How many elements to search, starting from the current index</param>
		/// <returns>The index of the element if it was found; <see cref="Unsafe.INVALID_VALUE" /> if the element was not found</returns>
		public int IndexOf(T value, int searchLength)
		{
			return IndexOf(value, 0, searchLength);
		}

		#endregion

		#region Set

		public void Set(T value, int startIndex, int elemCount)
		{
			for (int i = startIndex; i < elemCount + startIndex; i++) {
				Write(value, i - startIndex);
			}
		}

		public void Set(T value, int elemCount)
		{
			Set(value, 0, elemCount);
		}

		#endregion

		public IEnumerable<T> Where(int elemCount, Func<T, bool> predicate)
		{
			return CopyOut(elemCount).Where(predicate);
		}

		public IEnumerable<TResult> Select<TResult>(int elemCount, Func<T, TResult> selector)
		{
			return CopyOut(elemCount).Select(selector);
		}


		/// <summary>
		///     Initializes <paramref name="elemCount" /> elements with the default value of <typeparamref name="T" />.
		/// </summary>
		/// <param name="elemCount">Number of elements</param>
		public void Init(int elemCount)
		{
			Set(default, elemCount);
		}


		#region Contains

		/// <summary>
		///     Determines whether the pointer contains <paramref name="value" /> from the range specified.
		/// </summary>
		/// <param name="value">Value to search for</param>
		/// <param name="searchLength">Number of elements to search (range)</param>
		/// <returns><c>true</c> if the value was found within the range specified, <c>false</c> otherwise</returns>
		public bool Contains(T value, int searchLength)
		{
			return IndexOf(value, searchLength) != Unsafe.INVALID_VALUE;
		}

		#endregion

		#region SequenceEqual

		public bool SequenceEqual(T[] values)
		{
			return CopyOut(values.Length).SequenceEqual(values);
		}

		public bool SequenceEqual(IEnumerable<T> enumerable)
		{
			IEnumerator<T> enumerator = enumerable.GetEnumerator();
			int            i          = 0;
			while (enumerator.MoveNext()) {
				RazorContract.RequiresNotNull(enumerator.Current);

				if (!enumerator.Current.Equals(this[i++])) {
					enumerator.Dispose();
					return false;
				}
			}

			enumerator.Dispose();
			return true;
		}

		#endregion

		#endregion


		#region Bitwise operations

		/// <summary>
		///     Performs the bitwise AND (<c>&</c>) operation on <see cref="ToInt64" /> and
		///     sets <see cref="Address" /> as the result
		/// </summary>
		/// <returns>
		///     <c>this</c>
		/// </returns>
		/// <param name="l">Operand</param>
		public Pointer<T> And(long l)
		{
			long newAddr = ToInt64() & l;
			Address = new IntPtr(newAddr);
			return this;
		}

		/// <summary>
		///     Performs the bitwise OR (<c>|</c>) operation on <see cref="ToInt64" /> and
		///     sets <see cref="Address" /> as the result
		/// </summary>
		/// <returns>
		///     <c>this</c>
		/// </returns>
		/// <param name="l">Operand</param>
		public Pointer<T> Or(long l)
		{
			long newAddr = ToInt64() | l;
			Address = new IntPtr(newAddr);
			return this;
		}

		#endregion


		public MemoryBasicInformation Query()
		{
			return Kernel32.VirtualQuery(Address);
		}

		public bool IsWritable => !IsReadOnly;
		public bool IsReadOnly => Query().Protect.HasFlag(MemoryProtection.ReadOnly);


		private string DbgToString()
		{
			return string.Format("Address = {0} | Value = {1}", ToString(PointerSettings.FMT_P),
				Reference.ToString());
		}

		#region Read / write

		#region WriteAll

		/// <summary>
		///     Writes all elements of <paramref name="enumerable" /> to the current pointer.
		/// </summary>
		/// <param name="enumerable">Values to write</param>
		public void WriteAll(IEnumerable<T> enumerable)
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
		public void WriteAll(params T[] values)
		{
			for (int i = 0; i < values.Length; i++) {
				this[i] = values[i];
			}
		}

		#endregion

		#region As

		/// <summary>
		///     Reads a value of <typeparamref name="TType" /> as a <typeparamref name="TAs" />
		///     <remarks>
		///         This is the same operation as <see cref="Mem.ReinterpretCast{TFrom,TTo}" />
		///     </remarks>
		/// </summary>
		/// <param name="elemOffset">Element offset in terms of <typeparamref name="TType" /></param>
		/// <typeparam name="TType">Inherent type</typeparam>
		/// <typeparam name="TAs">Type to reinterpret <typeparamref name="TType" /> as</typeparam>
		/// <returns></returns>
		public TAs ReadAs<TType, TAs>(int elemOffset = 0)
		{
			TType t = ReadAny<TType>(elemOffset);
			return CSUnsafe.Read<TAs>(Unsafe.AddressOf(ref t).ToPointer());
		}

		/// <summary>
		///     Writes a value of <typeparamref name="TType" /> as a <typeparamref name="TAs" />
		///     <remarks>
		///         This is the same operation as <see cref="Mem.ReinterpretCast{TFrom,TTo}" />
		///     </remarks>
		/// </summary>
		/// <param name="value">Value to write as a <typeparamref name="TAs" /></param>
		/// <param name="elemOffset">Element offset in terms of <typeparamref name="TType" /></param>
		/// <typeparam name="TType">Inherent type</typeparam>
		/// <typeparam name="TAs">Type to reinterpret <typeparamref name="TType" /> as</typeparam>
		public void WriteAs<TType, TAs>(TType value, int elemOffset = 0)
		{
			WriteAny(CSUnsafe.Read<TAs>(Unsafe.AddressOf(ref value).ToPointer()), elemOffset);
		}

		#endregion

		[Pure]
		public string ReadString(StringTypes s)
		{
			switch (s) {
				case StringTypes.AnsiStr:
					return new string((sbyte*) m_value);
				case StringTypes.UniStr:
					return new string((char*) m_value);
				default:
					throw new ArgumentOutOfRangeException(nameof(s), s, null);
			}
		}


		public void Write(T t, int elemOffset = 0)
		{
			WriteAny(t, elemOffset);
		}

		#region Safe write

		/// <summary>
		///     Writes <paramref name="data" /> to <see cref="Address" /> after marking the memory region
		///     <see cref="MemoryProtection.ExecuteReadWrite" />. The original region protection is restored after
		///     the value <paramref name="data" /> is written.
		/// </summary>
		/// <param name="elemOffset">Element offset relative to <see cref="Address" /></param>
		/// <param name="data">Value to write</param>
		public void SafeWrite(T data, int elemOffset = 0)
		{
			IntPtr ptr = Offset(elemOffset);

			Kernel32.VirtualProtect(ptr, ElementSize, MemoryProtection.ExecuteReadWrite,
				out MemoryProtection oldProtect);

			WriteAny(data);

			Kernel32.VirtualProtect(ptr, ElementSize, oldProtect, out oldProtect);
		}

		/// <summary>
		///     Writes the values of <paramref name="mem" /> to <see cref="Address" /> after marking the memory region
		///     <see cref="MemoryProtection.ExecuteReadWrite" />. The original region protection is restored after
		///     the values of <paramref name="mem" /> are written.
		/// </summary>
		/// <param name="byteOffset">Byte offset relative to <see cref="Address" /></param>
		/// <param name="mem">Byte values to write</param>
		public void SafeWrite(byte[] mem, int byteOffset = 0)
		{
			Pointer<byte> ptr = Offset<byte>(byteOffset);

			Kernel32.VirtualProtect(ptr, mem.Length, MemoryProtection.ExecuteReadWrite,
				out MemoryProtection oldProtect);

			ptr.WriteAll(mem);

			Kernel32.VirtualProtect(ptr, mem.Length, oldProtect, out oldProtect);
		}

		#endregion

		[Pure]
		public T Read(int elemOffset = 0)
		{
			return Mem.Read<T>(Offset(elemOffset));
		}


		[Pure]
		public ref T AsRef(int elemOffset = 0)
		{
			return ref Mem.AsRef<T>(Offset(elemOffset));
		}

		#region Any

		public void WriteAny<TType>(TType t, int elemOffset = 0)
		{
			Mem.Write(Offset<TType>(elemOffset), 0, t);
		}


		[Pure]
		public TType ReadAny<TType>(int elemOffset = 0)
		{
			return Mem.Read<TType>(Offset<TType>(elemOffset));
		}

		[Pure]
		public ref TType AsRefAny<TType>(int elemOffset = 0)
		{
			return ref Mem.AsRef<TType>(Offset(elemOffset));
		}

		#endregion

		#region Copy

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
			T[] rg = new T[elemCnt];
			for (int i = startIndex; i < elemCnt + startIndex; i++) {
				rg[i - startIndex] = Read(i);
			}

			return rg;
		}

		/// <summary>
		///     Copies <paramref name="elemCnt" /> elements into an array of type <typeparamref name="T" />,
		///     starting from index 0.
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

		#endregion

		#endregion


		#region Other methods

		public Pointer<T> AddressOfIndex(int index)
		{
			return Offset(index);
		}

		public void Zero(int byteCnt)
		{
			Mem.Zero(m_value, byteCnt);
		}

		public PinHandle TryPin()
		{
			RazorContract.Requires(!typeof(T).IsValueType, "Value types do not need to be pinned");
			return new ObjectPinHandle(Value);
		}


		public ConsoleTable ToInfoTable()
		{
			ConsoleTable table = typeof(T).IsValueType
				? new ConsoleTable("Address", "Value", "Aligned", "Null", "Element size", "Type")
				: new ConsoleTable("Address", "Pointer", "Value", "Aligned", "Null", "Element size", "Type");

			if (typeof(T).IsValueType) {
				table.AddRow(Hex.ToHex(m_value), ToString(PointerSettings.FMT_O), IsAligned.Prettify(),
					IsNull.Prettify(), ElementSize, string.Format("<{0}>", typeof(T).Name));
			}
			else {
				table.AddRow(Hex.ToHex(m_value), Hex.ToHex(ReadAny<long>()), ToString(PointerSettings.FMT_O),
					IsAligned.Prettify(), IsNull.Prettify(), ElementSize, string.Format("<{0}>", typeof(T).Name));
			}

			return table;
		}

		public ConsoleTable ToTable(int elemCnt)
		{
			ConsoleTable table = typeof(T).IsValueType
				? new ConsoleTable("Address", "Offset", "Value")
				: new ConsoleTable("Address", "Offset", "Pointer", "Value");

			for (int i = 0; i < elemCnt; i++) {
				Pointer<T> ptr = AddressOfIndex(i);
				if (!typeof(T).IsValueType) {
					table.AddRow(ptr.ToString(PointerSettings.FMT_P), i, Hex.ToHex(ReadAny<long>(i)), ptr.ToStringSafe
						());
				}
				else {
					table.AddRow(ptr.ToString(PointerSettings.FMT_P), i, ptr.ToStringSafe());
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
			return m_value;
		}

		#region Integer conversions

		public int ToInt32()
		{
			return (int) m_value;
		}

		public long ToInt64()
		{
			return (long) m_value;
		}

		public ulong ToUInt64()
		{
			return (ulong) m_value;
		}

		public uint ToUInt32()
		{
			return (uint) m_value;
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

		public static implicit operator Pointer<T>(Pointer<byte> v)
		{
			return v.Address;
		}

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

		public static explicit operator ulong(Pointer<T> ptr)
		{
			return ptr.ToUInt64();
		}

		#endregion


		#region Arithmetic

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IntPtr Offset(int elemCnt)
		{
			return Offset<T>(elemCnt);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IntPtr Offset<TType>(int elemCnt)
		{
			return PointerUtils.Offset<TType>(m_value, elemCnt);
		}


		/// <summary>
		///     Increment <see cref="Address" /> by the specified number of bytes
		/// </summary>
		/// <param name="bytes">Number of bytes to add</param>
		/// <returns>
		///     <c>this</c>
		/// </returns>
		public Pointer<T> Add(long bytes = 1)
		{
			m_value = PointerUtils.Add(m_value, bytes).ToPointer();
			return this;
		}


		/// <summary>
		///     Decrement <see cref="Address" /> by the specified number of bytes
		/// </summary>
		/// <param name="bytes">Number of bytes to subtract</param>
		/// <returns>
		///     <c>this</c>
		/// </returns>
		public Pointer<T> Subtract(long bytes = 1)
		{
			m_value = PointerUtils.Subtract(m_value, bytes).ToPointer();
			return this;
		}


		/// <summary>
		///     Increment <see cref="Address" /> by the specified number of elements
		/// </summary>
		/// <param name="elemCnt">Number of elements</param>
		/// <returns>
		///     <c>this</c>
		/// </returns>
		public Pointer<T> Increment(int elemCnt = 1)
		{
			m_value = Offset(elemCnt).ToPointer();
			return this;
		}


		/// <summary>
		///     Decrement <see cref="Address" /> by the specified number of elements
		/// </summary>
		/// <param name="elemCnt">Number of elements</param>
		/// <returns>
		///     <c>this</c>
		/// </returns>
		public Pointer<T> Decrement(int elemCnt = 1)
		{
			m_value = Offset(-elemCnt).ToPointer();
			return this;
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

		#endregion

		#region Overrides

		/// <inheritdoc />
		/// <summary>
		/// </summary>
		/// <param name="format">
		///     <para>
		///         <c>"O"</c>: Object (<see cref="P:RazorSharp.Pointers.Pointer`1.Reference" />).
		///         <list type="bullet">
		///             <item>
		///                 <description>
		///                     If <typeparamref name="T" /> is <see cref="Char" />, it will be
		///                     returned as a C-string represented as a <see cref="String" />.
		///                 </description>
		///             </item>
		///             <item>
		///                 <description>
		///                     If <typeparamref name="T" /> is a reference type, its string representation will be
		///                     returned along with its heap pointer in <c>"P"</c> format.
		///                 </description>
		///             </item>
		///             <item>
		///                 <description>
		///                     If <typeparamref name="T" /> is an <see cref="IList" /> type, its contents will be returned
		///                     along with its heap pointer in <c>"P"</c> format.
		///                 </description>
		///             </item>
		///             <item>
		///                 <description>
		///                     If <typeparamref name="T" /> is a number type, its value will be returned as well its
		///                     value in <see cref="Hex.ToHex(long)" /> format.
		///                 </description>
		///             </item>
		///         </list>
		///     </para>
		///     <para>
		///         <c>"P"</c>: Pointer (<see cref="P:RazorSharp.Pointers.Pointer`1.Address" />) in <see cref="Hex.ToHex{T}" />
		///         format
		///     </para>
		///     <para><c>"I"</c>: Table of information </para>
		///     <para>
		///         <c>"B"</c>: Both <c>"P"</c> and <c>"O"</c>
		///     </para>
		/// </param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		[Pure]
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (string.IsNullOrEmpty(format)) {
				format = PointerSettings.FMT_B;
			}

			if (formatProvider == null) {
				formatProvider = CultureInfo.CurrentCulture;
			}


			switch (format.ToUpperInvariant()) {
				case PointerSettings.FMT_O:
					return ToStringSafe();

				case PointerSettings.FMT_I:
					return ToInfoTable().ToMarkDownString();

				case PointerSettings.FMT_P:
					return Hex.ToHex(this);

				case PointerSettings.FMT_B:
					string thisStr = ToStringSafe();
					return string.Format("{0} @ {1}: {2}", typeof(T) == typeof(char) ? "Char*" : typeof(T).Name,
						Hex.ToHex(Address), thisStr.Contains('\n') ? '\n' + thisStr : thisStr);
				default:
					goto case PointerSettings.FMT_O;
			}
		}

		public string ToStringSafe()
		{
			if (IsNull) {
				return PointerSettings.NULLPTR;
			}


			if (typeof(T).IsNumericType()) {
				return string.Format("{0} ({1})", Reference, Hex.TryCreateHex(Reference));
			}

			/* Special support for C-string */
			if (typeof(T) == typeof(char)) {
				return ReadString(StringTypes.UniStr);
			}

			/*if (typeof(T) == typeof(sbyte)) {
				return inst.ReadString(StringTypes.AnsiStr);
			}*/


			if (!typeof(T).IsValueType) {
				Pointer<byte> heapPtr = ReadAny<Pointer<byte>>();
				string        valueStr;

				if (heapPtr.IsNull) {
					valueStr = PointerSettings.NULLPTR;
					goto RETURN;
				}

				if (typeof(T).IsIListType()) {
					valueStr = $"[{Collections.ToString((IList) Reference)}]";
				}
				else {
					valueStr = Reference == null ? PointerSettings.NULLPTR : Reference.ToString();
				}

				RETURN:
				return string.Format("{0} ({1})", valueStr, heapPtr.ToString(PointerSettings.FMT_P));
			}


			return Reference.ToString();
		}

		[Pure]
		public string ToString(string format)
		{
			return ToString(format, CultureInfo.CurrentCulture);
		}

		[Pure]
		public override string ToString()
		{
			return ToString(PointerSettings.FMT_B);
		}

		#endregion


	}

}