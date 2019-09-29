using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using RazorSharp.Interop;
using SimpleSharp.Diagnostics;
using SimpleSharp.Strings;
using SimpleSharp.Strings.Formatting;
using SimpleSharp.Utilities;

namespace RazorSharp.Memory.Pointers
{
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
		public bool IsNull => this == Mem.Nullptr;

		/// <summary>
		///     Whether the value being pointed to is <c>default</c> or <c>null</c> bytes,
		/// or this pointer is <c>null</c>.
		/// </summary>
		public bool IsNil => RuntimeInfo.IsNil(Reference);

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

		[Pure]
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

		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void* OffsetFast(int elemCnt) => (void*) ((long) m_value + (FullSize(elemCnt)));

		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int FullSize(int elemCnt) => elemCnt * ElementSize;

		/// <summary>
		///     Returns the element index of a pointer relative to <see cref="Address" />
		/// </summary>
		/// <param name="current">Current pointer (high address)</param>
		/// <returns>The index</returns>
		[Pure]
		public int OffsetIndex(Pointer<byte> current)
		{
			long delta = current.ToInt64() - ToInt64();
			return (int) delta / ElementSize;
		}

		#endregion

		#region Read / write

		#region Pointer

		[Pure]
		public Pointer<byte> ReadPointer(int elemOffset = 0) => ReadPointer<byte>(elemOffset);

		[Pure]
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

		private MethodInfo GetMethod(Type t, string name, out object ptr)
		{
			ptr = CastAny(t);
			var fn = ptr.GetType().GetMethod(name);
			return fn;
		}

		public void WriteAny(Type type, object value, int elemOffset = 0)
		{
			var fn = GetMethod(type, nameof(Write), out var ptr);
			fn.Invoke(ptr, new object[] {value, elemOffset});
		}

		public object ReadAny(Type type, int elemOffset = 0)
		{
			var fn = GetMethod(type, nameof(Read), out var ptr);
			return fn.Invoke(ptr, new object[] {elemOffset});
		}

		#endregion


		/// <summary>
		///     Writes a value of type <typeparamref name="T" /> to <see cref="Address" />
		/// </summary>
		/// <param name="value">Value to write</param>
		/// <param name="elemOffset">Element offset (of type <typeparamref name="T" />)</param>
		public void Write(T value, int elemOffset = 0) => Unsafe.Write(OffsetFast(elemOffset), value);


		/// <summary>
		///     Reads a value of type <typeparamref name="T" /> from <see cref="Address" />
		/// </summary>
		/// <param name="elemOffset">Element offset (of type <typeparamref name="T" />)</param>
		/// <returns>The value read from the offset <see cref="Address" /></returns>
		[Pure]
		public T Read(int elemOffset = 0) => Unsafe.Read<T>(OffsetFast(elemOffset));

		/// <summary>
		///     Reinterprets <see cref="Address" /> as a reference to a value of type <typeparamref name="T" />
		/// </summary>
		/// <param name="elemOffset">Element offset (of type <typeparamref name="T" />)</param>
		/// <returns>A reference to a value of type <typeparamref name="T" /></returns>
		[Pure]
		public ref T AsRef(int elemOffset = 0) => ref Unsafe.AsRef<T>(OffsetFast(elemOffset));

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

		[Pure]
		private static bool IsCharPointer() => typeof(T) == typeof(char);

		[Pure]
		public Pointer<T> AddressOfIndex(int index) => OffsetFast(index);

		/// <summary>
		/// Zeros <paramref name="elemCnt"/> elements.
		/// </summary>
		/// <param name="elemCnt">Number of elements to zero</param>
		public void Clear(int elemCnt = 1)
		{
			for (int i = 0; i < elemCnt; i++)
				this[i] = default;
		}

		/// <summary>
		/// Zeros <paramref name="byteCnt"/> bytes.
		/// </summary>
		/// <param name="byteCnt">Number of bytes to zero</param>
		public void ClearBytes(int byteCnt)
		{
			var bytePtr = Cast();
			for (int i = 0; i < byteCnt; i++)
				bytePtr[i] = default;
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
			var ptr  = Functions.CallGenericMethod(cast, type, this);
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
		[Pure]
		public int ToInt32() => (int) m_value;

		/// <summary>
		///     Converts <see cref="Address" /> to a 64-bit signed integer.
		/// </summary>
		/// <returns></returns>
		[Pure]
		public long ToInt64() => (long) m_value;

		/// <summary>
		///     Converts <see cref="Address" /> to a 64-bit unsigned integer.
		/// </summary>
		/// <returns></returns>
		[Pure]
		public ulong ToUInt64() => (ulong) m_value;

		/// <summary>
		///     Converts <see cref="Address" /> to a 32-bit unsigned integer.
		/// </summary>
		/// <returns></returns>
		[Pure]
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

		/// <summary>
		///     Increment <see cref="Address" /> by the specified number of bytes
		/// </summary>
		/// <param name="byteCnt">Number of bytes to add</param>
		/// <returns>
		///     A new <see cref="Pointer{T}"/> with <paramref name="byteCnt"/> bytes added
		/// </returns>
		[Pure]
		public Pointer<T> Add(long byteCnt = 1)
		{
//			m_value = PointerUtil.Add(m_value, bytes).ToPointer();
//			return this;

			long val = ToInt64() + byteCnt;
			return (void*) val;
		}


		/// <summary>
		///     Decrement <see cref="Address" /> by the specified number of bytes
		/// </summary>
		/// <param name="byteCnt">Number of bytes to subtract</param>
		/// <returns>
		///     A new <see cref="Pointer{T}"/> with <paramref name="byteCnt"/> bytes subtracted
		/// </returns>
		[Pure]
		public Pointer<T> Subtract(long byteCnt = 1) => Add(-byteCnt);


		/// <summary>
		///     Increment <see cref="Address" /> by the specified number of elements
		/// </summary>
		/// <param name="elemCnt">Number of elements</param>
		/// <returns>
		///     A new <see cref="Pointer{T}"/> with <paramref name="elemCnt"/> elements incremented
		/// </returns>
		[Pure]
		public Pointer<T> Increment(int elemCnt = 1)
		{
			return OffsetFast(elemCnt);
		}


		/// <summary>
		///     Decrement <see cref="Address" /> by the specified number of elements
		/// </summary>
		/// <param name="elemCnt">Number of elements</param>
		/// <returns>
		///     A new <see cref="Pointer{T}"/> with <paramref name="elemCnt"/> elements decremented
		/// </returns>
		[Pure]
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
			return p.Increment(i);
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
			return p.Decrement(i);
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
			return p.Increment();
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
			return p.Decrement();
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
		///     See <see cref="PointerSettings" /> for format options
		/// </param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		[Pure]
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrEmpty(format))
				format = PointerSettings.DefaultFormat;

			if (formatProvider == null)
				formatProvider = CultureInfo.CurrentCulture;

			switch (format.ToUpperInvariant()) {
				case PointerSettings.FORMAT_ARRAY:
				{
					var rg = new string[PointerSettings.ArrayCount];

					for (int i = 0; i < rg.Length; i++) {
						rg[i] = AddressOfIndex(i).ToStringSafe();
					}


					return String.Join(StringConstants.JOIN_COMMA, rg);
				}

				case PointerSettings.FORMAT_INT:
					return ToInt64().ToString();
				case PointerSettings.FORMAT_OBJ:
					return ToStringSafe();
				case PointerSettings.FORMAT_PTR:
					return Hex.ToHex(ToInt64());

				case PointerSettings.FORMAT_BOTH:
				{
					string thisStr = ToStringSafe();

					string typeName = typeof(T).ContainsAnyGenericParameters()
						? SystemFormatting.GenericName(typeof(T))
						: typeof(T).Name;

					string typeNameDisplay = IsCharPointer() ? PointerSettings.CHAR_PTR : typeName;

					return String.Format("{0} @ {1}: {2}", typeNameDisplay, Hex.ToHex(Address),
					                     thisStr.Contains(Environment.NewLine)
						                     ? Environment.NewLine + thisStr
						                     : thisStr);
				}

				default:
					goto case PointerSettings.FORMAT_OBJ;
			}
		}

		public string ToStringSafe()
		{
			// todo: rewrite this

			if (IsNull)
				return StringConstants.NULL_STR;

			if (((MetaType) typeof(T)).IsInteger)
				return String.Format(PointerSettings.VAL_FMT, Reference, Hex.TryCreateHex(Reference));

			/* Special support for C-string */
//			if (IsCharPointer())
//				return ReadString(StringTypes.UNI);

			/*if (typeof(T) == typeof(sbyte)) {
				return inst.ReadString(StringTypes.AnsiStr);
			}*/


			if (!RuntimeInfo.IsStruct<T>()) {
				Pointer<byte> heapPtr = ReadPointer();
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
				return String.Format(PointerSettings.VAL_FMT, valueStr, heapPtr.ToString(PointerSettings.FORMAT_PTR));
			}


			return Reference.ToString();
		}

		[Pure]
		public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);

		[Pure]
		public override string ToString() => ToString(PointerSettings.DefaultFormat);

		#endregion
	}
}