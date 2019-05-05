using System.Runtime.InteropServices;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RazorSharp.Native
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct NativeModuleInfo
	{
		public void* lpBaseOfDll;
		public uint  SizeOfImage;
		public void* EntryPoint;
	}
}