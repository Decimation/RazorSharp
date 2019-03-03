#region

using System;
using System.Runtime.InteropServices;
using RazorSharp.Native;
using RazorSharp.Pointers;
using Serilog.Context;

#endregion

namespace RazorSharp.Experimental
{
	internal static class Registers
	{
		private static void TestRsp()
		{
			byte[] opCodes =
			{
				0x48, 0x89, 0xE0,       // mov 		rax,rsp
				0x48, 0x83, 0xC0, 0x08, // add    	rax,0x8
				0xC3                    // ret
			};

			var code = NativeFunctions.CodeAlloc(opCodes);

			Pointer<byte> rsp = Marshal.GetDelegateForFunctionPointer<GetRsp>(code)();
			using (LogContext.PushProperty(Global.CONTEXT_PROP, "testRSP")) {
				Global.Log.Information("rsp: {Ptr}", rsp);
				Global.Log.Information("rsp + 0xB0: {Ptr}", rsp + 0xB0);
				Global.Log.Information("getRSP(): {Ptr}", GetRspValue());
			}

			NativeFunctions.CodeFree(code);
		}

		public static Pointer<byte> GetRspValue()
		{
			byte[] opCodes =
			{
				0x48, 0x89, 0xE0,       // mov 		rax,rsp
				0x48, 0x83, 0xC0, 0x08, // add    	rax,0x8
				0xC3                    // ret
			};

			var code = NativeFunctions.CodeAlloc(opCodes);

			Pointer<byte> rsp = Marshal.GetDelegateForFunctionPointer<GetRsp>(code)();


			// rsp += 0xB0; //
			rsp += 150;
			rsp += 0xCA;
			rsp -= 0x3A8; // Subtracting this offset makes RSP match in WinDbg but breaks it in VS registers view?


			NativeFunctions.CodeFree(code);
			return rsp;
		}


		private delegate IntPtr GetRsp();
	}
}