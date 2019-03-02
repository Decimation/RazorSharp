#region

using System;
using System.Runtime.InteropServices;
using RazorSharp.Native.Structures.Images;

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.Native
{
	public static unsafe class DbgHelp
	{
		private const string DBG_HELP_DLL = "DbgHelp.dll";


		[DllImport(DBG_HELP_DLL)]
		private static extern ImageNtHeaders64* ImageNtHeader(IntPtr hModule);


		public static ImageSectionInfo[] GetPESectionInfo(IntPtr hModule)
		{
			// get the location of the module's IMAGE_NT_HEADERS structure
			ImageNtHeaders64* pNtHdr = ImageNtHeader(hModule);

			// section table immediately follows the IMAGE_NT_HEADERS
			var pSectionHdr = (IntPtr) (pNtHdr + 1);
			var imageBase   = hModule;

			var arr = new ImageSectionInfo[pNtHdr->FileHeader.NumberOfSections];

			for (int scn = 0; scn < pNtHdr->FileHeader.NumberOfSections; ++scn) {
				var struc = Marshal.PtrToStructure<ImageSectionHeader>(pSectionHdr);

				arr[scn] = new ImageSectionInfo(scn, struc.Name, (void*) (imageBase.ToInt64() + struc.VirtualAddress),
				                                (int) struc.VirtualSize, struc);

				pSectionHdr += Marshal.SizeOf<ImageSectionHeader>();
			}

			return arr;
		}
	}
}