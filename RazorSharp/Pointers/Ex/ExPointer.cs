#region

using System;
using System.Collections;
using System.Globalization;
using RazorSharp.Common;
using RazorSharp.Memory;

#endregion

namespace RazorSharp.Pointers.Ex
{

	#region

	#endregion

	/// <summary>
	///     A pointer to any type.
	///     Supports pointer arithmetic and other pointer operations.
	///     <para></para>
	///     If <![CDATA[T]]> is a reference type, pinning is not required as
	///     the pointer contains the stack pointer, meaning the pointer works with the GC.
	///     <para></para>
	///     However, if this pointer points to heap memory, the pointer may become invalidated when
	///     the GC compacts the heap.
	///     - No bounds checking
	///     <para></para>
	/// </summary>
	/// <typeparam name="T">Type this pointer points to. If just raw memory, use byte.</typeparam>
	[Obsolete]
	internal unsafe class ExPointer<T> : IFormattable //, IPointer<T>
	{
		protected readonly PointerMetadata m_metadata;

		private IntPtr m_addr;

		/// <summary>
		///     The size of the type being pointed to.
		/// </summary>
		public int ElementSize => m_metadata.ElementSize;

		public virtual IntPtr Address {
			get => m_addr;
			set => m_addr = value;
		}

		public ref T Reference => ref Mem.AsRef<T>(Address);

		public bool IsNull => m_addr == IntPtr.Zero;

		/// <summary>
		///     Returns the value the pointer is currently pointing to.
		///     This is the equivalent of dereferencing the pointer.
		///     This is equivalent to this[0].
		/// </summary>
		public virtual T Value {
			get => Mem.Read<T>(Address, 0);
			set => Mem.Write(Address, 0, value);
		}

		public virtual T this[int index] {
			get => Mem.Read<T>(PointerUtils.Offset<T>(Address, index), 0);
			set => Mem.Write(PointerUtils.Offset<T>(Address, index), 0, value);
		}

		/// <summary>
		///     Contains metadata for operating ExPointer
		/// </summary>
		protected class PointerMetadata
		{


			protected internal PointerMetadata(int elementSize, bool isDecayed)
			{
				ElementSize = elementSize;
			}

			internal PointerMetadata(int elementSize) : this(elementSize, false) { }

			/// <summary>
			///     The element size (size of type pointed to)
			/// </summary>
			internal int ElementSize { get; }

			protected bool Equals(PointerMetadata m)
			{
				return ElementSize == m.ElementSize;
			}

			public override bool Equals(object obj)
			{
				if (obj.GetType() == GetType()) {
					return Equals((PointerMetadata) obj);
				}

				return false;
			}

			public override int GetHashCode()
			{
				return ElementSize;
			}
		}

		#region Constructors

		public ExPointer(IntPtr p) : this(p, new PointerMetadata(Unsafe.SizeOf<T>())) { }

		// Base constructor
		private protected ExPointer(IntPtr p, PointerMetadata metadata)
		{
			m_addr     = p;
			m_metadata = metadata;
		}

		public ExPointer(void* v) : this((IntPtr) v) { }

		public ExPointer(ref T t) : this(Unsafe.AddressOf(ref t).Address) { }

		#endregion

		#region Methods

		public TNew Peek<TNew>()
		{
			return Mem.Read<TNew>(Address);
		}

		public ExPointer<TNew> Reinterpret<TNew>()
		{
			return new ExPointer<TNew>(Address);
		}

		protected virtual ConsoleTable ToTable()
		{
			ConsoleTable table = new ConsoleTable("Field", "Value");
			table.AddRow("Address", Hex.ToHex(Address));
			table.AddRow("Value", Value);
			table.AddRow("Type", typeof(T).Name);
			table.AddRow("this[0]", this[0]);
			table.AddRow("Null", IsNull);
			table.AddRow("Element size", m_metadata.ElementSize);
			return table;
		}

		public long ToInt64()
		{
			return (long) m_addr;
		}

		public int ToInt32()
		{
			return (int) m_addr;
		}

		protected virtual void Increment(int cnt = 1)
		{
			Address = PointerUtils.Offset<T>(Address, cnt);
		}

		protected virtual void Decrement(int cnt = 1)
		{
			Address = PointerUtils.Offset<T>(Address, -cnt);
		}

		protected virtual ConsoleTable ToElementTable(int length)
		{
			ConsoleTable table = new ConsoleTable("Address", "Index", "Value");
			for (int i = 0; i < length; i++) table.AddRow(Hex.ToHex(PointerUtils.Offset<T>(Address, i)), i, this[i]);

			return table;
		}

		#endregion

		#region Operators

		#region Implicit

		public static implicit operator ExPointer<T>(void* v)
		{
			return new ExPointer<T>(v);
		}

		public static implicit operator ExPointer<T>(IntPtr v)
		{
			return new ExPointer<T>(v.ToPointer());
		}

		#endregion

		#region Arithmetic

		public static ExPointer<T> operator +(ExPointer<T> p, int i)
		{
			//p.IncrementBytes(i * Unsafe.SizeOf<T>());
			p.Increment(i);
			return p;
		}

		public static ExPointer<T> operator -(ExPointer<T> p, int i)
		{
			//p.DecrementBytes(i * Unsafe.SizeOf<T>());
			p.Decrement(i);
			return p;
		}

		public static ExPointer<T> operator ++(ExPointer<T> p)
		{
			//p.IncrementBytes(1 * Unsafe.SizeOf<T>());
			p.Increment();
			return p;
		}

		public static ExPointer<T> operator --(ExPointer<T> p)
		{
			//p.DecrementBytes(1 * Unsafe.SizeOf<T>());
			p.Decrement();
			return p;
		}

		#endregion

		#endregion

		#region Overrides

		#region Equality

		public override bool Equals(object obj)
		{
			if (obj?.GetType() == GetType()) {
				ExPointer<object> ptr = (ExPointer<object>) obj;
				return ptr.Address == Address;
			}

			return false;
		}

		protected bool Equals(ExPointer<T> other)
		{
			return m_addr.Equals(other.m_addr) && m_metadata.Equals(other.m_metadata);
		}

		public override int GetHashCode()
		{
			unchecked {
				return (m_addr.GetHashCode() * 397) ^ m_metadata.GetHashCode();
			}
		}

		public static bool operator ==(ExPointer<T> left, ExPointer<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ExPointer<T> left, ExPointer<T> right)
		{
			return !left.Equals(right);
		}

		#endregion

		public virtual string ToString(string format)
		{
			return ToString(format, CultureInfo.CurrentCulture);
		}

		/// <inheritdoc />
		/// <param name="format">O: Object, P: Pointer, T: Table</param>
		public virtual string ToString(string format, IFormatProvider formatProvider)
		{
			if (string.IsNullOrEmpty(format)) {
				format = "O";
			}

			if (formatProvider == null) {
				formatProvider = CultureInfo.CurrentCulture;
			}


			/**
			 * @O	Object
			 * @P	Pointer
			 * @T	Table
			 */
			switch (format.ToUpperInvariant()) {
				case "O":
					if (typeof(T).IsIListType()) {
						return Collections.ToString((IList) Value);
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