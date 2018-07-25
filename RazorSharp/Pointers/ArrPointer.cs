using System;

namespace RazorSharp.Pointers
{
	//todo: use stack ptr to make ArrayPointer
	// which won't require pinning
	// then fuse the 2 types together
	// for an all-in-one
	/*public class ArrPointer<T> : Pointer<T>
	{
		private class ArrPointerMetadata : PointerMetadata
		{
			internal bool HasStackPtr { get; }

			protected internal ArrPointerMetadata(int elementSize, bool isDecayed) : base(elementSize, isDecayed) { }
			internal ArrPointerMetadata(int elementSize, bool isDecayed, bool hasStackPtr) : base(elementSize) { }
		}

		public override T this[int index] {
			get => base[index];
			set => base[index] = value;
		}

		private IntPtr Heap {
			get {

			}
		}

		private ArrPointerMetadata Metadata => (ArrPointerMetadata) m_metadata;

		public override IntPtr Address {
			get {
				if (Metadata.HasStackPtr) {

				}
			}
			set;
		}

		public ArrPointer(ref T t) : base(ref t,new ArrPointerMetadata(Unsafe.SizeOf<T>(), false, true))
		{
			m_metadata = ;
		}
	}*/

}