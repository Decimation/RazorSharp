#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorInvoke;
using RazorInvoke.Libraries;
using RazorSharp.Pointers;
using RazorSharp.Runtime.CLRTypes;

#endregion

namespace RazorSharp.Experimental
{

	// WIP
	internal unsafe class SmartScanner : IDisposable
	{
		private readonly Process m_proc;

		private readonly IntPtr m_handle;

		private readonly IntPtr m_maxAddress;

		private readonly Dictionary<IntPtr, byte[]> m_regions;

		public SmartScanner(Process proc)
		{
			m_proc    = proc;
			m_regions = new Dictionary<IntPtr, byte[]>();

			SystemInfo info = Kernel32.GetSystemInfo();
			m_maxAddress = info.MaximumApplicationAddress;

			m_handle = Kernel32.OpenProcess((Enumerations.ProcessAccessFlags) 1080, false, m_proc.Id);
		}

		public void PrintRegions()
		{
			foreach (var v in GetRegions()) {
				Console.WriteLine("{0} | {1} | {2}", v.Type, Hex.ToHex(v.BaseAddress), v.RegionSize);
			}
		}


		public Dictionary<IntPtr, byte[]> Find(byte[] memory)
		{
			int cnt  = 0;
			var list = new Dictionary<IntPtr, byte[]>();
			foreach (var v in m_regions) {
				// Read aligned
				// Scan for heap memory
				for (int i = 0; i < v.Value.Length; i += memory.Length) {
					var segment = new ArraySegment<byte>(v.Value, i, memory.Length);

					//Console.Write("\rReading region [{0}, {1} bytes] [{2}] [{3}]", Hex.ToHex(v.Key), v.Value.Length,Collections.ToString(segment.ToArray()),cnt);

					if (segment.SequenceEqual(memory)) {
						list.Add(v.Key + i, segment.ToArray());
						cnt++;
					}
				}
			}

			return list;
		}

		public List<T> ReadAll<T>() where T : class
		{
			int validC = 0;

			// Stack size
			var typeSize = Unsafe.SizeOf<T>();
			var list     = new List<T>();
			foreach (var v in m_regions) {
				Console.WriteLine("Reading region [{0}, {1} bytes]", Hex.ToHex(v.Key), v.Value.Length);

				// For now we'll only read as if it's the stack
				// - We won't scan for heap memory, only stack pointers
				// - We'll also read aligned          \/
				for (int i = 0; i < v.Value.Length; i += typeSize) {
					byte[] mem = new byte[typeSize];
					for (int j = i; j < i + typeSize; j++) {
						mem[j - i] = v.Value[j];
					}

					var valid = IsMethodTable<T>(mem);
					if (valid) {
						validC++;
					}
				}
			}

			Console.WriteLine(validC);

			return list;
		}

		[HandleProcessCorruptedStateExceptions]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsMethodTable<T>(byte[] mem) where T : class
		{
			Debug.Assert(mem.Length == Unsafe.SizeOf<T>());
			fixed (byte* b = mem) {
				return *(MethodTable**) b == Runtime.Runtime.MethodTableOf<T>();
			}
		}

		private List<MemoryBasicInformation> GetRegions()
		{
			var                    ls      = new List<MemoryBasicInformation>();
			MemoryBasicInformation memInfo = new MemoryBasicInformation();
			IntPtr                 current = IntPtr.Zero;

			while (current.ToInt64() < m_maxAddress.ToInt64() &&
			       Kernel32.VirtualQueryEx(m_handle, current, out memInfo, (uint) Marshal.SizeOf(memInfo)) != 0) {
				if ((int) memInfo.State == 4096 && (int) memInfo.Protect == 4 && (uint) memInfo.RegionSize != 0) {
					ls.Add(memInfo);
				}

				//ls.Add(memInfo);


				current = PointerUtils.Add(memInfo.BaseAddress, memInfo.RegionSize);
			}

			return ls;
		}

		public void ReadRegions()
		{
			MemoryBasicInformation memInfo = new MemoryBasicInformation();
			IntPtr                 current = IntPtr.Zero;

			while (current.ToInt64() < m_maxAddress.ToInt64() &&
			       Kernel32.VirtualQueryEx(m_handle, current, out memInfo, (uint) Marshal.SizeOf(memInfo)) != 0) {
				if ((int) memInfo.State == 4096 && (int) memInfo.Protect == 4 && (uint) memInfo.RegionSize != 0) {
					byte[] regionData = new byte[(int) memInfo.RegionSize];
					ulong  bytesRead  = 0;


					if (!Kernel32.ReadProcessMemory(m_handle, memInfo.BaseAddress, regionData,
						(ulong) memInfo.RegionSize, ref bytesRead))
						throw new Exception("Failed to read process memory at " + memInfo.BaseAddress +
						                    ". Error code: " + Marshal.GetLastWin32Error());

					m_regions.Add(memInfo.BaseAddress, regionData);
				}

				current = PointerUtils.Add(memInfo.BaseAddress, memInfo.RegionSize);
			}

			Console.WriteLine("Regions: {0}", m_regions.Count);
		}

		public void Dispose()
		{
			m_proc?.Dispose();
			Kernel32.CloseHandle(m_handle);
		}
	}

}