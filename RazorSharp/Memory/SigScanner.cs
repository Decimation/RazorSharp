#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using RazorInvoke.Libraries;

#endregion

namespace RazorSharp.Memory
{

	/// <summary>
	///     Edited by Decimation (not original)
	/// </summary>
	public class SigScanner
	{
		private IntPtr                     m_hProcess           { get; set; }
		private byte[]                     m_rgModuleBuffer     { get; set; }
		private IntPtr                     m_lpModuleBase       { get; set; }
		private Dictionary<string, string> m_dictStringPatterns { get; }
		private string                     m_moduleName;

		public IntPtr BaseAddress => m_lpModuleBase;

		public SigScanner() : this(Process.GetCurrentProcess())
		{

		}

		public SigScanner(Process proc) : this(proc.Handle) { }

		public SigScanner(IntPtr hProc)
		{
			m_hProcess           = hProc;
			m_dictStringPatterns = new Dictionary<string, string>();
		}


		public bool SelectModule(ProcessModule targetModule)
		{
			// Module already selected
			if (targetModule.ModuleName == m_moduleName) {
				return true;
			}

			m_lpModuleBase   = targetModule.BaseAddress;
			m_rgModuleBuffer = new byte[targetModule.ModuleMemorySize];


			m_dictStringPatterns.Clear();
			ulong lpNumberOfBytesRead = 0;

			m_moduleName = targetModule.ModuleName;
			return Kernel32.ReadProcessMemory(m_hProcess, m_lpModuleBase, m_rgModuleBuffer,
				(uint) targetModule.ModuleMemorySize, ref lpNumberOfBytesRead);
		}

		public void AddPattern(string szPatternName, string szPattern)
		{
			m_dictStringPatterns.Add(szPatternName, szPattern);
		}

		private bool PatternCheck(int nOffset, byte[] arrPattern)
		{
			// ReSharper disable once LoopCanBeConvertedToQuery
			for (int i = 0; i < arrPattern.Length; i++) {
				if (arrPattern[i] == 0x0) {
					continue;
				}

				if (arrPattern[i] != m_rgModuleBuffer[nOffset + i]) {
					return false;
				}
			}

			return true;
		}

		private void ModuleCheck()
		{
			if (m_rgModuleBuffer == null || m_lpModuleBase == IntPtr.Zero) {
				throw new Exception("Selected module is null");
			}
		}

		public IntPtr FindPattern(byte[] rgPattern)
		{
			ModuleCheck();

			for (int nModuleIndex = 0; nModuleIndex < m_rgModuleBuffer.Length; nModuleIndex++) {
				if (m_rgModuleBuffer[nModuleIndex] != rgPattern[0]) {
					continue;
				}

				if (PatternCheck(nModuleIndex, rgPattern)) {
					return m_lpModuleBase + nModuleIndex;
				}
			}


			return IntPtr.Zero;
		}

		public IntPtr FindPattern(string szPattern)
		{
			ModuleCheck();


			byte[] arrPattern = ParsePatternString(szPattern);

			return FindPattern(arrPattern);
		}

		/*public IntPtr FindPattern(string szPattern, out long lTime)
		{
			ModuleCheck();

			Stopwatch stopwatch = Stopwatch.StartNew();

			byte[] arrPattern = ParsePatternString(szPattern);

			for (int nModuleIndex = 0; nModuleIndex < m_rgModuleBuffer.Length; nModuleIndex++) {
				if (m_rgModuleBuffer[nModuleIndex] != arrPattern[0]) {
					continue;
				}

				if (PatternCheck(nModuleIndex, arrPattern)) {
					lTime = stopwatch.ElapsedMilliseconds;
					return m_lpModuleBase + nModuleIndex;
				}
			}

			lTime = stopwatch.ElapsedMilliseconds;
			return IntPtr.Zero;
		}*/

		public TDelegate GetDelegate<TDelegate>(string opcodes) where TDelegate : Delegate
		{
			IntPtr addr = FindPattern(opcodes);
			if (addr == IntPtr.Zero) {
				throw new Exception($"Could not find function with opcodes {opcodes}");
			}

			return Marshal.GetDelegateForFunctionPointer<TDelegate>(addr);
		}

		public void SelectModule(string name)
		{
			// Module already selected
			if (name == m_moduleName) {
				return;
			}

			foreach (ProcessModule m in Process.GetCurrentProcess().Modules) {
				if (m.ModuleName == name) {
					SelectModule(m);
					return;
				}
			}
		}

		public Dictionary<string, IntPtr> FindPatterns(out long lTime)
		{
			if (m_rgModuleBuffer == null || m_lpModuleBase == IntPtr.Zero) {
				throw new Exception("Selected module is null");
			}

			Stopwatch stopwatch = Stopwatch.StartNew();

			byte[][] arrBytePatterns = new byte[m_dictStringPatterns.Count][];
			IntPtr[] arrResult       = new IntPtr[m_dictStringPatterns.Count];

			// PARSE PATTERNS
			for (int nIndex = 0; nIndex < m_dictStringPatterns.Count; nIndex++)
				arrBytePatterns[nIndex] = ParsePatternString(m_dictStringPatterns.ElementAt(nIndex).Value);

			// SCAN FOR PATTERNS
			for (int nModuleIndex = 0; nModuleIndex < m_rgModuleBuffer.Length; nModuleIndex++)
			for (int nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++) {
				if (arrResult[nPatternIndex] != IntPtr.Zero) {
					continue;
				}

				if (this.PatternCheck(nModuleIndex, arrBytePatterns[nPatternIndex])) {
					arrResult[nPatternIndex] = m_lpModuleBase + nModuleIndex;
				}
			}

			Dictionary<string, IntPtr> dictResultFormatted = new Dictionary<string, IntPtr>();

			// FORMAT PATTERNS
			for (int nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++)
				dictResultFormatted[m_dictStringPatterns.ElementAt(nPatternIndex).Key] = arrResult[nPatternIndex];

			lTime = stopwatch.ElapsedMilliseconds;
			return dictResultFormatted;
		}

		private static byte[] ParsePatternString(string szPattern)
		{
			List<byte> patternbytes = new List<byte>();

			foreach (string szByte in szPattern.Split(' '))
				patternbytes.Add(szByte == "?" ? (byte) 0x0 : Convert.ToByte(szByte, 16));

			return patternbytes.ToArray();
		}
	}

}