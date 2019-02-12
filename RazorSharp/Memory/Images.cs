#region

using System;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.Memory
{
	internal static class Images
	{
		internal static IntPtr GetAddress(string dll, long offset)
		{
			var           module = Modules.GetModule(dll);
			Pointer<byte> addr   = module.BaseAddress;
			addr.Add(offset);
			return addr.Address;
		}
	}
}