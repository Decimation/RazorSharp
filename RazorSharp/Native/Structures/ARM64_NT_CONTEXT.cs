using System;
using System.Runtime.InteropServices;
using RazorSharp.Common;

// ReSharper disable BuiltInTypeReferenceStyle

namespace RazorSharp.Native.Structures
{

	using WORD = UInt16;
	using DWORD = UInt32;
	using DWORD64 = UInt64;

	public enum AddressMode
	{
		/// <summary>
		/// 16:16 addressing. To support this addressing mode, you must supply a TranslateAddressProc64 callback function.
		/// </summary>
		AddrMode1616 = 0,

		/// <summary>
		/// 16:32 addressing. To support this addressing mode, you must supply a TranslateAddressProc64 callback function.
		/// </summary>
		AddrMode1632 = 1,

		/// <summary>
		/// Real-mode addressing. To support this addressing mode, you must supply a TranslateAddressProc64 callback function.
		/// </summary>
		AddrModeReal = 2,

		/// <summary>
		/// Flat addressing. This is the only addressing mode supported by the library.
		/// </summary>
		AddrModeFlat = 3,
	}


}