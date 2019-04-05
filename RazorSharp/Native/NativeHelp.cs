using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace RazorSharp.Native
{
	public static unsafe class NativeHelp
	{
		public static string GetString(sbyte* first, int len)
		{
			if (first == null || len <= 0) {
				return null;
			}
			return Marshal.PtrToStringAuto(new IntPtr(first), len);
		}

		public static string GetString(sbyte* first, uint len) => GetString(first, (int) len);

		public static string GetMessageForWin32Error(int code)
		{
			return new Win32Exception(code).Message;
		}
	}
}