#region

using System;
using System.Runtime.InteropServices;

#endregion

namespace RazorSharp.Experimental
{

	[Obsolete("Use ObjectPinner", true)]
	internal struct PinHandleOld<T> where T : class
	{
		private GCHandle m_handle;

		public bool IsAllocated => m_handle.IsAllocated;
		public bool IsPinned    => GCHandleIsPinned(m_handle);


		#region Inlined GCHandle methods

		private static IntPtr GetRawHandle(GCHandle g)
		{
			return GCHandle.ToIntPtr(g);
		}

		private static bool GCHandleIsPinned(GCHandle g)
		{
			return GCHandleIsPinned(GetRawHandle(g));
		}

		private static bool GCHandleIsPinned(IntPtr handle)
		{
#if WIN32
            return (((int)handle) & 1) != 0;
#else
			return ((long) handle & 1) != 0;
#endif
		}

		private static IntPtr SetIsPinned(IntPtr handle)
		{
#if WIN32
           return new IntPtr(((int)handle) | 1);
#else
			return new IntPtr((long) handle | 1L);
#endif
		}

		#endregion

		public static PinHandleOld<T> Pin(ref T t)
		{
			PinHandleOld<T> handle = new PinHandleOld<T>();
			if (Runtime.Runtime.IsBlittable<T>()) {
				handle.m_handle = GCHandle.Alloc(t, GCHandleType.Pinned);
			}
			else {
				handle.m_handle = GCHandle.Alloc(t, GCHandleType.Pinned);
			}

			return handle;
		}

		public void Unpin()
		{
			m_handle.Free();
		}
	}

}