using System;

namespace RazorSharp.Pointers
{

	public interface IPointer<T>
	{
		T this[int index] { get; set; }
		T      Value       { get; set; }
		ref T  Reference   { get; }
		IntPtr Address     { get; set; }
		int    ElementSize { get; }


		/// <summary>
		/// Reinterpret the underlying memory as the specified type.<para></para>
		///
		/// (Note: Not like CSUnsafe.As)
		/// </summary>
		/// <typeparam name="TNew"></typeparam>
		/// <returns></returns>
		TNew As<TNew>();

		int ToInt32();
		long ToInt64();
	}

}