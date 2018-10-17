using System;
using RazorSharp.CLR;
using RazorSharp.Memory;

namespace RazorSharp
{

	public static unsafe class Substrate
	{


		// 100663314
		[ClrSigcall("0F B7 41 06 4C 8D 05 45 6D 6F 00 8B D0 83 E2 1F")]
		public static int SizeOf(void* _)
		{
			return 0;
		}


		public static int Tk()
		{
			var sizeOf = Runtime.GetMethodDesc(typeof(Substrate), "SizeOf");

			return sizeOf.Reference.Token;
		}
	}

}