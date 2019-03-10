#region

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Utilities;
using RazorSharp.Native;
using RazorSharp.Pointers;
using Serilog.Context;

#endregion

// ReSharper disable InconsistentNaming

#endregion

namespace RazorSharp.Memory
{
	// todo: make a memory scanner (sigscanner for memory rather than sigs)

	/// <summary>
	///     Edited by Decimation (not entirely original)
	/// </summary>
	public class SigScanner
	{
		private const string                     CONTEXT_PROP_TAG = "SigScanner";
		private       string                     m_moduleName;
		private       IntPtr                     m_hProcess           { get; }
		private       byte[]                     m_rgModuleBuffer     { get; set; }
		private       IntPtr                     m_lpModuleBase       { get; set; }
		private       Dictionary<string, string> m_dictStringPatterns { get; }

		public IntPtr BaseAddress => m_lpModuleBase;

		public void AddPattern(string szPatternName, string szPattern)
		{
			m_dictStringPatterns.Add(szPatternName, szPattern);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool PatternCheck(long nOffset, IReadOnlyList<byte> arrPattern, int byteTolerance = 0)
		{
			int failedMatches = 0;
			// ReSharper disable once LoopCanBeConvertedToQuery
			for (int i = 0; i < arrPattern.Count; i++) {
				if (arrPattern[i] == 0x0) continue;

				if (arrPattern[i] != m_rgModuleBuffer[nOffset + i]) {
					failedMatches++;
					if (failedMatches > byteTolerance)
						return false;
				}
			}


			using (LogContext.PushProperty(Global.CONTEXT_PROP, CONTEXT_PROP_TAG)) {
				Global.Log.Verbose("Failed bytes: {Count} (tolerance: {N})", failedMatches, byteTolerance);
			}

			if (failedMatches <= byteTolerance) return true;
			return true;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ModuleCheck()
		{
			if (m_rgModuleBuffer == null || m_lpModuleBase == IntPtr.Zero)
				throw new Exception("Selected module is null");
		}

		/// <summary>
		///     Use <c>0x00</c> for <c>"?"</c>
		/// </summary>
		/// <param name="rgPattern"></param>
		/// <param name="ofsGuess"></param>
		/// <returns><see cref="IntPtr.Zero" /> if the pattern could not be found</returns>
		public IntPtr FindPattern(byte[] rgPattern, long ofsGuess = 0, int byteTolerance = 0)
		{
			ModuleCheck();
			bool ofsGuessFailed = false;

			if (ofsGuess != 0) {
				if (PatternCheck(ofsGuess, rgPattern, byteTolerance))
					return PointerUtils.Add(m_lpModuleBase, ofsGuess).Address;

//					throw new Exception($"Offset guess of {ofsGuess} failed");

//					Logger.Log("Offset guess of {0} failed", Hex.ToHex(ofsGuess));
				ofsGuessFailed = true;
			}

			for (int nModuleIndex = 0; nModuleIndex < m_rgModuleBuffer.Length; nModuleIndex++) {
				if (m_rgModuleBuffer[nModuleIndex] != rgPattern[0])
					continue;


				if (PatternCheck(nModuleIndex, rgPattern)) {
					

					var p = m_lpModuleBase + nModuleIndex;
					//Console.WriteLine("Matched opcodes: {0} (actual offset: {1:X}) (theoretical offset: {2:X}) (addr: {3:X})",
					//                  Collections.CreateString(rgPattern, ToStringOptions.Hex), nModuleIndex, 
					//                  ofsGuess,p.ToInt64());
					return p;
				}
			}


			return IntPtr.Zero;
		}

		public IntPtr FindPattern(string szPattern, long ofsGuess = 0, int byteTolerance = 0)
		{
			ModuleCheck();


			//Debug.Assert(szPattern!=null);
			byte[] arrPattern = StringUtil.ParseByteArray(szPattern);

			Debug.Assert(arrPattern != null);
			var p = FindPattern(arrPattern, ofsGuess, byteTolerance);

			return p;
		}

		// todo: add byteTolerance params

		public static IntPtr QuickScan(string module, string szPattern, long ofsGuess = 0)
		{
			return QuickScan(module, StringUtil.ParseByteArray(szPattern), ofsGuess);
		}

		public static IntPtr QuickScan(string module, byte[] rgPattern, long ofsGuess = 0)
		{
			var ss = new SigScanner();
			ss.SelectModule(module);
			return ss.FindPattern(rgPattern, ofsGuess);
		}

		public static TDelegate QuickScanDelegate<TDelegate>(string module, byte[] rgPattern, long ofsGuess = 0)
			where TDelegate : Delegate
		{
			var ss = new SigScanner();
			ss.SelectModule(module);
			return ss.GetDelegate<TDelegate>(rgPattern, ofsGuess);
		}

		public static TDelegate QuickScanDelegate<TDelegate>(string module, string szPattern, long ofsGuess = 0)
			where TDelegate : Delegate
		{
			return QuickScanDelegate<TDelegate>(module, StringUtil.ParseByteArray(szPattern), ofsGuess);
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

		public TDelegate GetDelegate<TDelegate>(byte[] rgPattern, long ofsGuess = 0) where TDelegate : Delegate
		{
			var addr = FindPattern(rgPattern, ofsGuess);
			if (addr == IntPtr.Zero)
				throw new Exception($"Could not find function with opcodes {Collections.CreateString(rgPattern, ToStringOptions.Hex)}");

			return Marshal.GetDelegateForFunctionPointer<TDelegate>(addr);
		}

		public TDelegate GetDelegate<TDelegate>(string szPattern, long ofsGuess = 0) where TDelegate : Delegate
		{
			return GetDelegate<TDelegate>(StringUtil.ParseByteArray(szPattern), ofsGuess);
		}


		public Dictionary<string, IntPtr> FindPatterns(out long lTime)
		{
			ModuleCheck();

			var stopwatch = Stopwatch.StartNew();

			var arrBytePatterns = new byte[m_dictStringPatterns.Count][];
			var arrResult       = new IntPtr[m_dictStringPatterns.Count];

			// PARSE PATTERNS
			for (int nIndex = 0; nIndex < m_dictStringPatterns.Count; nIndex++)
				arrBytePatterns[nIndex] = StringUtil.ParseByteArray(m_dictStringPatterns.ElementAt(nIndex).Value);

			// SCAN FOR PATTERNS
			for (int nModuleIndex = 0; nModuleIndex < m_rgModuleBuffer.Length; nModuleIndex++)
			for (int nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++) {
				if (arrResult[nPatternIndex] != IntPtr.Zero) continue;

				if (PatternCheck(nModuleIndex, arrBytePatterns[nPatternIndex]))
					arrResult[nPatternIndex] = m_lpModuleBase + nModuleIndex;
			}

			var dictResultFormatted = new Dictionary<string, IntPtr>();

			// FORMAT PATTERNS
			for (int nPatternIndex = 0; nPatternIndex < arrBytePatterns.Length; nPatternIndex++)
				dictResultFormatted[m_dictStringPatterns.ElementAt(nPatternIndex).Key] = arrResult[nPatternIndex];

			lTime = stopwatch.ElapsedMilliseconds;
			return dictResultFormatted;
		}

		#region Constructors

		public SigScanner() : this(Process.GetCurrentProcess()) { }

		public SigScanner(Process proc) : this(proc.Handle) { }

		public SigScanner(IntPtr hProc)
		{
			m_hProcess           = hProc;
			m_dictStringPatterns = new Dictionary<string, string>();
		}

		#endregion

		#region Module

		public void SelectModuleBySegment(string moduleName, string segmentName)
		{
			// Module already selected
			if (moduleName == m_moduleName) return;

			var segment = Segments.GetSegment(segmentName, moduleName);
			Setup(segment.SectionSize, segment.SectionAddress.Address, moduleName);
		}

		public void SelectModuleByPage(ProcessModule module, Pointer<byte> pageBase)
		{
			SelectModuleAsPage(module.ModuleName, pageBase);
		}

		private void Setup(int bufSize, Pointer<byte> baseAddr, string moduleName)
		{
			m_rgModuleBuffer = new byte[bufSize];
			m_lpModuleBase   = baseAddr.Address;
			m_dictStringPatterns.Clear();
			m_moduleName = moduleName;
			Marshal.Copy(m_lpModuleBase, m_rgModuleBuffer, 0, bufSize);
		}

		public void SelectModuleAsPage(string moduleName, Pointer<byte> pageBase)
		{
			// Module already selected
			if (moduleName == m_moduleName) return;

			var page = Kernel32.VirtualQuery(pageBase.Address);

			Setup(page.RegionSize.ToInt32(), pageBase, moduleName);
		}

		public bool SelectModule(ProcessModule targetModule)
		{
			// Module already selected
			if (targetModule.ModuleName == m_moduleName) return true;

			m_lpModuleBase   = targetModule.BaseAddress;
			m_rgModuleBuffer = new byte[targetModule.ModuleMemorySize];


			m_dictStringPatterns.Clear();
			ulong lpNumberOfBytesRead = 0;

			m_moduleName = targetModule.ModuleName;
			return Kernel32.ReadProcessMemory(m_hProcess, m_lpModuleBase, m_rgModuleBuffer,
			                                  (uint) targetModule.ModuleMemorySize, ref lpNumberOfBytesRead);
		}

		public void SelectModule(string name)
		{
			// Module already selected
			if (name == m_moduleName) return;

			SelectModule(Modules.GetModule(name));
		}

		public void SelectMainModule()
		{
			SelectModule(Process.GetCurrentProcess().MainModule);
		}

		#endregion
	}
}