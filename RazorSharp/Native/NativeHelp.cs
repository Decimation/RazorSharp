#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SimpleSharp.Strings;

#endregion

namespace RazorSharp.Native
{
	public static class NativeHelp
	{
		public static void Call(bool value, string name, bool throwOnFalse = false)
		{
			if (!value) {
				int win32Error = Marshal.GetLastWin32Error();
				int hResult    = Marshal.GetHRForLastWin32Error();

				string err = GetMessageForWin32Error(win32Error);


				string msg = String.Format("Function \"{0}\" failed. (Error: {3}) (Win32 error: {1}) (HRESULT: {2})",
				                           name,
				                           win32Error,
				                           hResult,
				                           err);

				Global.Log.Error("Function {Name} failed. Error: {Err} Code: {Code}",
				                 name, err, win32Error);

				if (throwOnFalse) {
					throw new Win32Exception(win32Error, msg);
				}
			}
		}

		public static void Call(bool value, bool throwOnFalse = false)
		{
			if (!value) {
				var    stackTrace = new StackFrame(1);
				string name       = stackTrace.GetMethod().Name;

				Call(false, name, throwOnFalse);
			}
		}

		public static byte[] GetBytes(string s)
		{
			var rg = new byte[s.Length];

			unsafe {
				fixed (char* c = s) {
					for (int i = 0; i < rg.Length; i++) {
						rg[i] = (byte) c[i];
					}
				}
			}
			

			return rg;
		}
		
		public static sbyte[] GetSBytes(string s)
		{
			var rg = new sbyte[s.Length];

			unsafe {
				fixed (char* c = s) {
					for (int i = 0; i < rg.Length; i++) {
						rg[i] = (sbyte) c[i];
					}
				}
			}
			

			return rg;
		}
		
		public static unsafe string GetString(sbyte* first)
		{
			return new string(first);
		}

		public static unsafe string GetStringAlt(sbyte* first, int len)
		{
			return new string(first, 0, len);
		}
		
		public static unsafe string GetString(sbyte* first, int len)
		{
			if (first == null || len <= 0) {
				return null;
			}

			return Marshal.PtrToStringAuto(new IntPtr(first), len)
			              .Erase(StringConstants.NULL_TERMINATOR);

			//return new string(first, 0, len);
			
			/*byte[] rg = new byte[len];
			Marshal.Copy(new IntPtr(first), rg, 0, rg.Length);
			return Encoding.ASCII.GetString(rg);*/
		}

		public static string GetMessageForWin32Error(int code)
		{
			return new Win32Exception(code).Message;
		}

		public static string GetLastWin32ErrorMessage()
			=> GetMessageForWin32Error(Marshal.GetLastWin32Error());

		public static unsafe string GetString(sbyte* first, uint len)
		{
			return GetString(first, (int) len);
		}
	}
}