using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RazorSharp.Native
{
	public static class NativeFunctions
	{
		public static void Call(bool value, string name, bool throwOnFalse = false)
		{
			if (!value) {
				var win32Error = Marshal.GetLastWin32Error();
				var hResult    = Marshal.GetHRForLastWin32Error();

				string err = NativeHelp.GetMessageForWin32Error(win32Error);


				string msg = String.Format("Function \"{0}\" failed. (Error: {3}) (Win32 error: {1}) (HRESULT: {2})",
				                           name,
				                           win32Error,
				                           hResult,
				                           err);

				Global.Log.Error("Function {Name} failed. Error: {Err} Code: {Code}",
				                 name,err, win32Error);

				if (throwOnFalse) {
					throw new Win32Exception(win32Error, msg);
				}
			}
		}

		public static void Call(bool value, bool throwOnFalse = false)
		{
			if (!value) {
				var stackTrace = new StackFrame(1);
				var name       = stackTrace.GetMethod().Name;

				Call(false, name, throwOnFalse);
			}
		}
	}
}