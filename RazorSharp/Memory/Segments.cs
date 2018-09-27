#region

using System;
using System.Linq;
using RazorCommon;
using RazorSharp.Native;
using RazorSharp.Native.Structures.Images;
using RazorSharp.Pointers;

#endregion

// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace RazorSharp.Memory
{

	/// <summary>
	///     Provides utilities for operating with module (DLL) data segments.
	///     todo: data segment sizes seem to be a few Ks off from VMMap
	/// </summary>
	public static class Segments
	{

		/// <summary>
		///     Gets the segment type (<see cref="SegmentType" />) in which <paramref name="addr" /> resides.
		/// </summary>
		/// <param name="addr">Address to find the <see cref="SegmentType" /> of</param>
		/// <param name="moduleName">DLL module name; <c>null</c> for the current module</param>
		/// <returns>
		///     The corresponding <see cref="SegmentType" /> if <paramref name="addr" /> is in the address space
		///     of the specified module <paramref name="moduleName" />
		/// </returns>
		/// <exception cref="Exception">
		///     If the address <paramref name="addr" /> is not found in the address space of
		///     module <paramref name="moduleName" />
		/// </exception>
		public static SegmentType GetSegment(Pointer<byte> addr, string moduleName = null)
		{
			ImageSectionInfo[] sections = DbgHelp.GetPESectionInfo(Kernel32.GetModuleHandle(moduleName));
			foreach (ImageSectionInfo s in sections) {
				if (Mem.IsAddressInRange(s.EndAddress, addr.Address, s.SectionAddress)) {
					return Parse(s.SectionName);
				}
			}

			throw new Exception($"Could not find corresponding segment for {Hex.ToHex(addr.Address)}");
		}

		/// <summary>
		///     Gets the <see cref="ImageSectionInfo" /> of segment <paramref name="segment" /> in module
		///     <paramref name="moduleName" />
		/// </summary>
		/// <param name="segment">Segment name</param>
		/// <param name="moduleName">DLL module name; <c>null</c> for the current module</param>
		/// <returns>
		///     The corresponding <see cref="ImageSectionInfo" /> for the specified segment name <paramref name="segment" />
		/// </returns>
		/// <exception cref="Exception">
		///     If the segment <paramref name="segment" /> could not be found in module
		///     <paramref name="moduleName" />
		/// </exception>
		public static ImageSectionInfo GetSegment(string segment, string moduleName = null)
		{
			ImageSectionInfo[] arr = DbgHelp.GetPESectionInfo(Kernel32.GetModuleHandle(moduleName));

			foreach (ImageSectionInfo t in arr) {
				if (t.SectionName == segment) {
					return t;
				}
			}

			throw new Exception(
				$"Could not find segment: \"{segment}\". Try prefixing \"{segment}\" with a period: (e.g. \".{segment}\")");
		}

		/// <summary>
		///     Gets all of the <see cref="ImageSectionInfo" />s in the module <paramref name="moduleName" />
		/// </summary>
		/// <param name="moduleName">DLL module name; <c>null</c> for the current module</param>
		/// <returns>All of the segments as an array of <see cref="ImageSectionInfo" /></returns>
		public static ImageSectionInfo[] GetSegments(string moduleName = null)
		{
			return DbgHelp.GetPESectionInfo(Kernel32.GetModuleHandle(moduleName));
		}

		public static void DumpSegments(string moduleName = null)
		{
			ImageSectionInfo[] segments = GetSegments(moduleName);
			foreach (ImageSectionInfo v in segments) {
				ConsoleTable table =
					new ConsoleTable("Number", "Name", "Size", "Address", "End Address", "Characteristics");
				table.AddRow(v.SectionNumber, v.SectionName,
					String.Format("{0} ({1} K)", v.SectionSize, v.SectionSize / Mem.BytesInKilobyte),
					Hex.ToHex(v.SectionAddress),
					Hex.ToHex(v.EndAddress), v.SectionHeader.Characteristics);
				Console.WriteLine(table.ToMarkDownString());
			}
		}

		private static SegmentType Parse(string name)
		{
			switch (name) {
				case ".rdata":
					return SegmentType.rdata;
				case ".idata":
					return SegmentType.idata;
				case ".data":
					return SegmentType.data;
				case ".pdata":
					return SegmentType.pdata;
				case ".bss":
					return SegmentType.bss;
				case ".rsrc":
					return SegmentType.rsrc;
				case ".reloc":
					return SegmentType.reloc;
				case TEXT_SEGMENT:
					return SegmentType.text;
				case ".didat":
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
			byte[]           segMem = Mem.ReadBytes(s.SectionAddress, 0, s.SectionSize);
			for (int i = 0; i < s.SectionSize; i += IntPtr.Size) {
				if (new ArraySegment<byte>(segMem, i, IntPtr.Size).SequenceEqual(mem)) {
					return s.SectionAddress + i;
				}
			}

			return IntPtr.Zero;
		}

		internal const string TEXT_SEGMENT = ".text";
	}

}