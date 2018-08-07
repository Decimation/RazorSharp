using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using RazorInvoke;

namespace RazorSharp.Memory
{

	/// <summary>
	/// Edited by Decimation (not original)
	/// </summary>
	public unsafe class SigScanner
	{
		private IntPtr g_hProcess        { get; set; }
		private byte[] g_arrModuleBuffer { get; set; }
		private IntPtr g_lpModuleBase    { get; set; }

		private Dictionary<string, string> g_dictStringPatterns { get; }

		public SigScanner(Process proc) : this(proc.Handle) { }

		public SigScanner(IntPtr hProc)
		{
			g_hProcess           = hProc;
			g_dictStringPatterns = new Dictionary<string, string>();
		}

		public bool SelectModule(ProcessModule targetModule)
		{
			g_lpModuleBase    = targetModule.BaseAddress;
			g_arrModuleBuffer = new byte[targetModule.ModuleMemorySize];

			g_dictStringPatterns.Clear();
			ulong lpNumberOfBytesRead = 0;

			return Kernel32.ReadProcessMemory(g_hProcess, g_lpModuleBase, g_arrModuleBuffer,
				(uint) targetModule.ModuleMemorySize, ref lpNumberOfBytesRead);
		}

		public void AddPattern(string szPatternName, string szPattern)
		{
			g_dictStringPatterns.Add(szPatternName, szPattern);
		}

		private bool PatternCheck(int nOffset, byte[] arrPattern)
		{
			for (int i = 0; i < arrPattern.Length; i++) {
				if (arrPattern[i] == 0x0)
					continue;

				if (arrPattern[i] != this.g_arrModuleBuffer[nOffset + i])
					return false;
			}

			return true;
		}

		public IntPtr FindPattern(string szPattern, out long lTime)
		{
			if (g_arrModuleBuffer == null || g_lpModuleBase == IntPtr.Zero)
				throw new Exception("Selected module is null");

			Stopwatch stopwatch = Stopwatch.StartNew();

			byte[] arrPattern = ParsePatternString(szPattern);

			for (int nModuleIndex = 0; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++) {
				if (this.g_arrModuleBuffer[nModuleIndex] != arrPattern[0])
					continue;

				if (PatternCheck(nModuleIndex, arrPattern)) {
					lTime = stopwatch.ElapsedMilliseconds;
					return g_lpModuleBase + nModuleIndex;
				}
			}

			lTime = stopwatch.ElapsedMilliseconds;
			return IntPtr.Zero;
		}

		public TDelegate GetDelegate<TDelegate>(string opcodes) where TDelegate : Delegate
		{
			IntPtr addr = FindPattern(opcodes, out long time);
			Debug.Assert(addr != IntPtr.Zero);
			return Marshal.GetDelegateForFunctionPointer<TDelegate>(addr);
		}

		public void SelectModule(string name)
		{
			foreach (var m in Process.GetCurrentProcess().Modules) {
				if (((ProcessModule) m).ModuleName == name) {
					SelectModule((ProcessModule) m);
					return;
				}
			}
		}

		public Dictionary<string, IntPtr> FindPatterns(out long lTime)
		{
			if (g_arrModuleBuffer == null || g_lpModuleBase == IntPtr.Zero)
				throw new Exception("Selected module is null");

			Stopwatch stopwatch = Stopwatch.StartNew();

			byte[][] arrBytePatterns = new byte[g_dictStringPatterns.Count][];
			IntPtr[] arrResult       = new IntPtr[g_dictStringPatterns.Count];

			// PARSE PATTERNS
			for (int nIndex = 0; nIndex < g_dictStringPatterns.Count; nIndex++)
				arrBytePatterns[nIndex] = ParsePatternString(g_dictStringPatterns.ElementAt(nIndex).Value);

			// SCAN FOR PATTERNS
			for (int nModuleIndex = 0; nModuleIndex < g_arrModuleBuffer.Length; nModuleIndex++) {
				for (int nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++) {
					if (arrResult[nPatternIndex] != IntPtr.Zero)
						continue;

					if (this.PatternCheck(nModuleIndex, arrBytePatterns[nPatternIndex]))
						arrResult[nPatternIndex] = g_lpModuleBase + nModuleIndex;
				}
			}

			Dictionary<string, IntPtr> dictResultFormatted = new Dictionary<string, IntPtr>();

			// FORMAT PATTERNS
			for (int nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++)
				dictResultFormatted[g_dictStringPatterns.ElementAt(nPatternIndex).Key] = arrResult[nPatternIndex];

			lTime = stopwatch.ElapsedMilliseconds;
			return dictResultFormatted;
		}

		private static byte[] ParsePatternString(string szPattern)
		{
			List<byte> patternbytes = new List<byte>();

			foreach (var szByte in szPattern.Split(' '))
				patternbytes.Add(szByte == "?" ? (byte) 0x0 : Convert.ToByte(szByte, 16));

			return patternbytes.ToArray();
		}
	}

}