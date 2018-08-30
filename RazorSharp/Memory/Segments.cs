#region

using System;
using System.Linq;
using RazorCommon;
using RazorInvoke;
using RazorInvoke.Libraries;

#endregion

// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace RazorSharp.Memory
{

	/// <summary>
	///     Provides utilities for operating with module (DLL) data segments.
	/// </summary>
	public static unsafe class Segments
	{


		public static SegmentType GetSegment(IntPtr addr, string moduleName = null)
		{
			ImageSectionInfo[] sections = DbgHelp.GetPESectionInfo(Kernel32.GetModuleHandle(moduleName));
			foreach (ImageSectionInfo s in sections) {
				if (RazorMath.Between(addr.ToInt64(), s.SectionAddress.ToInt64(), s.EndAddress.ToInt64(), true)) {
					return Parse(s.SectionName);
				}
			}

			throw new Exception($"Could not find corresponding segment for {Hex.ToHex(addr)}");
		}

		public static ImageSectionInfo GetSegment(string segment, string module)
		{
			ImageSectionInfo[] arr = DbgHelp.GetPESectionInfo(Kernel32.GetModuleHandle(module));

			foreach (ImageSectionInfo t in arr) {
				if (t.SectionName == segment) {
					return t;
				}
			}

			throw new Exception($"Could not find segment: {segment}");
		}


		private static SegmentType Parse(string name)
		{
			switch (name) {
				case ".rdata":
				case "rdata":
					return SegmentType.rdata;
				case ".idata":
				case "idata":
					return SegmentType.idata;
				case ".data":
				case "data":
					return SegmentType.data;
				case ".pdata":
				case "pdata":
					return SegmentType.pdata;
				case ".bss":
				case "bss":
					return SegmentType.bss;
				case ".rsrc":
				case "rsrc":
					return SegmentType.rsrc;
				case ".reloc":
				case "reloc":
					return SegmentType.reloc;
				case ".text":
				case "text":
					return SegmentType.text;
				case ".didat":
				case "didat":
					return SegmentType.didat;
				default:
					throw new Exception();
			}
		}

		//todo
		public enum SegmentType
		{
			/// <summary>
			///     <c>const</c> data; readonly of <see cref="data" />
			/// </summary>
			rdata,

			/// <summary>
			///     Import directory; designates the imported and exported functions
			/// </summary>
			idata,

			/// <summary>
			///     Initialized data
			/// </summary>
			data,

			/// <summary>
			///     Exception information
			/// </summary>
			pdata,

			/// <summary>
			///     Uninitialized data
			/// </summary>
			bss,

			/// <summary>
			///     Resource directory
			/// </summary>
			rsrc,

			/// <summary>
			///     Image relocations
			/// </summary>
			reloc,

			/// <summary>
			///     Executable code. Also known as the <c>code</c> segment.
			/// </summary>
			text,

			/// <summary>
			///     Delay import section
			/// </summary>
			didat,
		}


		internal static IntPtr ScanSegment(string segment, string module, byte[] mem)
		{
			ImageSectionInfo s      = GetSegment(segment, module);
			byte[]           segMem = Memory.ReadBytes(s.SectionAddress, 0, s.SectionSize);
			for (int i = 0; i < s.SectionSize; i += IntPtr.Size) {
				if (new ArraySegment<byte>(segMem, i, IntPtr.Size).SequenceEqual(
					mem)) {
					return s.SectionAddress + i;
				}
			}

			return IntPtr.Zero;
		}
	}

}