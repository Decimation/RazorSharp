// ReSharper disable InconsistentNaming

using System;
using System.Runtime.InteropServices;

namespace RazorSharp.Memory.Fixed
{
	public class GCHelper : IDisposable
	{
		private GCHandle m_handle;

		private GCHelper(object o)
		{
			m_handle = GCHandle.Alloc(o, GCHandleType.Pinned);
		}

		public static GCHelper Pin(object o)
		{
			return new GCHelper(o);
		}


		public void Dispose()
		{
			m_handle.Free();
		}
	}
}