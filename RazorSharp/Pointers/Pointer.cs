using System;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using RazorCommon;
using RazorCommon.Extensions;
using RazorSharp.Utilities;

namespace RazorSharp.Pointers
{

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;


	/// <summary>
	/// A pointer to any type.
	/// Supports pointer arithmetic and other pointer operations.
	///
	/// Note: This does not pin references, but that is only a problem if the type is a decayed type.
	/// </summary>
	/// <typeparam name="T">Type this pointer points to. If just raw memory, use byte.</typeparam>
	public unsafe class Pointer<T> : IFormattable
	{
		/// <summary>
		/// Contains metadata for operating Pointer
		/// </summary>
		protected class PointerMetadata
		{

			/// <summary>
			/// The element size (size of type pointed to)
			/// </summary>
			internal int ElementSize { get; }

			/// <summary>
			/// Whether the type was decayed into a pointer
			///
			/// (i.e. a string or array was implicitly cast into a pointer)
			/// </summary>
			internal bool IsDecayed { get; }


			protected internal PointerMetadata(int elementSize, bool isDecayed)
			{
				ElementSize = elementSize;
				IsDecayed   = isDecayed;
			}

			internal PointerMetadata(int elementSize) : this(elementSize, false) { }
		}

		private                    IntPtr          m_addr;
		private protected readonly PointerMetadata m_metadata;

		/// <summary>
		/// Whether this Pointer was created from implicit array conversion.
		///
		/// Note: Unless the type is pinned or this points to a stack pointer, this Pointer will become
		/// invalid when the GC compacts the heap as the pointer points
		/// to heap memory
		/// </summary>
		public bool IsDecayed => m_metadata.IsDecayed;

		/// <summary>
		/// The size of the type being pointed to.
		/// </summary>
		public int ElementSize => m_metadata.ElementSize;

		public virtual IntPtr Address {
			get => m_addr;
			set => m_addr = value;
		}

		public bool IsNull => m_addr == IntPtr.Zero;

		public virtual T Value {
			get => CSUnsafe.Read<T>(m_addr.ToPointer());
			set => CSUnsafe.Write(m_addr.ToPointer(), value);
		}

		public virtual T this[int index] {
			get => CSUnsafe.Read<T>(OffsetIndex(index));
			set => CSUnsafe.Write(OffsetIndex(index), value);
		}

		#region Constructors

		public Pointer(IntPtr p) : this(p, new PointerMetadata(Unsafe.SizeOf<T>())) { }

		// Base constructor
		private protected Pointer(IntPtr p, PointerMetadata metadata)
		{
			m_addr     = p;
			m_metadata = metadata;
		}

		private protected Pointer(PointerMetadata metadata)
		{
			m_metadata = metadata;
		}

		public Pointer(void* v) : this((IntPtr) v) { }

		public Pointer(ref T t) : this(Unsafe.AddressOf(ref t)) { }

		private static Pointer<T> CreateDecayedPointer(IntPtr pHeap)
		{
			PointerMetadata meta = new PointerMetadata(Unsafe.SizeOf<T>(), true);
			return new Pointer<T>(pHeap, meta);
		}

		#endregion

		#region Methods

		protected virtual ConsoleTable ToTable()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Address", Hex.ToHex(Address));

			if (Assertion.Throws<NullReferenceException>(delegate { table.AddRow("Value", Value); })) {
				table.AddRow("Value", "(null)");
			}


			table.AddRow("Type", typeof(T).Name);

			if (Assertion.Throws<NullReferenceException>(delegate { table.AddRow("this[0]", this[0]); })) {
				table.AddRow("this[0]", "(null)");
			}


			table.AddRow("Null", IsNull);
			table.AddRow("Element size", m_metadata.ElementSize);
			table.AddRow("Decayed", m_metadata.IsDecayed);
			return table;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void* OffsetIndex(int index)
		{
			return (void*) ((long) m_addr + (m_metadata.ElementSize * index));
		}

		public long ToInt64()
		{
			return (long) m_addr;
		}

		public int ToInt32()
		{
			return checked((int) m_addr);
		}

		protected virtual void Increment(int cnt = 1)
		{
			IncrementBytes(cnt * ElementSize);
		}

		protected virtual void Decrement(int cnt = 1)
		{
			DecrementBytes(cnt * ElementSize);
		}

		protected virtual ConsoleTable ToElementTable(int length)
		{
			var table = new ConsoleTable("Address", "Offset", "Value");
			for (int i = 0; i < length; i++) {
				table.AddRow(Hex.ToHex(OffsetIndex(i)), i, this[i]);
			}

			return table;
		}

		/// <summary>
		/// Increment the pointer by n bytes.
		/// </summary>
		/// <param name="i">Number of bytes to increment the pointer by.</param>
		private void IncrementBytes(int i)
		{
			switch (IntPtr.Size) {
				case 8:
					long l = ToInt64() + i;
					Address = (IntPtr) l;
					break;
				case 4:
					int n = ToInt32() + i;
					Address = (IntPtr) n;
					break;
			}
		}

		/// <summary>
		/// Decrement the pointer by n bytes.
		/// </summary>
		/// <param name="i">Number of bytes to decrement the pointer by.</param>
		private void DecrementBytes(int i)
		{
			switch (IntPtr.Size) {
				case 8:
					long l = ToInt64() - i;
					Address = (IntPtr) l;
					break;
				case 4:
					int n = ToInt32() - i;
					Address = (IntPtr) n;
					break;
			}
		}

		#endregion

		#region Operators

		#region Implicit

		// Special support for strings
		public static implicit operator Pointer<T>(string s)
		{
			Assertion.AssertType<char, T>();
			return CreateDecayedPointer(Unsafe.AddressOfHeap(ref s, OffsetType.StringData));
		}

		public static implicit operator Pointer<T>(T[] arr)
		{
			return CreateDecayedPointer(Unsafe.AddressOfHeap(ref arr, OffsetType.ArrayData));
		}

		public static implicit operator Pointer<T>(void* v)
		{
			return new Pointer<T>(v);
		}

		public static implicit operator Pointer<T>(IntPtr v)
		{
			return new Pointer<T>(v.ToPointer());
		}

		#endregion


		#region Arithmetic

		public static Pointer<T> operator +(Pointer<T> p, int i)
		{
			//p.IncrementBytes(i * Unsafe.SizeOf<T>());
			p.Increment(i);
			return p;
		}

		public static Pointer<T> operator -(Pointer<T> p, int i)
		{
			//p.DecrementBytes(i * Unsafe.SizeOf<T>());
			p.Decrement(i);
			return p;
		}

		public static Pointer<T> operator ++(Pointer<T> p)
		{
			//p.IncrementBytes(1 * Unsafe.SizeOf<T>());
			p.Increment();
			return p;
		}

		public static Pointer<T> operator --(Pointer<T> p)
		{
			//p.DecrementBytes(1 * Unsafe.SizeOf<T>());
			p.Decrement();
			return p;
		}

		#endregion

		#endregion

		#region Overrides

		public override bool Equals(object obj)
		{
			if (obj?.GetType() == this.GetType()) {
				Pointer<object> ptr = (Pointer<object>) obj;
				return ptr.Address == this.Address;
			}

			return base.Equals(obj);
		}

		public virtual string ToString(string format)
		{
			return this.ToString(format, CultureInfo.CurrentCulture);
		}


		/// <inheritdoc />
		/// <param name="format">O: Object, P: Pointer, T: Table</param>
		public virtual string ToString(string format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrEmpty(format)) format   = "O";
			if (formatProvider == null) formatProvider = CultureInfo.CurrentCulture;


			/**
			 * @O	Object
			 * @P	Pointer
			 * @T	Table
			 */
			switch (format.ToUpperInvariant()) {
				case "O":
					if (typeof(T).IsIListType()) {
						return Collections.ListToString((IList) Value);
					}

					return Value.ToString();
				case "P":
					return Hex.ToHex(Address);
				case "T":
					return ToTable().ToMarkDownString();
				default:
					goto case "O";
			}
		}

		public override string ToString()
		{
			return ToString("O");
		}

		#endregion

	}

}