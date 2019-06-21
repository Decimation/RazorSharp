#region

using System;
using System.Diagnostics;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native;
using RazorSharp.Native.Images;
using RazorSharp.Native.Win32;
using RazorSharp.Native.Win32.Structures;

#endregion

namespace RazorSharp.Memory
{
	/// <summary>
	///     Represents a region in memory.
	/// </summary>
	public class Region
	{
		public long Size => Math.Abs((long) (HighAddress - LowAddress));

		public byte[] Memory => Kernel32.ReadCurrentProcessMemory(LowAddress, (int) Size);

		/// <summary>
		///     Base address
		/// </summary>
		public Pointer<byte> LowAddress { get; }

		public Pointer<byte> HighAddress { get; }

		public static Region FromModule(ProcessModule module)
		{
			var lo   = module.BaseAddress;
			int size = module.ModuleMemorySize;

			return new Region(lo, size);
		}

		public static Region FromSegment(ImageSectionInfo img)
		{
			Pointer<byte> lo = img.SectionAddress;
			Pointer<byte> hi = img.EndAddress;

			// region.Size seems to be 1 less than img.SectionSize
			return new Region(lo, hi);
		}

		public static Region FromPage(MemoryBasicInformation memInfo)
		{
			var  lo   = memInfo.BaseAddress;
			long size = (long) memInfo.RegionSize;

			return new Region(lo, size);
		}

		#region Constructors

		public Region(Pointer<byte> lo, Pointer<byte> hi)
		{
			LowAddress  = lo;
			HighAddress = hi;
		}

		public Region(Pointer<byte> lo, long size)
		{
			LowAddress  = lo;
			HighAddress = (IntPtr) (lo.ToInt64() + size);
		}

		#endregion
	}
}