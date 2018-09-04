// ReSharper disable ClassNeverInstantiated.Global

// ReSharper disable ClassCannotBeInstantiated

namespace RazorSharp.CLR.Fixed
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion

	/// <summary>
	///     Allows for pinning of unblittable objects, similar to <see cref="ObjectPinner" /> but
	///     with the <c>fixed</c> statement.
	/// </summary>
	public static class PinHelper
	{

		/// <summary>
		///     Used for unsafe pinning of arbitrary objects.
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static PinningHelper GetPinningHelper(object o)
		{
			return CSUnsafe.As<PinningHelper>(o);
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
		///         <para><c>pData</c> is also equal to offsetting the pointer by <see cref="OffsetType.Fields" />. </para>
		///         <para>From <see cref="System.Runtime.CompilerServices.JitHelpers" />. </para>
		///     </remarks>
		/// </summary>
		public sealed class PinningHelper
		{
			/// <summary>
			///     Represents the first field in an object, such as <see cref="OffsetType.Fields" />.
			/// </summary>
			public byte Data;

			private PinningHelper() { }
		}
	}

}