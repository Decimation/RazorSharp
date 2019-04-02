#region

using System;
using System.Collections.Generic;
using RazorCommon.Utilities;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.Memory
{
	/// <summary>
	///     A basic sig scanner.
	/// </summary>
	public class MemScanner
	{
		private byte[]        m_buffer;
		private Pointer<byte> m_lo;

		public MemScanner(Region region)
		{
			SelectRegion(region);
		}

		public MemScanner()
		{
			m_lo     = null;
			m_buffer = null;
		}

		private void EnsureSetup()
		{
			if (m_lo.IsNull || m_buffer == null) {
				throw new Exception("A memory region must be specified.");
			}
		}

		public void SelectRegion(Region region)
		{
			m_buffer = region.Memory;
			m_lo     = region.LowAddress;
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

		public Pointer<byte> FindPattern(string szPattern)
		{
			return FindPattern(StringUtil.ParseByteArray(szPattern));
		}

		public Pointer<byte> FindPattern(byte[] rgPattern)
		{
			EnsureSetup();

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