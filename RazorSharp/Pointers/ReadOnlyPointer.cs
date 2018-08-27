#region

using System;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using RazorCommon;
using RazorCommon.Extensions;
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
	///     <para>Compared to <see cref="Pointer{T}" />, <see cref="Address" /> is <c>readonly</c>. </para>
	///     <para>Similar to <code>T *const ptr = &amp;var </code>in C/C++.</para>
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
	public unsafe struct ReadOnlyPointer<T> : IPointer<T>
	{
		/// <summary>
		///     <para>The address we're pointing to.</para>
		///     <para>We want this to be the only field so it can be represented as a pointer in memory.</para>
		/// </summary>
		private readonly void* m_value;

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
			set => throw new Exception();
		}

		public int ElementSize => Unsafe.SizeOf<T>();

		public bool IsNull => m_value == null;

		public bool IsAligned => MMemory.IsAligned(Address);

		#endregion

		#region Constructors

		public ReadOnlyPointer(IntPtr p) : this(p.ToPointer()) { }

		public ReadOnlyPointer(void* v)
		{
			m_value = v;
		}

		public ReadOnlyPointer(long v)
		{
			m_value = (void*) v;
		}

		public ReadOnlyPointer(ref T t)
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


		public void Write<TType>(TType t, int elemOffset = 0)
		{
			MMemory.Write(Offset<TType>(elemOffset), 0, t);
		}


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

		#endregion

		#region Operators

		public static implicit operator ReadOnlyPointer<T>(void* v)
		{
			return new ReadOnlyPointer<T>(v);
		}

		public static implicit operator ReadOnlyPointer<T>(IntPtr p)
		{
			return new ReadOnlyPointer<T>(p.ToPointer());
		}

		public static explicit operator void*(ReadOnlyPointer<T> ptr)
		{
			return ptr.Address.ToPointer();
		}

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

			return obj is ReadOnlyPointer<T> && Equals((ReadOnlyPointer<T>) obj);
		}

		public override int GetHashCode()
		{
			return unchecked((int) (long) m_value);
		}


		public static bool operator ==(ReadOnlyPointer<T> left, ReadOnlyPointer<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ReadOnlyPointer<T> left, ReadOnlyPointer<T> right)
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