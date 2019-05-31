#region

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using InlineIL;
using JetBrains.Annotations;
using SimpleSharp;
using SimpleSharp.Diagnostics;
using SimpleSharp.Extensions;
using SimpleSharp.Strings;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Native;
using RazorSharp.Native.Win32;
using RazorSharp.Utilities;

// ReSharper disable HeuristicUnreachableCode
#pragma warning disable 162

#endregion

// ReSharper disable UseStringInterpolation

#endregion

namespace RazorSharp.Memory.Pointers
{
	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion

	// todo: decorate the remaining Pure methods with PureAttribute

	/// <summary>
	///     <para>Represents a native pointer. Equals the size of <see cref="P:System.IntPtr.Size" />.</para>
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
//	[DebuggerDisplay("{" + nameof(Dbg) + "}")]
	public unsafe struct Pointer<T> : IFormattable
	{
		/// <summary>
		///     <para>The address we're pointing to.</para>
		///     <para>We want this to be the only field so it can be represented as a pointer in memory.</para>
		/// </summary>
		private void* m_value;

		#region Properties

		/// <summary>
		///     Indexes <see cref="Address" /> as a reference.
		/// </summary>
		public ref T this[int index] => ref AsRef(index);

		/// <summary>
		///     Returns the value as a reference.
		/// </summary>
		public ref T Reference => ref AsRef();

		/// <summary>
		///     Dereferences the pointer as the specified type.
		/// </summary>
		public T Value {
			get => Read();
			set => Write(value);
		}

		/// <summary>
		///     Address being pointed to.
		/// </summary>
		public IntPtr Address {
			get => (IntPtr) m_value;
			set => m_value = (void*) value;
		}

		/// <summary>
		///     Size of type <typeparamref name="T" />.
		/// </summary>
		public int ElementSize => Unsafe.SizeOf<T>();

		/// <summary>
		///     Whether <see cref="Address" /> is <c>null</c> (<see cref="IntPtr.Zero" />).
		/// </summary>
		public bool IsNull => m_value == null;

		/// <summary>
		///     Whether the value being pointed to is <c>default</c> or <c>null</c> bytes,
		/// or this pointer is <c>null</c>.
		/// </summary>
		public bool IsNil => RtInfo.IsNil(Reference);

		#endregion

		#region Constructors

		/// <summary>
		///     Creates a new <see cref="T:RazorSharp.Memory.Pointers.Pointer`1" /> pointing to the address <paramref name="value" />
		/// </summary>
		/// <param name="value">Address to point to</param>
		public Pointer(IntPtr value) : this(value.ToPointer()) { }

		/// <summary>
		///     Creates a new <see cref="Pointer{T}" /> pointing to the address <paramref name="value" />
		/// </summary>
		/// <remarks>
		/// Root constructor.
		/// </remarks>
		/// <param name="value">Address to point to</param>
		public Pointer(void* value)
		{
			m_value = value;
		}

		/// <summary>
		///     Creates a new <see cref="T:RazorSharp.Memory.Pointers.Pointer`1" /> pointing to the address <paramref name="value" />
		///     represented as an <see cref="T:System.Int64" />
		/// </summary>
		/// <param name="value">Address to point to</param>
		public Pointer(long value) : this((void*) value) { }

		public Pointer(ulong value) : this((void*) value) { }

		/// <summary>
		///     Creates a new <see cref="T:RazorSharp.Memory.Pointers.Pointer`1" /> pointing to the address of
		///     <paramref name="value" />
		/// </summary>
		/// <param name="value">Variable whose address will be pointed to</param>
		public Pointer(ref T value) : this(Unsafe.AddressOf(ref value).Address) { }

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
		/// <returns>The index of the element if it was found; <see cref="Constants.INVALID_VALUE" /> if the element was not found</returns>
		public int IndexOf(T value, int startIndex, int searchLength)
		{
			for (int i = startIndex; i < searchLength + startIndex; i++)
				if (Read(i).Equals(value))
					return i;

			return Constants.INVALID_VALUE;
		}

		/// <summary>
		///     Retrieves the index of the specified element <paramref name="value" />.
		///     <remarks>
		///         Indexes are in terms of <typeparamref name="T" />
		///     </remarks>
		/// </summary>
		/// <param name="value">Value to retrieve the index of</param>
		/// <param name="searchLength">How many elements to search, starting from the current index</param>
		/// <returns>The index of the element if it was found; <see cref="Constants.INVALID_VALUE" /> if the element was not found</returns>
		public int IndexOf(T value, int searchLength) => IndexOf(value, 0, searchLength);

		#endregion


		public IEnumerator<T> GetEnumerator(int elemCount)
		{
			for (int i = 0; i < elemCount; i++) {
				yield return this[i];
			}
		}

		#endregion


		#region Bitwise operations

		/// <summary>
		///     Performs the bitwise OR (<c>&amp;</c>) operation on <see cref="ToInt64" /> and
		///     sets <see cref="Address" /> as the result
		/// </summary>
		/// <returns>
		///     <c>this</c>
		/// </returns>
		/// <param name="l">Operand</param>
		public Pointer<T> And(long l)
		{
			this = ToInt64() & l;
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
			this = ToInt64() | l;
			return this;
		}

		#endregion


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int CompleteSize(int elemCnt) => elemCnt * ElementSize;

		#region Read / write

		/// <summary>
		/// Reads a C-style string.
		/// </summary>
		public string ReadCString(StringTypes type = StringTypes.ANSI)
		{
			switch (type) {
				case StringTypes.ANSI:
					return ReadCString<byte>(type);
				case StringTypes.UNI:
					return ReadCString<short>(type);
				case StringTypes.CHAR32:
					return ReadCString<int>(type);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		private string ReadCString<TChar>(StringTypes type) => ReadPointer<TChar>().ReadString(type);

		public Pointer<TType> ReadPointer<TType>(int elemOffset = 0)
		{
			return ReadAny<Pointer<TType>>(elemOffset);
		}

		public void WritePointer<TType>(Pointer<TType> ptr, int elemOffset = 0)
		{
			WriteAny(ptr, elemOffset);
		}


		public int ReadUntil(Predicate<T> predicate)
		{
			int  i = 0;
			bool match;
			do {
				match = predicate(this[i++]);
			} while (!match);

			return i - 1;
		}

		#region WriteAll

		/// <summary>
		///     Writes all elements of <paramref name="enumerable" /> to the current pointer.
		/// </summary>
		/// <param name="enumerable">Values to write</param>
		public void WriteAll(IEnumerable<T> enumerable)
		{
			int i = 0;

			IEnumerator<T> enumerator = enumerable.GetEnumerator();

			while (enumerator.MoveNext())
				this[i++] = enumerator.Current;

			enumerator.Dispose();
		}

		/// <summary>
		///     Writes all elements of <paramref name="values" /> to the current pointer.
		/// </summary>
		/// <param name="values">Values to write</param>
		public void WriteAll(params T[] values)
		{
			Conditions.NotNull(values, nameof(values));
			Conditions.Require(values.Length > 0);
			for (int i = 0; i < values.Length; i++)
				this[i] = values[i];
		}

		#endregion


		#region String

		/// <summary>
		///     Writes a native string type
		/// </summary>
		public void WriteString(string s, StringTypes type)
		{
			byte[] bytes;
			switch (type) {
				case StringTypes.ANSI:
					bytes = Encoding.UTF8.GetBytes(s);
					break;
				case StringTypes.UNI:
					bytes = Encoding.Unicode.GetBytes(s);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}

			Cast<byte>().WriteAll(bytes);
			WriteAny<byte>(0, bytes.Length + 1);
		}

		/// <summary>
		///     Reads a native string type
		/// <seealso cref="NativeHelp.GetString(SByte*,Int32)"/>
		/// <seealso cref="NativeHelp.GetString(SByte*)"/>
		/// <seealso cref="Encoding.GetString(byte[])"/>
		/// <seealso cref="string"/>
		/// <seealso cref="Marshal.PtrToStringAuto(IntPtr)"/>
		/// <seealso cref="Marshal.PtrToStringAnsi(IntPtr)"/>
		/// <seealso cref="Marshal.PtrToStringUni(IntPtr)"/>
		/// </summary>
		[Pure]
		public string ReadString(StringTypes s)
		{
			switch (s) {
				case StringTypes.ANSI:
					return new string((sbyte*) m_value);
				case StringTypes.UNI:
					return new string((char*) m_value);
				case StringTypes.CHAR32:
					int len = Mem.StringLength<int>(m_value);
					return Encoding.UTF32.GetString(CopyOutBytes(len * sizeof(int)));
				default:
					throw new ArgumentOutOfRangeException(nameof(s), s, null);
			}
		}

		/// <summary>
		/// <seealso cref="NativeHelp.GetString(SByte*,Int32)"/>
		/// <seealso cref="NativeHelp.GetString(SByte*)"/>
		/// <seealso cref="Encoding.GetString(byte[])"/>
		/// <seealso cref="string"/>
		/// <seealso cref="Marshal.PtrToStringAuto(IntPtr)"/>
		/// <seealso cref="Marshal.PtrToStringAnsi(IntPtr)"/>
		/// <seealso cref="Marshal.PtrToStringUni(IntPtr)"/>
		/// </summary>
		public string ReadString(int len)
		{
			char[] chars = CopyOutAny<char>(len);
			return new string(chars);
		}

		#endregion

		/// <summary>
		///     Writes a value of type <typeparamref name="T" /> to <see cref="Address" />
		/// </summary>
		/// <param name="t">Value to write</param>
		/// <param name="elemOffset">Element offset (of type <typeparamref name="T" />)</param>
		public void Write(T t, int elemOffset = 0) => WriteAny(t, elemOffset);


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
			// todo: use new VirtualProtect methods here for consistency

			Pointer<byte> ptr = OffsetFast(elemOffset);

			Kernel32.VirtualProtect(ptr, ElementSize, MemoryProtection.ExecuteReadWrite,
			                        out var oldProtect);

			ptr.WriteAny(data);

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
			// todo: use new VirtualProtect methods here for consistency

			Pointer<byte> ptr = OffsetFast<byte>(byteOffset);

			Kernel32.VirtualProtect(ptr, mem.Length, MemoryProtection.ExecuteReadWrite,
			                        out var oldProtect);

			ptr.WriteAll(mem);

			Kernel32.VirtualProtect(ptr, mem.Length, oldProtect, out oldProtect);
		}

		#endregion

		/// <summary>
		///     Reads a value of type <typeparamref name="T" /> from <see cref="Address" />
		/// </summary>
		/// <param name="elemOffset">Element offset (of type <typeparamref name="T" />)</param>
		/// <returns>The value read from the offset <see cref="Address" /></returns>
		[Pure]
		public T Read(int elemOffset = 0) => ReadAny<T>(elemOffset);

		/// <summary>
		///     Reinterprets <see cref="Address" /> as a reference to a value of type <typeparamref name="T" />
		/// </summary>
		/// <param name="elemOffset">Element offset (of type <typeparamref name="T" />)</param>
		/// <returns>A reference to a value of type <typeparamref name="T" /></returns>
		[Pure]
		public ref T AsRef(int elemOffset = 0) => ref AsRefAny<T>(elemOffset);

		#region Any

		#region Ex

		private object InvokeGenericMethod(string name, Type typeArgs, params object[] args)
		{
			Conditions.Require(!IsNull, nameof(IsNull));
			return ReflectionUtil.InvokeGenericMethod(GetType(), name, this, 
			                                          new[] {typeArgs}, args);
		}

		public void WriteAnyEx(Type t, object value, int elemOffset = 0)
		{
			InvokeGenericMethod(nameof(WriteAny), t, value, elemOffset);
		}

		public object ReadAnyEx(Type t, int elemOffset = 0)
		{
			return InvokeGenericMethod(nameof(ReadAny), t, elemOffset);
		}

		#endregion

		public void WriteAny<TType>(TType t, int elemOffset = 0)
		{
			CSUnsafe.Write(OffsetFast<TType>(elemOffset), t);
		}


		[Pure]
		public TType ReadAny<TType>(int elemOffset = 0)
		{
			return CSUnsafe.Read<TType>(OffsetFast<TType>(elemOffset));
		}


		[Pure]
		public ref TType AsRefAny<TType>(int elemOffset = 0)
		{
			return ref CSUnsafe.AsRef<TType>(OffsetFast(elemOffset));
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
		[Pure]
		public T[] CopyOut(int startIndex, int elemCnt)
		{
			var rg = new T[elemCnt];
			for (int i = startIndex; i < elemCnt + startIndex; i++)
				rg[i - startIndex] = this[i];

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
		[Pure]
		public T[] CopyOut(int elemCnt)
		{
			return CopyOut(0, elemCnt);
		}

		public TType[] CopyOutAny<TType>(int startIndex, int elemCnt)
		{
			return Cast<TType>().CopyOut(startIndex, elemCnt);
		}

		public TType[] CopyOutAny<TType>(int elemCnt)
		{
			return Cast<TType>().CopyOut(elemCnt);
		}

		[Pure]
		public byte[] CopyOutBytes(int elemCnt) => CopyOutAny<byte>(elemCnt);

		[Pure]
		public byte[] CopyOutBytes(int startIndex, int elemCnt) => CopyOutAny<byte>(startIndex, elemCnt);

		#endregion

		#endregion


		#region Other methods

		public Pointer<T> AddressOfIndex(int index) => OffsetFast(index);

		public void Zero(int elemCnt)
		{
			Mem.Zero(m_value, CompleteSize(elemCnt));
		}

		public void ZeroBytes(int byteCnt)
		{
			Mem.Zero(m_value, byteCnt);
		}

		/// <summary>
		///     Creates a new <see cref="Pointer{T}" /> of type <typeparamref name="TNew" />, pointing to
		///     <see cref="Address" />
		/// </summary>
		/// <typeparam name="TNew">Type to point to</typeparam>
		/// <returns>A new <see cref="Pointer{T}" /> of type <typeparamref name="TNew" /></returns>
		public Pointer<TNew> Cast<TNew>() => new Pointer<TNew>(Address);

		public Pointer<byte> Cast() => Cast<byte>();

		/// <summary>
		///     Returns <see cref="Address" /> as a native pointer.
		/// </summary>
		/// <returns></returns>
		[Pure]
		public void* ToPointer() => m_value;

		[Pure]
		public TUnmanaged* ToPointer<TUnmanaged>() where TUnmanaged : unmanaged => (TUnmanaged*) m_value;

		#region Integer conversions

		/// <summary>
		///     Converts <see cref="Address" /> to a 32-bit signed integer.
		/// </summary>
		/// <returns></returns>
		public int ToInt32() => (int) m_value;

		/// <summary>
		///     Converts <see cref="Address" /> to a 64-bit signed integer.
		/// </summary>
		/// <returns></returns>
		public long ToInt64() => (long) m_value;

		/// <summary>
		///     Converts <see cref="Address" /> to a 64-bit unsigned integer.
		/// </summary>
		/// <returns></returns>
		public ulong ToUInt64() => (ulong) m_value;

		/// <summary>
		///     Converts <see cref="Address" /> to a 32-bit unsigned integer.
		/// </summary>
		/// <returns></returns>
		public uint ToUInt32() => (uint) m_value;

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

		public static explicit /*implicit*/ operator bool(Pointer<T> ptr) => !ptr.IsNil;

		public static implicit operator Pointer<T>(Pointer<byte> v) => v.Address;

		public static implicit operator Pointer<T>(void* v) => new Pointer<T>(v);

		public static implicit operator Pointer<T>(IntPtr p) => new Pointer<T>(p.ToPointer());

		public static implicit operator Pointer<T>(long l) => new Pointer<T>(l);

		public static implicit operator Pointer<T>(ulong ul) => new Pointer<T>(ul);

		public static explicit operator void*(Pointer<T> ptr) => ptr.ToPointer();

		public static explicit operator long(Pointer<T> ptr) => ptr.ToInt64();

		public static explicit operator ulong(Pointer<T> ptr) => ptr.ToUInt64();

		public static explicit operator Pointer<byte>(Pointer<T> ptr) => ptr.ToUInt64();

		#endregion


		#region Arithmetic

		/// <summary>
		///     Returns the element index of a pointer relative to <see cref="Address" />
		/// </summary>
		/// <param name="current">Current pointer (high address)</param>
		/// <returns>The index</returns>
		public int OffsetIndex(Pointer<byte> current) => OffsetIndex<T>(current);

		/// <summary>
		///     Returns the element index of a pointer relative to <see cref="Address" />
		/// </summary>
		/// <param name="current">Current pointer (high address)</param>
		/// <typeparam name="TElement">Element type</typeparam>
		/// <returns>The index</returns>
		public int OffsetIndex<TElement>(Pointer<byte> current)
		{
			long delta = current.ToInt64() - ToInt64();
			return (int) delta / Unsafe.SizeOf<TElement>();
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void* OffsetFast(int elemCnt) => OffsetFast<T>(elemCnt);

		/// <summary>
		///     Offsets a pointer by <paramref name="elemCnt" /> elements.
		/// </summary>
		/// <param name="elemCnt">Elements to offset by</param>
		/// <typeparam name="TType">Element type</typeparam>
		/// <returns>
		///     <see cref="Address" /> <c>+</c> <c>(</c><paramref name="elemCnt" /> <c>*</c>
		///     <see cref="Unsafe.SizeOf{T}()" /><c>)</c>
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void* OffsetFast<TType>(int elemCnt) => (void*) ((long) m_value + Mem.CompleteSize<TType>(elemCnt));


		public Pointer<T> Add<TType>(int elemCnt = 1) => Add(Mem.CompleteSize<TType>(elemCnt));

		public Pointer<T> Subtract<TType>(int elemCnt = 1) => Add<TType>(-elemCnt);

		/// <summary>
		///     Increment <see cref="Address" /> by the specified number of bytes
		/// </summary>
		/// <param name="right">Number of bytes to add</param>
		/// <returns>
		///     <c>this</c>
		/// </returns>
		public Pointer<T> Add(long right = 1)
		{
//			m_value = PointerUtil.Add(m_value, bytes).ToPointer();
//			return this;

			long val = ToInt64() + right;
			this = val;
			return this;
		}


		/// <summary>
		///     Decrement <see cref="Address" /> by the specified number of bytes
		/// </summary>
		/// <param name="right">Number of bytes to subtract</param>
		/// <returns>
		///     <c>this</c>
		/// </returns>
		public Pointer<T> Subtract(long right = 1) => Add(-right);


		/// <summary>
		///     Increment <see cref="Address" /> by the specified number of elements
		/// </summary>
		/// <param name="elemCnt">Number of elements</param>
		/// <returns>
		///     <c>this</c>
		/// </returns>
		public Pointer<T> Increment(int elemCnt = 1)
		{
			m_value = OffsetFast(elemCnt);
			return this;
		}


		/// <summary>
		///     Decrement <see cref="Address" /> by the specified number of elements
		/// </summary>
		/// <param name="elemCnt">Number of elements</param>
		/// <returns>
		///     <c>this</c>
		/// </returns>
		public Pointer<T> Decrement(int elemCnt = 1) => Increment(-elemCnt);

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

		public static Pointer<T> operator +(Pointer<T> p, uint i) => p + (int) i;

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

		public static Pointer<T> operator -(Pointer<T> p, uint i) => p - (int) i;

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

		public static bool operator >=(Pointer<T> left, Pointer<T> right) => left.ToInt64() >= right.ToInt64();

		public static bool operator <=(Pointer<T> left, Pointer<T> right) => left.ToInt64() <= right.ToInt64();

		/// <summary>
		///     Checks if <paramref name="left" /> <see cref="Address" /> is higher than <paramref name="right" />'s.
		/// </summary>
		/// <param name="left">Left operand</param>
		/// <param name="right">Right operand</param>
		/// <returns>
		///     <c>true</c> if <paramref name="left" /> points to a higher address than <paramref name="right" />;
		///     <c>false</c> otherwise
		/// </returns>
		public static bool operator >(Pointer<T> left, Pointer<T> right) => left.ToInt64() > right.ToInt64();

		/// <summary>
		///     Checks if <paramref name="left" /> <see cref="Address" /> is lower than <paramref name="right" />'s.
		/// </summary>
		/// <param name="left">Left operand</param>
		/// <param name="right">Right operand</param>
		/// <returns>
		///     <c>true</c> if <paramref name="left" /> points to a lower address than <paramref name="right" />; <c>false</c>
		///     otherwise
		/// </returns>
		public static bool operator <(Pointer<T> left, Pointer<T> right) => left.ToInt64() < right.ToInt64();

		public static Pointer<T> operator +(Pointer<T> left, Pointer<T> right) => left.ToInt64() + right.ToInt64();

		public static Pointer<T> operator -(Pointer<T> left, Pointer<T> right) => left.ToInt64() - right.ToInt64();

		#endregion


		#region Equality operators

		/// <summary>
		///     Checks to see if <see cref="other" /> is equal to the current instance.
		/// </summary>
		/// <param name="other">Other <see cref="Pointer{T}" /></param>
		/// <returns></returns>
		public bool Equals(Pointer<T> other) => Address == other.Address;

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;

			return obj is Pointer<T> pointer && Equals(pointer);
		}

		public override int GetHashCode()
		{
			// ReSharper disable once NonReadonlyMemberInGetHashCode
			return unchecked((int) (long) m_value);
		}

		public static bool operator ==(Pointer<T> left, Pointer<T> right) => left.Equals(right);

		public static bool operator !=(Pointer<T> left, Pointer<T> right) => !left.Equals(right);

		#endregion

		#endregion

		#region Overrides

		/// <inheritdoc />
		/// <summary>
		/// </summary>
		/// <param name="format">
		///     See <see cref="PointerFormat" /> for format options
		/// </param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		[Pure]
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrEmpty(format))
				format = PointerFormat.DefaultFormat;

			if (formatProvider == null)
				formatProvider = CultureInfo.CurrentCulture;


			switch (format.ToUpperInvariant()) {
				case PointerFormat.FORMAT_INT:
					return ToInt64().ToString();
				case PointerFormat.FORMAT_OBJ:
					return ToStringSafe();
				case PointerFormat.FORMAT_PTR:
					return Hex.ToHex(ToInt64());

				case PointerFormat.FORMAT_BOTH:
					string thisStr = ToStringSafe();

					string typeName = typeof(T).ContainsAnyGenericParameters()
						? Formatting.GenericName(typeof(T))
						: typeof(T).Name;

					string typeNameDisplay = typeof(T) == typeof(char) ? "Char*" : typeName;

					return String.Format("{0} @ {1}: {2}", typeNameDisplay, Hex.ToHex(Address),
					                     thisStr.Contains('\n') ? '\n' + thisStr : thisStr);
				default:
					goto case PointerFormat.FORMAT_OBJ;
			}
		}

		public string ToStringSafe()
		{
			if (IsNull)
				return StringConstants.NULL_STR;


			if (typeof(T).IsIntegerType())
				return String.Format(PointerFormat.VAL_FMT, Reference, Hex.TryCreateHex(Reference));

			/* Special support for C-string */
			if (typeof(T) == typeof(char))
				return ReadString(StringTypes.UNI);

			/*if (typeof(T) == typeof(sbyte)) {
				return inst.ReadString(StringTypes.AnsiStr);
			}*/


			if (!RtInfo.IsStruct<T>()) {
				Pointer<byte> heapPtr = ReadPointer<byte>();
				string        valueStr;

				if (heapPtr.IsNull) {
					valueStr = StringConstants.NULL_STR;
					goto RETURN;
				}

				if (typeof(T).IsIListType())
					valueStr = $"[{((IEnumerable) Reference).AutoJoin()}]";
				else
					valueStr = Reference == null ? StringConstants.NULL_STR : Reference.ToString();

				RETURN:
				return String.Format(PointerFormat.VAL_FMT, valueStr, heapPtr.ToString(PointerFormat.FORMAT_PTR));
			}


			return Reference.ToString();
		}

		[Pure]
		public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);

		[Pure]
		public override string ToString() => ToString(PointerFormat.DefaultFormat);

		#endregion
	}
}