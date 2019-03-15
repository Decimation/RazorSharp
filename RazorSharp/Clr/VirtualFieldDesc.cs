using RazorSharp.Clr.Structures;
using RazorSharp.Pointers;

namespace RazorSharp.Clr
{
	public class VirtualFieldDesc
	{
		/*
		 * [FieldOffset(0)]
		 * private readonly MethodTable* m_pMTOfEnclosingClass;
		 * 
		 * // unsigned m_mb                  	: 24;
		 * // unsigned m_isStatic            	: 1;
		 * // unsigned m_isThreadLocal       	: 1;
		 * // unsigned m_isRVA               	: 1;
		 * // unsigned m_prot                	: 3;
		 * // unsigned m_requiresFullMbValue 	: 1;
		 * [FieldOffset(PTR_SIZE)]
		 * private readonly uint m_dword1;
		 * 
		 * // unsigned m_dwOffset         		: 27;
		 * // unsigned m_type             		: 5;
		 * [FieldOffset(PTR_SIZE + sizeof(uint))]
		 * private uint m_dword2;
		 */

		private readonly Pointer<MethodTable> m_pMTOfEnclosingClass;
		private readonly uint m_dword1;
		private readonly uint m_dword2;

		internal VirtualFieldDesc(Pointer<MethodTable> pMtOfEnclosingClass, uint dword1, uint dword2)
		{
			m_pMTOfEnclosingClass = pMtOfEnclosingClass;
			m_dword1 = dword1;
			m_dword2 = dword2;
		}
	}
}