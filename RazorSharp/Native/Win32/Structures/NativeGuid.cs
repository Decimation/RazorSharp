#region

using System.Runtime.InteropServices;

#endregion

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RazorSharp.Native.Win32.Structures
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct NativeGuid
	{
		//	typedef struct _GUID {
		//		unsigned long  Data1;
		//		unsigned short Data2;
		//		unsigned short Data3;
		//		unsigned char  Data4[ 8 ];
		//	} GUID;

		internal       ulong  Data1;
		internal       ushort Data2;
		internal       ushort Data3;
		internal fixed byte   Data4[sizeof(ulong)];
	}
}