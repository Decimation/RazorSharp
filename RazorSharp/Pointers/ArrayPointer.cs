using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorSharp.Utilities;

namespace RazorSharp.Pointers
{

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	//todo: fix bounds checking FUCK
	public unsafe class ArrayPointer<T> : Pointer<T>
	{


		#region Fields and Accessors

		/// <summary>
		/// Strings require special length calculation.
		/// </summary>
		private bool m_isString;

		/// <summary>
		/// A pointer to the Int32 of the size.
		/// </summary>
		private IntPtr m_sizePtr {
			get {
				if (m_isString) {
					// an Int32 is the first field in a string
					// indicating the number of the elements
					return Origin - sizeof(int);
				}
				else {
					// The lowest DWORD of a QWORD is the length of the array
					return Origin - sizeof(long);
				}
			}
		}

		private IntPtr Origin { get; }


		/// <summary>
		/// Offset relative to the origin address.
		/// This is only updated by pointer arithmetic, not by indexing.
		/// </summary>
		public int Offset { get; private set; }

		/// <summary>
		/// Starting index
		/// </summary>
		public int Start => -Offset;

		/// <summary>
		/// Ending index
		/// </summary>
		public int End => Start + (Count - 1);


		/// <summary>
		/// Returns the heap address of the array's elements
		/// </summary>
		public IntPtr FirstElement {
			get {
				// move back the indexes
				return Address - (Offset * ElementSize);
			}
		}

		private IntPtr LastElement {
			get { return FirstElement + ((Count - 1) * ElementSize); }
		}

		public virtual int Count {
			get { return Marshal.ReadInt32(m_sizePtr); }
		}

		public override T this[int index] {
			get {
				AddressInBounds(index);
				IndexBoundsCheck(index);
				return base[index];
			}
			set {
				AddressInBounds(index);
				IndexBoundsCheck(index);
				base[index] = value;
			}
		}

		#endregion

		#region Bounds checking

		private static bool IsWithin(long value, long minimum, long maximum)
		{
			return value >= minimum && value <= maximum;
		}

		private void AddressInBounds(int indexerIndex = 0)
		{
			long start   = FirstElement.ToInt64();
			long current = Address.ToInt64() + (indexerIndex * ElementSize);
			long end     = LastElement.ToInt64();

			string b = $"Address {current:X}, start: {start:X}, end: {end:X}";

			// This is for isolated incidents when iterators
			// and pointer arithmetic move past the end by 1 element.
			//
			// So we'll automatically move to the last element instead, rather
			// than throwing an exception, just for convenience's sake.
			if (current == start - ElementSize) {
				MoveToStart();
				return;
			}

			// ...and vice versa
			if (current == end + ElementSize) {
				MoveToEnd();
				return;
			}

			if (!IsWithin(current, start, end)) {
				throw new Exception(b);
			}


			//Console.WriteLine(b);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void IndexBoundsCheck(int index)
		{
			string bounds = $"Index {index} | bounds [{Start} - {End}]";


			if (index == End) {
				//Logger.Log(Level.Warning, Flags.Pointer, "Index {0} == {1}, moving to end", index, End);
				//MoveToEnd();
				return;
			}


			else if (index == Start) {
				//Logger.Log(Level.Warning, Flags.Pointer, "Index {0} == {1}, moving to start", index, Start);
				//MoveToStart();
				return;
			}


			else if (!IsWithin(index, Start, End)) {
				throw new IndexOutOfRangeException(bounds);
			}
		}

		public void MoveToStart()
		{
			Address = FirstElement;
			Offset  = 0;
			//Console.WriteLine("Moved to start");
		}

		private void MoveToEnd()
		{
			Address = LastElement;
			Offset  = Count - 1;
			//Console.WriteLine("Moved to end");
		}

		#endregion


		#region Overrides

		protected override ConsoleTable ToTable()
		{
			var table = base.ToTable();
			table.AddRow("Count", Count);
			table.AddRow("Index", Offset);


			table.AddRow("Size ptr", Hex.ToHex(m_sizePtr));
			table.AddRow("Address", Hex.ToHex(Address));
			table.AddRow("Start", Start);
			table.AddRow("End", End);
			table.AddRow("First Element", Hex.ToHex(FirstElement));
			table.AddRow("Last element", Hex.ToHex(LastElement));
			return table;
		}

		protected override ConsoleTable ToElementTable(int length)
		{
			var table = new ConsoleTable("Address", "Offset", "Value");

			for (int i = Start; i <= End; i++) {
				table.AddRow(Hex.ToHex(OffsetIndex(i)), i, this[i]);
			}

			return table;
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

		private void PtrInRange(IntPtr val)
		{


			if (!IsWithin(val.ToInt64(), FirstElement.ToInt64(), LastElement.ToInt64())) {

				if (val == LastElement + ElementSize) {
					MoveToEnd();
				}
				else if (val == FirstElement + ElementSize) {
					MoveToStart();
				}
				else throw new Exception(string.Format("{0} out of {1}, {2}", Hex.ToHex(val), Hex.ToHex(FirstElement), Hex.ToHex(LastElement)));
			}
		}

		protected override void Increment(int cnt = 1)
		{
			var newAddr = Address + (cnt * ElementSize);
			PtrInRange(newAddr);
			Offset += cnt;

			//IndexBoundsCheck(m_index);

			//IndexBoundsCheck(cnt);


			base.Increment(cnt);
			//AddressInBounds();
		}

		protected override void Decrement(int cnt = 1)
		{
			var newAddr = Address - (cnt * ElementSize);
			PtrInRange(newAddr);
			Offset -= cnt;


			//IndexBoundsCheck(m_index);

			//IndexBoundsCheck(cnt);

			base.Decrement(cnt);
			//AddressInBounds();

		}

		#endregion


		#region Constructors

		private protected ArrayPointer(IntPtr pHeap, PointerMetadata metadata, bool isString) :
			base(pHeap, metadata)
		{
			m_isString = isString;
			Origin     = pHeap;
		}


		//
		// FUCK
		//

		private static ArrayPointer<T> CreateDecayedPointer(IntPtr pHeap, bool isString)
		{
			PointerMetadata meta = new PointerMetadata(Unsafe.SizeOf<T>(), true);
			var             p    = new ArrayPointer<T>(pHeap, meta, isString);


			return p;
		}

		#endregion


		#region Operators

		#region Arithmetic

		public static ArrayPointer<T> operator +(ArrayPointer<T> p, int i)
		{
			//p.IncrementBytes(i * p.ElementSize);
			p.Increment(i);
			return p;
		}

		public static ArrayPointer<T> operator -(ArrayPointer<T> p, int i)
		{
			//p.DecrementBytes(i * p.ElementSize);
			p.Decrement(i);
			return p;
		}

		public static ArrayPointer<T> operator ++(ArrayPointer<T> p)
		{
			//p.IncrementBytes(p.ElementSize);
			p.Increment();
			return p;
		}

		public static ArrayPointer<T> operator --(ArrayPointer<T> p)
		{
			//p.DecrementBytes(Unsafe.SizeOf<T>());
			p.Decrement();
			return p;
		}

		#endregion

		#region Implicit

		// Implicit operators will have their pointers copied so
		// we can't get an accurate stack pointer, but we CAN get a heap pointer
		//
		// However this means we may need to pin the object


		public static implicit operator ArrayPointer<T>(string s)
		{
			Assertion.AssertType<char, T>();


			return CreateDecayedPointer(Unsafe.AddressOfHeap(ref s, OffsetType.StringData),
				true);
		}

		public static implicit operator ArrayPointer<T>(T[] arr)
		{
			return CreateDecayedPointer(Unsafe.AddressOfHeap(ref arr, OffsetType.ArrayData),
				false);
		}

		#endregion

		#endregion


	}

}