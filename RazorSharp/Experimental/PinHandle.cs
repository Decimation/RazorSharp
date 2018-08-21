#region

using System;
using System.Threading;

#endregion

namespace RazorSharp.Experimental
{

	/// <inheritdoc />
	/// <summary>
	///     An object that pins a reference (so it doesn't change its address).
	///     <remarks>
	///         <para>Source: https://github.com/IllidanS4/SharpUtils/blob/master/Unsafe/Experimental/PinHandle.cs </para>
	///     </remarks>
	/// </summary>
	public abstract class PinHandle : IDisposable
	{
		/// <summary>
		///     Used to tell the pinning thread to stop pinning the object.
		/// </summary>
		protected AutoResetEvent Reset { get; set; }

		/// <summary>
		///     Initializes the pin handle.
		/// </summary>
		protected PinHandle()
		{
			Reset = new AutoResetEvent(false);
		}

		/// <summary>
		///     Finalizes the pin handle.
		/// </summary>
		~PinHandle()
		{
			Dispose(false);
		}

		/// <inheritdoc />
		/// <summary>
		///     Disposes the pin handle.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		///     Disposes the pin handle.
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			Reset.Set();
		}
	}

}