using System;

namespace RazorSharp.Pointers
{

	public interface IPointer<T>
	{
		T this[int index] { get; set; }
		T      Value       { get; set; }
		IntPtr Address     { get; set; }
		int    ElementSize { get; }

		int ToInt32();
		long ToInt64();
	}

}