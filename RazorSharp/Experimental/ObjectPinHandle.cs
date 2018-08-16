using System.Threading;

namespace RazorSharp.Experimental
{

	/// <summary>
	/// <para>Pins an object on the heap, so its address stays unchanged during the lifetime of this object.</para>
	/// <para> Use only if <see cref="System.Runtime.InteropServices.GCHandle"/> cannot be used for the object.</para>
	/// <para>Note that this class doesn't provide a way to get the data pointer, like GCHandle does, and that's for the same reason a pinned GCHandle cannot be used for some objects.</para>
	///
	/// <para>Source: https://github.com/IllidanS4/SharpUtils/blob/master/Unsafe/Experimental/PinHandle.cs </para>
	/// </summary>
	public class ObjectPinHandle : PinHandle
	{
		/// <summary>
		/// Gets the pinned object.
		/// </summary>
		public object Object { get; private set; }

		/// <summary>
		/// Pins an object in a memory and constructs its pin handle.
		/// </summary>
		/// <param name="obj">The object to pin.</param>
		public ObjectPinHandle(object obj)
		{
			Object = obj;

			using (AutoResetEvent re1 = new AutoResetEvent(false)) {
				Thread thr = new Thread(
					delegate()
					{
						ObjectPinner.InvokeWhilePinned(obj,
							delegate
							{
								re1.Set();
								Reset.WaitOne();
							});
					}
				);
				thr.Start();
				re1.WaitOne();
			}
		}
	}

}