#region

using System.Runtime.InteropServices;

#endregion

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RazorSharp.Native
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct GUID
	{
		//	typedef struct _GUID {
		//		unsigned long  Data1;
		//		unsigned short Data2;
		//		unsigned short Data3;
		//		unsigned char  Data4[ 8 ];
		//	} GUID;

		public       ulong  Data1;
		public       ushort Data2;
		public       ushort Data3;
		public fixed byte   Data4[8];
	}
}