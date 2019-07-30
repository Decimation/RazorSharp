using System;

namespace RazorSharp.Model
{
	/// <summary>
	/// Describes a type that must be closed after usage. Similar to <see cref="IDisposable"/>.
	/// </summary>
	internal interface IClosable
	{
		void Close();
	}
}