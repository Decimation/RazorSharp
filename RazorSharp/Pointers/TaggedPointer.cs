using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RazorCommon;

namespace RazorSharp.Pointers
{

	using intptr_t = Int64;
	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;


	public static class PointerSettings
	{
		/// <summary>
		/// When <c>true</c>, <see cref="TaggedPointer{T}.Tag"/> will retain its value
		/// when the pointer <see cref="TaggedPointer{T}.Pointer"/> is changed. When
		/// <c>false</c>, <see cref="TaggedPointer{T}.Tag"/> will be set to <c>0</c>
		/// when the pointer <see cref="TaggedPointer{T}.Pointer"/> is changed.
		/// </summary>
		public static bool RetainTagValue;

		static PointerSettings()
		{
			RetainTagValue = false;
		}
	}

	// todo
	/// <summary>
	/// https://nikic.github.io/2012/02/02/Pointer-magic-for-efficient-dynamic-value-representations.html
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public unsafe struct TaggedPointer<T>
	{
		/// <summary>
		/// <c>8</c> for 64-bit
		/// </summary>
		private const int ALIGNED_TO = 8;

		// for 8 byte alignment tagMask = alignedTo - 1 = 8 - 1 = 7 = 0b111
		// i.e. the lowest three bits are set, which is where the tag is stored
		private const intptr_t TAG_MASK = ALIGNED_TO - 1;

		// pointerMask is the exact contrary: 0b...11111000
		// i.e. all bits apart from the three lowest are set, which is where the pointer is stored
		private const intptr_t POINTER_MASK = ~TAG_MASK;


		private Pointer<T> m_pAsPointer;

		public Pointer<T> Pointer {
			get => m_pAsPointer & POINTER_MASK;
			set {
				int oldTagValue = 0;

				if (PointerSettings.RetainTagValue) {
					oldTagValue = Tag;
				}

				m_pAsPointer =  value;
				m_pAsPointer |= oldTagValue;
			}
		}

		public int Tag {
			get => (m_pAsPointer & TAG_MASK).ToInt32();
			set {
				// make sure that the tag isn't too large
				Debug.Assert((value & POINTER_MASK) == 0);
				m_pAsPointer |= value;
			}
		}

		public TaggedPointer(Pointer<T> ptr, int tag = 0)
		{
			m_pAsPointer = ptr | tag;

			set(ptr, tag);
		}


		private void set(Pointer<T> ptr, int tag = 0)
		{
			// make sure that the pointer really is aligned
//			Debug.Assert(CSUnsafe.Read<intptr_t>((ptr & TAG_MASK).ToPointer())==0);

			// make sure that the tag isn't too large
//			Debug.Assert((tag & POINTER_MASK).ToInt64()==0);
			m_pAsPointer =  ptr;
			m_pAsPointer |= tag;
		}


		public override string ToString()
		{
			var table = new ConsoleTable("Pointer", "Tag");
			table.AddRow(Hex.ToHex(Pointer.Address), Tag);
			return table.ToMarkDownString();
		}
	}

}