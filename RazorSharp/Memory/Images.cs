#region

using System;
using System.Diagnostics;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.Memory
{

	internal static class Images
	{
		public static IntPtr GetAddress(string dll, long offset)
		{
			ProcessModule module = Modules.GetModule(dll);
			Pointer<byte> addr   = module.BaseAddress;
			addr.Add(offset);
			return addr.Address;
		}
	}

}