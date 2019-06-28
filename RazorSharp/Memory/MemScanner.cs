#region

using System;
using System.Collections.Generic;
using RazorSharp.Memory.Pointers;
using SimpleSharp.Strings;

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

			for (int i = 0; i < m_buffer.Length; i++) {
				if (m_buffer[i] != rgPattern[0])
					continue;


				if (PatternCheck(i, rgPattern)) {
					Pointer<byte> p = m_lo + i;
					return p;
				}
			}

			return IntPtr.Zero;
		}
	}
}