#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using SimpleSharp;
using SimpleSharp.Extensions;
using SimpleSharp.Strings;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Images;
using RazorSharp.Native.Win32;

// ReSharper disable InconsistentNaming

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
		private const string TEXT_SEGMENT = ".text";

		public static bool SegmentExists(string module, string segmentType)
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
			ImageSectionInfo[] sections = GetPESectionInfo(ProcessApi.GetModuleHandle(moduleName));
			foreach (var s in sections)
				if (Mem.IsAddressInRange(s.EndAddress.Address, addr.Address, s.SectionAddress.Address))
					return Parse(s.SectionName);

			throw new Exception($"Could not find corresponding segment for {Hex.ToHex(addr.Address)}");
		}

		public static ImageSectionInfo GetSegment(Pointer<byte> addr, string moduleName = null)
		{
			ImageSectionInfo[] sections = GetPESectionInfo(ProcessApi.GetModuleHandle(moduleName));
			foreach (var s in sections)
				if (Mem.IsAddressInRange(s.EndAddress.Address, addr.Address, s.SectionAddress.Address))
					return s;

			return default;
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
			ImageSectionInfo[] arr = GetPESectionInfo(ProcessApi.GetModuleHandle(moduleName));

			foreach (var t in arr)
				if (t.SectionName == segment)
					return t;

			throw new Exception($"Could not find segment: \"{segment}\". " +
			                    $"Try prefixing \"{segment}\" with a period: (e.g. \".{segment}\")");
		}

		/// <summary>
		///     Gets all of the <see cref="ImageSectionInfo" />s in the module <paramref name="moduleName" />
		/// </summary>
		/// <param name="moduleName">DLL module name; <c>null</c> for the current module</param>
		/// <returns>All of the segments as an array of <see cref="ImageSectionInfo" /></returns>
		public static ImageSectionInfo[] GetSegments(string moduleName = null)
		{
			return GetPESectionInfo(ProcessApi.GetModuleHandle(moduleName));
		}

		public static void DumpAllSegments()
		{
			foreach (ProcessModule v in Modules.CurrentModules) {
				DumpSegments(v.ModuleName);
			}
		}

		public static void DumpSegments(string moduleName = null)
		{
			ImageSectionInfo[] segments = GetSegments(moduleName);
			var table = new ConsoleTable("Number", "Name",
			                             "Size", "Address", "End Address", "Characteristics", "Module");
			string moduleNameTable = moduleName ?? Process.GetCurrentProcess().MainModule.ModuleName; // todo
			
			foreach (var v in segments) {
				object[] rowCpy = v.Row;
				rowCpy[rowCpy.Length - 1] = moduleNameTable;
				table.AddRow(rowCpy);
			}

			Console.WriteLine(table.ToString());
		}

		private static SegmentType Parse(string name)
		{
			name = name.ToUpper();
			if (name[0] == '.') {
				name = name.Substring(1);
			}

			// Optimization
			if (name == TEXT_SEGMENT) {
				return SegmentType.TEXT;
			}

			return (SegmentType) Enum.Parse(typeof(SegmentType), name);
		}

		internal static IntPtr ScanSegment(string segment, string module, byte[] mem)
		{
			var    s      = GetSegment(segment, module);
			byte[] segMem = Mem.ReadBytes(s.SectionAddress, 0, s.SectionSize);
			for (int i = 0; i < s.SectionSize; i += IntPtr.Size) {
				var rgSeg = new ArraySegment<byte>(segMem, i, IntPtr.Size);
				if (rgSeg.SequenceEqual(mem))
					return (s.SectionAddress + i).Address;
			}


			return IntPtr.Zero;
		}


		public static unsafe ImageSectionInfo[] GetPESectionInfo(IntPtr hModule)
		{
			// get the location of the module's IMAGE_NT_HEADERS structure
			ImageNtHeaders64* pNtHdr = DbgHelp.ImageNtHeader(hModule);

			// section table immediately follows the IMAGE_NT_HEADERS
			var pSectionHdr = (IntPtr) (pNtHdr + 1);
			var imageBase   = hModule;
			var arr         = new ImageSectionInfo[pNtHdr->FileHeader.NumberOfSections];
			int size        = Marshal.SizeOf<ImageSectionHeader>();

			for (int scn = 0; scn < pNtHdr->FileHeader.NumberOfSections; ++scn) {
				var struc = Marshal.PtrToStructure<ImageSectionHeader>(pSectionHdr);

				arr[scn] = new ImageSectionInfo(scn, struc.Name,
				                                (void*) (imageBase.ToInt64() + struc.VirtualAddress),
				                                (int) struc.VirtualSize, struc);

				pSectionHdr += size;
			}

			return arr;
		}
	}
}