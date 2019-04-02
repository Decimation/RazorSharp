#region

using System;
using System.Collections.Generic;
using RazorCommon.Utilities;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.Memory
{
	public class MemScanner
	{
		private readonly Pointer<byte> m_lo;
		private readonly byte[]        m_buffer;

		public MemScanner(Region region)
		{
			m_buffer = region.Memory;
			m_lo   = region.LowAddress;
		}

		private bool PatternCheck(int nOffset, IReadOnlyList<byte> arrPattern)
		{
			// ReSharper disable once LoopCanBeConvertedToQuery
			for (int i = 0; i < arrPattern.Count; i++) {
				if (arrPattern[i] == 0x0)
					continue;

				if (arrPattern[i] != m_buffer[nOffset + i])
					return false;
			}

			return true;
		}

		public Pointer<byte> FindPattern(string szPattern) => FindPattern(StringUtil.ParseByteArray(szPattern));
		
		public Pointer<byte> FindPattern(byte[] rgPattern)
		{
			for (int nModuleIndex = 0; nModuleIndex < m_buffer.Length; nModuleIndex++) {
				if (m_buffer[nModuleIndex] != rgPattern[0])
					continue;


				if (PatternCheck(nModuleIndex, rgPattern)) {
					Pointer<byte> p = m_lo + nModuleIndex;
					return p;
				}
			}

			return IntPtr.Zero;
		}
	}
}