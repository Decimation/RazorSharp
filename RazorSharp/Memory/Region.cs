using System;
using System.Diagnostics;
using RazorSharp.Native;
using RazorSharp.Native.Structures;
using RazorSharp.Native.Structures.Images;
using RazorSharp.Pointers;

namespace RazorSharp.Memory
{
	/// <summary>
	/// Represents a region in memory.
	/// </summary>
	public class Region
	{
		public long Size {
			get { return Math.Abs((long) (HighAddress - LowAddress)); }
		}

		public byte[] Memory {
			get { return Kernel32.ReadCurrentProcessMemory(LowAddress, (int) Size); }
		}

		/// <summary>
		/// Base address
		/// </summary>
		public Pointer<byte> LowAddress { get; }

		public Pointer<byte> HighAddress { get; }

		#region Constructors

		public Region(Pointer<byte> lo, Pointer<byte> hi)
		{
			LowAddress  = lo;
			HighAddress = hi;
		}

		public Region(Pointer<byte> lo, long size)
		{
			LowAddress  = lo;
			HighAddress = (lo + size);
		}

		#endregion

		public static Region FromModule(ProcessModule module)
		{
			var lo   = module.BaseAddress;
			var size = module.ModuleMemorySize;

			return new Region(lo, size);
		}

		public static Region FromSegment(ImageSectionInfo img)
		{
			var lo = img.SectionAddress;
			var hi = img.EndAddress;

			// region.Size seems to be 1 less than img.SectionSize
			return new Region(lo, hi);
		}

		public static Region FromPage(MemoryBasicInformation memInfo)
		{
			var lo   = memInfo.BaseAddress;
			var size = (long) memInfo.RegionSize;

			return new Region(lo, size);
		}
	}
}