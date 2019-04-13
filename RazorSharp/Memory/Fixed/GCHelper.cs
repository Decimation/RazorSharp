#region

using System;
using System.Runtime.InteropServices;

#endregion

namespace RazorSharp.Memory.Fixed
{
	public class GCHelper : IDisposable
	{
		private GCHandle m_handle;

		private GCHelper(object o)
		{
			m_handle = GCHandle.Alloc(o, GCHandleType.Pinned);
		}


		public void Dispose()
		{
			m_handle.Free();
		}

		public static GCHelper Pin(object o)
		{
			return new GCHelper(o);
		}
	}
}