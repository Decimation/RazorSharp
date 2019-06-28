#region

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using SimpleSharp.Diagnostics;
using SimpleSharp.Extensions;
using SimpleSharp.Strings;
using RazorSharp.CoreClr;
using RazorSharp.Native;
using RazorSharp.Native.Win32;
using RazorSharp.Native.Win32.Enums;
using RazorSharp.Utilities;
using SimpleSharp.Strings.Formatting;

// ReSharper disable PossibleNullReferenceException

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
	///         Note: <c>AltPointer&lt;byte&gt;</c> is used as an opaque pointer where applicable.
	///     </remarks>
	/// </summary>
	/// <typeparam name="T">Element type to point to</typeparam>
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
		///     Creates a new <see cref="T:RazorSharp.Memory.Pointers.AltPointer`1" /> pointing to the address <paramref name="value" />
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

		#region Offset

		// (void*) (((long) m_value) + byteOffset)
		// (void*) (((long) m_value) + (elemOffset * ElementSize))

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int CompleteSize(int elemCnt) => elemCnt * ElementSize;

		/// <summary>
		///     Returns the element index of a pointer relative to <see cref="Address" />
		/// </summary>
		/// <param name="current">Current pointer (high address)</param>
		/// <returns>The index</returns>
		public int OffsetIndex(Pointer<byte> current)
		{
			long delta = current.ToInt64() - ToInt64();
			return (int) delta / ElementSize;
		}

		#endregion

		#region Read / write

		#region Pointer

		public Pointer<TType> ReadPointer<TType>(int elemOffset = 0)
		{
			return Cast<Pointer<TType>>().Read(elemOffset);
		}

		public void WritePointer<TType>(Pointer<TType> ptr, int elemOffset = 0)
		{
			Cast<Pointer<TType>>().Write(ptr, elemOffset);
		}

		#endregion

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

		#region Any

		public void WriteAny(Type type, object value, int elemOffset = 0)
		{
			var ptr = CastAny(type);
			var fn  = ptr.GetType().GetMethod(nameof(Write));
			fn.Invoke(ptr, new object[] {value, elemOffset});
		}

		public object ReadAny(Type type, int elemOffset = 0)
		{
			var ptr = CastAny(type);
			var fn  = ptr.GetType().GetMethod(nameof(Read));
			return fn.Invoke(ptr, new object[] {elemOffset});
		}

		#endregion


		/// <summary>
		///     Writes a value of type <typeparamref name="T" /> to <see cref="Address" />
		/// </summary>
		/// <param name="value">Value to write</param>
		/// <param name="elemOffset">Element offset (of type <typeparamref name="T" />)</param>
		public void Write(T value, int elemOffset = 0) => CSUnsafe.Write(OffsetFast(elemOffset), value);

		#region String

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

			var bptr = Cast();
			bptr.WriteAll(bytes);
			bptr.Write(0, bytes.Length + 1);
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
					return Encoding.UTF32.GetString(CopyBytes(len * sizeof(int)));
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
			char[] chars = Cast<char>().Copy(len);
			return new string(chars);
		}

		#endregion


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

			ptr.Cast<T>().Write(data);

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

			Pointer<byte> ptr = Cast().OffsetFast(byteOffset);

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
		public T Read(int elemOffset = 0) => CSUnsafe.Read<T>(OffsetFast(elemOffset));

		/// <summary>
		///     Reinterprets <see cref="Address" /> as a reference to a value of type <typeparamref name="T" />
		/// </summary>
		/// <param name="elemOffset">Element offset (of type <typeparamref name="T" />)</param>
		/// <returns>A reference to a value of type <typeparamref name="T" /></returns>
		[Pure]
		public ref T AsRef(int elemOffset = 0) => ref CSUnsafe.AsRef<T>(OffsetFast(elemOffset));

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
		public T[] Copy(int startIndex, int elemCnt)
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
		public T[] Copy(int elemCnt)
		{
			return Copy(0, elemCnt);
		}

		[Pure]
		public byte[] CopyBytes(int elemCnt) => Cast().Copy(elemCnt);

		[Pure]
		public byte[] CopyBytes(int startIndex, int elemCnt) => Cast().Copy(startIndex, elemCnt);

		#endregion

		#endregion


		#region Other methods

		private static bool IsCharPointer() => typeof(T) == typeof(char);

		public Pointer<T> AddressOfIndex(int index) => OffsetFast(index);

		public void Zero(int elemCnt)
		{
			Mem.Zero(m_value, CompleteSize(elemCnt));
		}

		public void ZeroBytes(int byteCnt)
		{
			Mem.Zero(m_value, byteCnt);
		}

		#region Cast

		/// <summary>
		///     Creates a new <see cref="Pointer{T}" /> of type <typeparamref name="TNew" />, pointing to
		///     <see cref="Address" />
		/// </summary>
		/// <typeparam name="TNew">Type to point to</typeparam>
		/// <returns>A new <see cref="Pointer{T}" /> of type <typeparamref name="TNew" /></returns>
		public Pointer<TNew> Cast<TNew>() => new Pointer<TNew>(Address);

		public Pointer<byte> Cast() => Cast<byte>();

		public object CastAny(Type type)
		{
			var cast = GetType().GetMethods().First(f => f.Name == nameof(Cast) && f.IsGenericMethod);
			var ptr  = ReflectionUtil.InvokeGenericMethod(cast, this, new[] {type});
			return ptr;
		}

		#endregion

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

		#region Implicit and explicit conversions

		public static explicit /*implicit*/ operator bool(Pointer<T> ptr) => !ptr.IsNil;

		public static implicit operator Pointer<T>(Pointer<byte> v) => v.Address;

		public static implicit operator Pointer<T>(void* v) => new Pointer<T>(v);

		public static implicit operator Pointer<T>(IntPtr p) => new Pointer<T>(p.ToPointer());

		public static explicit operator Pointer<T>(long l) => new Pointer<T>((void*) l);

		public static explicit operator Pointer<T>(ulong ul) => new Pointer<T>((void*) ul);

		public static explicit operator void*(Pointer<T> ptr) => ptr.ToPointer();

		public static explicit operator long(Pointer<T> ptr) => ptr.ToInt64();

		public static explicit operator ulong(Pointer<T> ptr) => ptr.ToUInt64();

		public static explicit operator Pointer<byte>(Pointer<T> ptr) => ptr.ToPointer();

		#endregion


		#region Arithmetic

		public static Pointer<T> operator +(Pointer<T> left, long right)
		{
			return (void*) (left.ToInt64() + right);
		}

		public static Pointer<T> operator -(Pointer<T> left, long right)
		{
			return (void*) (left.ToInt64() - right);
		}

		public static Pointer<T> operator +(Pointer<T> left, Pointer<T> right)
		{
			return (void*) (left.ToInt64() + right.ToInt64());
		}

		public static Pointer<T> operator -(Pointer<T> left, Pointer<T> right)
		{
			return (void*) (left.ToInt64() - right.ToInt64());
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void* OffsetFast(int elemCnt) => (void*) ((long) m_value + (CompleteSize(elemCnt)));

		/// <summary>
		///     Increment <see cref="Address" /> by the specified number of bytes
		/// </summary>
		/// <param name="byteCnt">Number of bytes to add</param>
		/// <returns>
		///     <c>this</c>
		/// </returns>
		public Pointer<T> Add(long byteCnt = 1)
		{
//			m_value = PointerUtil.Add(m_value, bytes).ToPointer();
//			return this;

			long val = ToInt64() + byteCnt;
			this = (void*) val;
			return this;
		}


		/// <summary>
		///     Decrement <see cref="Address" /> by the specified number of bytes
		/// </summary>
		/// <param name="byteCnt">Number of bytes to subtract</param>
		/// <returns>
		///     <c>this</c>
		/// </returns>
		public Pointer<T> Subtract(long byteCnt = 1) => Add(-byteCnt);


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
				case PointerFormat.FORMAT_ARRAY:
				{
					var rg = new string[PointerFormat.ArrayCount];

					for (int i = 0; i < rg.Length; i++) {
						rg[i] = AddressOfIndex(i).ToStringSafe();
					}


					return String.Join(StringConstants.JOIN_COMMA, rg);
				}

				case PointerFormat.FORMAT_INT:
					return ToInt64().ToString();
				case PointerFormat.FORMAT_OBJ:
					return ToStringSafe();
				case PointerFormat.FORMAT_PTR:
					return Hex.ToHex(ToInt64());

				case PointerFormat.FORMAT_BOTH:
				{
					string thisStr = ToStringSafe();

					string typeName = typeof(T).ContainsAnyGenericParameters()
						? SystemFormatting.GenericName(typeof(T))
						: typeof(T).Name;

					string typeNameDisplay = IsCharPointer() ? PointerFormat.CHAR_PTR : typeName;

					return String.Format("{0} @ {1}: {2}", typeNameDisplay, Hex.ToHex(Address),
					                     thisStr.Contains(Environment.NewLine)
						                     ? Environment.NewLine + thisStr
						                     : thisStr);
				}

				default:
					goto case PointerFormat.FORMAT_OBJ;
			}
		}

		public string ToStringAlt()
		{
			if (IsNull)
				return StringConstants.NULL_STR;




			return null;
		}

		public string ToStringSafe()
		{
			// todo: rewrite this

			if (IsNull)
				return StringConstants.NULL_STR;

			if (RtInfo.IsInteger<T>())
				return String.Format(PointerFormat.VAL_FMT, Reference, Hex.TryCreateHex(Reference));

			/* Special support for C-string */
			if (IsCharPointer())
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

				if (typeof(T).IsIListType() && typeof(T) != typeof(string))
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