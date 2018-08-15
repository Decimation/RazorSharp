#region

using System;
using System.Runtime.InteropServices;

#endregion

namespace RazorSharp.Experimental
{

	[Obsolete("Use ObjectPinner", true)]
	internal struct PinHandle<T> where T : class
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

		public static PinHandle<T> Pin(ref T t)
		{
			var handle = new PinHandle<T>();
			if (Runtime.Runtime.IsBlittable<T>()) {
				handle.m_handle = GCHandle.Alloc(t, GCHandleType.Pinned);
			}
			else {
//				Runtime.Runtime.SpoofMethodTable<T, string>(ref t);
				handle.m_handle = GCHandle.Alloc(t, GCHandleType.Pinned);

//				Runtime.Runtime.RestoreMethodTable<string, T>(ref t);
			}

			return handle;
		}

		public void Unpin()
		{
			m_handle.Free();
		}
	}

}