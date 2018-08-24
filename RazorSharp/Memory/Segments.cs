#region

using System;
using System.Linq;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorInvoke.Libraries;
using RazorSharp.Pointers;

#endregion

// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace RazorSharp.Memory
{

	public static unsafe class Segments
	{

		private const string RAZOR_NATIVE_DLL =
			@"C:\Users\Viper\CLionProjects\RazorNative\cmake-build-debug\RazorNative.dll";

		private const int SEGMENT_ARR_DEFAULT_SIZE = 10;
		private const int IMAGE_SIZEOF_SHORT_NAME  = 8;


		[DllImport(RAZOR_NATIVE_DLL, EntryPoint = "PrintPESectionInfo")]
		private static extern void PrintPESectionInfo(IntPtr hModule);

		[DllImport(RAZOR_NATIVE_DLL)]
		public static extern void GetPESectionInfo(IntPtr hModule, [Out] [MarshalAs(UnmanagedType.LPArray)]
			ImageSectionInfo[] arr);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public unsafe struct ImageSectionInfo
		{

			#region Fields

			private readonly int m_sectionNumber;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = IMAGE_SIZEOF_SHORT_NAME)]
			private readonly string m_sectionName;

			private readonly void* m_sectionAddress;
			private readonly int   m_sectionSize;

			#endregion

			#region Accessors

			public int    SectionNumber  => m_sectionNumber;
			public IntPtr SectionAddress => (IntPtr) m_sectionAddress;
			public string SectionName    => m_sectionName;
			public int    SectionSize    => m_sectionSize;

			public IntPtr EndAddress =>
				PointerUtils.Add(m_sectionAddress, (byte*) m_sectionSize - 1);

			#endregion


			public override string ToString()
			{
				ConsoleTable table = new ConsoleTable("Section #", "Name", "Address", "End Address", "Size");
				table.AddRow(m_sectionNumber, m_sectionName, Hex.ToHex(m_sectionAddress), Hex.ToHex(EndAddress),
					m_sectionSize);
				return table.ToMarkDownString();
			}
		}

		public static SegmentType GetSegment(IntPtr addr, string moduleName = null)
		{
			ImageSectionInfo[] sections = new ImageSectionInfo[SEGMENT_ARR_DEFAULT_SIZE];
			GetPESectionInfo(Kernel32.GetModuleHandle(moduleName), sections);
			foreach (ImageSectionInfo s in sections) {
				if (RazorMath.Between(addr.ToInt64(), s.SectionAddress.ToInt64(), s.EndAddress.ToInt64(), true)) {
					return Parse(s.SectionName);
				}
			}

			throw new Exception($"Could not find corresponding segment for {Hex.ToHex(addr)}");
		}

		public static ImageSectionInfo GetSegment(string segment, string module)
		{
			ImageSectionInfo[] arr = new ImageSectionInfo[SEGMENT_ARR_DEFAULT_SIZE];
			GetPESectionInfo(Kernel32.GetModuleHandle(module), arr);

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
			///     Executable code
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