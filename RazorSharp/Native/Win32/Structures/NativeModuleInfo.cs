using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RazorSharp.Native.Win32.Structures
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct NativeModuleInfo
	{
		internal void* lpBaseOfDll;
		internal uint  SizeOfImage;
		internal void* EntryPoint;
	}
}