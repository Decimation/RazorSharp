using System;
using System.Collections.Generic;
using RazorSharp.Memory.Pointers;
using SimpleSharp.Strings;

namespace RazorSharp.Memory
{
	/// <summary>
	///     A basic sig scanner.
	/// </summary>
	public class SigScanner
	{
		private byte[]        m_buffer;
		private Pointer<byte> m_lo;

		public SigScanner(Segment r)
		{
			SelectRegion(r);
		}

		public SigScanner() : this(null) { }

		private void EnsureSetup()
		{
			if (m_lo.IsNull || m_buffer == null) {
				throw new Exception("A memory region must be specified.");
			}
		}

		public void SelectRegion(Segment r)
		{
			m_buffer = r.BaseAddress.CopyBytes(r.Size);
			m_lo     = r.BaseAddress;
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

		public Pointer<byte> FindPattern(string pattern)
		{
			return FindPattern(Strings.ParseByteArray(pattern));
		}

		public Pointer<byte> FindPattern(byte[] pattern)
		{
			EnsureSetup();

			for (int i = 0; i < m_buffer.Length; i++) {
				if (m_buffer[i] != pattern[0])
					continue;


				if (PatternCheck(i, pattern)) {
					Pointer<byte> p = m_lo + i;
					return p;
				}
			}

			return Mem.Nullptr;
		}
	}
}