#region

using System;
using System.Runtime.InteropServices;
using RazorSharp.Clr.Structures.EE;

#endregion

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace RazorSharp.Clr.Structures
{
	#region

	using DWORD = UInt32;

	#endregion

	/// <summary>
	///     <para>Used only for <see cref="EEClass" /></para>
	///     <remarks>
	///         <para>Source: /src/vm/packedfields.inl</para>
	///     </remarks>
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct PackedDWORDFields
	{
		/// <summary>
		///     <see cref="EEClassFieldId.COUNT" /> == 11
		/// </summary>
		private const int kLength = 11;

		private const int kMaxLengthBits = 5;
		private const int kBitsPerDWORD  = 32;

		[FieldOffset(0)]
		private fixed DWORD m_rgUnpackedFields[kLength];

		[FieldOffset(0)]
		private fixed DWORD m_rgPackedFields[1];

		// Get the value of the given field when the structure is in its unpacked state.
		internal DWORD GetUnpackedField(DWORD dwFieldIndex)
		{
			fixed (PackedDWORDFields* p = &this) {
				return p->m_rgUnpackedFields[dwFieldIndex];
			}
		}


		internal DWORD GetPackedField(DWORD dwFieldIndex)
		{
			DWORD dwOffset = 0;
			for (DWORD i = 0; i < dwFieldIndex; i++)
				dwOffset += kMaxLengthBits + BitVectorGet(dwOffset, kMaxLengthBits) +
				            1; // +1 since size is [1,32] not [0,31]

			// The next kMaxLengthBits bits contain the length in bits of the field we want (-1 due to the way we
			// encode the length).
			DWORD dwFieldLength = BitVectorGet(dwOffset, kMaxLengthBits) + 1;
			dwOffset += kMaxLengthBits;

			// Grab the field value.
			DWORD dwReturn = BitVectorGet(dwOffset, dwFieldLength);


			return dwReturn;
		}

		private DWORD BitVectorGet(DWORD dwOffset, DWORD dwLength)
		{
			// Calculate the start and end naturally aligned DWORDs from which the value will come.
			DWORD dwStartBlock = dwOffset / kBitsPerDWORD;
			DWORD dwEndBlock   = (dwOffset + dwLength - 1) / kBitsPerDWORD;
			if (dwStartBlock == dwEndBlock) {
				// Easy case: the new value fits entirely within one aligned DWORD. Compute the number of bits
				// we'll need to shift the extracted value (to the right) and a mask of the bits that will be
				// extracted in the destination DWORD.
				DWORD dwValueShift = dwOffset % kBitsPerDWORD;
				DWORD dwValueMask  = ((1U << (int) dwLength) - 1) << (int) dwValueShift;


				// Mask out the bits we want and shift them down into the bottom of the result DWORD.

				fixed (PackedDWORDFields* p = &this) {
					return (p->m_rgPackedFields[dwStartBlock] & dwValueMask) >> (int) dwValueShift;
				}
			}

			// Hard case: the return value is split across two DWORDs (two DWORDs is the max as the new value
			// can be at most DWORD-sized itself). For simplicity we'll simply break this into two separate
			// non-spanning gets and stitch the result together from that. We can revisit this in the future
			// if the perf is a problem.
			DWORD dwInitialBits = kBitsPerDWORD - dwOffset % kBitsPerDWORD; // Number of bits to get in the first DWORD
			DWORD dwReturn;

			// Get the initial (low-order) bits from the first DWORD.
			dwReturn = BitVectorGet(dwOffset, dwInitialBits);

			// Get the remaining bits from the second DWORD. These bits will need to be shifted to the left
			// (past the bits we've already read) before being OR'd into the result.
			dwReturn |= BitVectorGet(dwOffset + dwInitialBits, dwLength - dwInitialBits) << (int) dwInitialBits;

			return dwReturn;
		}
	}
}