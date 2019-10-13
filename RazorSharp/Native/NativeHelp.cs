namespace RazorSharp.Native
{
	public static class NativeHelp
	{
		
		internal static unsafe string GetString(sbyte* first, int len)
		{
			if (first == null || len <= 0) {
				return null;
			}

//			return Marshal.PtrToStringAuto(new IntPtr(first), len)
//			              .Erase(StringConstants.NULL_TERMINATOR);

			return new string(first, 0, len);
			
			/*byte[] rg = new byte[len];
			Marshal.Copy(new IntPtr(first), rg, 0, rg.Length);
			return Encoding.ASCII.GetString(rg);*/
		}
	}
}