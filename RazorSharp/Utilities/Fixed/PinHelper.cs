using JetBrains.Annotations;
using RazorSharp.Memory;

// ReSharper disable ClassCannotBeInstantiated

namespace RazorSharp.Utilities.Fixed
{
	/// <summary>
	///     Allows for pinning of unblittable objects, similar to <see cref="ObjectPinner" /> but
	///     with the <c>fixed</c> statement.
	/// </summary>
	public static class PinHelper
	{
		/// <summary>
		///     Used for unsafe pinning of arbitrary objects.
		/// </summary>
		public static PinningHelper GetPinningHelper(object o)
		{
			return Unsafe.As<PinningHelper>(o);
		}


		/// <summary>
		///     <para>Helper class to assist with unsafe pinning of arbitrary objects. The typical usage pattern is:</para>
		///     <code>
		///  fixed (byte* pData = &amp;PinHelper.GetPinningHelper(value).Data)
		///  {
		///  }
		///  </code>
		///     <remarks>
		///         <para><c>pData</c> is what <c>Object::GetData()</c> returns in VM.</para>
		///         <para><c>pData</c> is also equal to offsetting the pointer by <see cref="OffsetOptions.FIELDS" />. </para>
		///         <para>From <see cref="System.Runtime.CompilerServices.JitHelpers" />. </para>
		///     </remarks>
		/// </summary>
		[UsedImplicitly]
		public sealed class PinningHelper
		{
			/// <summary>
			///     Represents the first field in an object, such as <see cref="OffsetOptions.FIELDS" />.
			/// </summary>
			public byte Data;

			private PinningHelper() { }
		}
	}
}