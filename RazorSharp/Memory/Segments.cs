#region

using System;
using System.Diagnostics;
using System.Linq;
using RazorCommon;
using RazorCommon.Extensions;
using RazorCommon.Strings;
using RazorSharp.Native;
using RazorSharp.Native.Structures.Images;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

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
		static Segments()
		{
			Conditions.Requires64Bit();
		}
		
		internal const string TEXT_SEGMENT = ".text";

		public static bool SegmentExists(string module,string segmentType)
		{
			return GetSegments(module).ContainsBy(x => x.SectionName, segmentType);
		}

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
		public static SegmentType GetSegmentType(Pointer<byte> addr, string moduleName = null)
		{
			ImageSectionInfo[] sections = DbgHelp.GetPESectionInfo(Kernel32.GetModuleHandle(moduleName));
			foreach (var s in sections)
				if (Mem.IsAddressInRange(s.EndAddress.Address, addr.Address, s.SectionAddress.Address))
					return Parse(s.SectionName);

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

			foreach (var t in arr)
				if (t.SectionName == segment)
					return t;

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

		public static void DumpAllSegments()
		{
			foreach (ProcessModule v in Process.GetCurrentProcess().Modules) {
				DumpSegments(v.ModuleName);
			}
		}

		public static void DumpSegments(string moduleName = null)
		{
			ImageSectionInfo[] segments = GetSegments(moduleName);
			var table = new ConsoleTable("Number", "Name", "Size", "Address", "End Address", "Characteristics",
			                             "Module");
			string moduleNameTable = moduleName ?? Process.GetCurrentProcess().MainModule.ModuleName; // todo
			foreach (var v in segments) {
				object[] rowCpy = v.Row;
				rowCpy[rowCpy.Length - 1] = moduleNameTable;
				table.AddRow(rowCpy);
			}

			Console.WriteLine(table.ToMarkDownString());
		}

		private static SegmentType Parse(string name)
		{
			switch (name.ToLower()) {
				case ".rdata":
					return SegmentType.RDATA;
				case ".idata":
					return SegmentType.IDATA;
				case ".data":
					return SegmentType.DATA;
				case ".pdata":
					return SegmentType.PDATA;
				case ".bss":
					return SegmentType.BSS;
				case ".rsrc":
					return SegmentType.RSRC;
				case ".reloc":
					return SegmentType.RELOC;
				case TEXT_SEGMENT:
					return SegmentType.TEXT;
				case ".didat":
					return SegmentType.DIDAT;
				default:
					throw new Exception();
			}
		}

		internal static IntPtr ScanSegment(string segment, string module, byte[] mem)
		{
			var    s      = GetSegment(segment, module);
			byte[] segMem = Mem.ReadBytes(s.SectionAddress, 0, s.SectionSize);
			for (int i = 0; i < s.SectionSize; i += IntPtr.Size)
				if (new ArraySegment<byte>(segMem, i, IntPtr.Size).SequenceEqual(mem))
					return (s.SectionAddress + i).Address;

			return IntPtr.Zero;
		}
	}
}