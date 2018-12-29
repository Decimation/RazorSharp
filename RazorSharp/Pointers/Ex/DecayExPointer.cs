#region

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorSharp.Common;
using RazorSharp.Utilities;

#endregion

namespace RazorSharp.Pointers.Ex
{

	/// <summary>
	///     This won't work when:
	///     - A string is changed (i.e. concatenations)
	///     - Reference changes
	///     Unlike ExPointer, this supports implicit array-type conversions (decaying)
	///     but doesn't work with the GC. This requires pinning.
	///     - Bounds checking
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Obsolete("Use AllocExPointer", true)]
	internal class DecayExPointer<T> : ExPointer<T>
	{

		private enum FixType
		{
			/// <summary>
			///     Offset was 1 past the bounds, so we moved back
			/// </summary>
			BounceBack,

			/// <summary>
			///     Offset is >1 out of bounds
			/// </summary>
			OutOfBounds,

			/// <summary>
			///     Offset is OK
			/// </summary>
			Verified
		}

		#region Fields and accessors

		/// <summary>
		///     Original heap address
		/// </summary>
		private readonly IntPtr _origin;

		/// <summary>
		///     Offset relative to the origin
		/// </summary>
		private int m_offset;

		/// <summary>
		///     Number of elements in this array
		/// </summary>
		public int Count { get; }

		// If the count changes in either a string or an array
		// its address will very likely change either way,
		// invalidating this pointer,
		// so recalculating it every time is pointless

		//public int Count {
		//	[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//	get { return Marshal.ReadInt32(m_sizePtr); }
		//}

		/// <summary>
		///     Returns the heap address of the array's first element
		/// </summary>
		public IntPtr FirstElement => Address - m_offset * ElementSize;

		/// <summary>
		///     Returns the heap address of the array's last element
		/// </summary>
		private IntPtr LastElement => FirstElement + (Count - 1) * ElementSize;

		/// <summary>
		///     Starting index
		/// </summary>
		public int Start => -m_offset;

		/// <summary>
		///     Ending index
		/// </summary>
		public int End => Start + (Count - 1);

		#endregion

		#region Constructors

		private protected DecayExPointer(IntPtr pHeap, PointerMetadata metadata, bool isString) :
			base(pHeap, metadata)
		{
			_origin = pHeap;

			IntPtr sizePtr;

			// Calculate the size ptr
			if (isString) {
				sizePtr = _origin - sizeof(int);
			}
			else {
				sizePtr = _origin - sizeof(long);
			}

			Count = Marshal.ReadInt32(sizePtr);
		}

		private static DecayExPointer<T> CreateDecayedPointer(IntPtr pHeap, bool isString)
		{
			PointerMetadata   meta = new PointerMetadata(Unsafe.SizeOf<T>(), true);
			DecayExPointer<T> p    = new DecayExPointer<T>(pHeap, meta, isString);


			return p;
		}

		#endregion

		#region Instance methods

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MoveToStart()
		{
			Address  = FirstElement;
			m_offset = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MoveToEnd()
		{
			Address  = LastElement;
			m_offset = Count - 1;
		}

		#endregion

		#region Bounds checking

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureIndexerBounds(int requestedIndex)
		{
			if (requestedIndex > End) {
				throw new IndexOutOfRangeException($"Requested index of {requestedIndex} > {End}");
			}

			if (requestedIndex < Start) {
				throw new IndexOutOfRangeException($"Requested index of {requestedIndex} < {Start}");
			}
		}

		private FixType EnsureOffsetBounds(int requestedOffset = 1)
		{
			// Past the end?
			if (Address.ToInt64() + requestedOffset * ElementSize > LastElement.ToInt64()) {
				// This is for isolated incidents when iterators
				// and pointer arithmetic move past the end by 1 element.
				//
				// So we'll automatically move to the last element instead, rather
				// than throwing an exception, just for convenience's sake.
				if (requestedOffset == 1) {
					MoveToEnd();
					return FixType.BounceBack;
				}

				return FixType.OutOfBounds;
			}

			// Before the start?
			if (Address.ToInt64() + requestedOffset * ElementSize < FirstElement.ToInt64()) {
				// ... and vice versa
				if (requestedOffset == -1) {
					MoveToStart();
					return FixType.BounceBack;
				}

				return FixType.OutOfBounds;
			}

			return FixType.Verified;
		}

		#endregion

		#region Operators

		#region Equality

		protected bool Equals(DecayExPointer<T> other)
		{
			return base.Equals(other) && _origin.Equals(other._origin) && m_offset == other.m_offset &&
			       Count == other.Count;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) {
				return false;
			}

			if (ReferenceEquals(this, obj)) {
				return true;
			}

			if (obj.GetType() != GetType()) {
				return false;
			}

			return Equals((DecayExPointer<T>) obj);
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ _origin.GetHashCode();

				//hashCode = (hashCode * 397) ^ m_offset;
				hashCode = (hashCode * 397) ^ Count;
				return hashCode;
			}
		}

		public static bool operator ==(DecayExPointer<T> left, DecayExPointer<T> right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(DecayExPointer<T> left, DecayExPointer<T> right)
		{
			return !Equals(left, right);
		}

		#endregion


		public static DecayExPointer<T> operator ++(DecayExPointer<T> p)
		{
			p.Increment();
			return p;
		}

		public static DecayExPointer<T> operator --(DecayExPointer<T> p)
		{
			p.Decrement();
			return p;
		}

		#region Implicit

		// Implicit operators will have their stack pointers copied so
		// we can't get an accurate stack pointer, but we CAN get a heap pointer
		//
		// However this means we may need to pin the object


		public static implicit operator DecayExPointer<T>(string s)
		{
			RazorContract.RequiresType<char, T>();


			return CreateDecayedPointer(Unsafe.AddressOfHeap(ref s, OffsetType.StringData).Address,
				true);
		}

		public static implicit operator DecayExPointer<T>(T[] arr)
		{
			return CreateDecayedPointer(Unsafe.AddressOfHeap(ref arr, OffsetType.ArrayData).Address,
				false);
		}

		#endregion

		#endregion

		#region Overrides

		public override T this[int index] {
			get {
				EnsureIndexerBounds(index);
				return base[index];
			}
			set {
				EnsureIndexerBounds(index);
				base[index] = value;
			}
		}

		protected override ConsoleTable ToTable()
		{
			ConsoleTable table = base.ToTable();
			table.AddRow("Start", Start);
			table.AddRow("End", End);
			table.AddRow("Offset", m_offset);
			table.AddRow("First element", Hex.ToHex(FirstElement));
			table.AddRow("Last element", Hex.ToHex(LastElement));

			return table;
		}

		protected override ConsoleTable ToElementTable(int length)
		{
			ConsoleTable table = new ConsoleTable("Address", "Offset", "Value");

			for (int i = Start; i <= End; i++) table.AddRow(Hex.ToHex(PointerUtils.Offset<T>(Address, i)), i, this[i]);

			return table;
		}

		protected override void Increment(int cnt = 1)
		{
			switch (EnsureOffsetBounds(cnt)) {
				case FixType.BounceBack:
					return;

				case FixType.OutOfBounds:
					throw new IndexOutOfRangeException();
				case FixType.Verified:
					m_offset += cnt;
					base.Increment(cnt);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		protected override void Decrement(int cnt = 1)
		{
			switch (EnsureOffsetBounds(-cnt)) {
				case FixType.BounceBack:
					return;
				case FixType.OutOfBounds:
					throw new IndexOutOfRangeException();
				case FixType.Verified:
					m_offset -= cnt;
					base.Decrement(cnt);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public override string ToString(string format)
		{
			return base.ToString(format);
		}

		/// <inheritdoc />
		/// <summary>
		/// </summary>
		/// <param name="format">E: Element table</param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		public override string ToString(string format, IFormatProvider formatProvider)
		{
			switch (format) {
				case "E":
					return ToElementTable(Count).ToMarkDownString();
			}

			return base.ToString(format, formatProvider);
		}

		public override string ToString()
		{
			return base.ToString();
		}

		#endregion

	}

}