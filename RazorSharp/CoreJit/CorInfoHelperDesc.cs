#region

using System.Runtime.InteropServices;

#endregion

namespace RazorSharp.CoreJit
{
	//CORINFO_HELPER_DESC
	[StructLayout(LayoutKind.Sequential)]
	internal struct CorInfoHelperDesc
	{
		internal CorInfoHelpFunc helperNum;
		internal ushort          numArgs;

		[StructLayout(LayoutKind.Explicit)]
		internal struct Args
		{
			[FieldOffset(0)]
			private readonly uint fieldHandle;

			[FieldOffset(0)]
			private readonly uint methodHandle;

			[FieldOffset(0)]
			private readonly uint classHandle;

			[FieldOffset(0)]
			private readonly uint moduleHandle;

			[FieldOffset(0)]
			private readonly uint constant;
		}
	}
}