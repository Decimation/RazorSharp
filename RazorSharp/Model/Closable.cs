using System;
using System.Diagnostics;

namespace RazorSharp.Model
{
	/// <summary>
	/// Describes a type that must be closed after usage. Implements <see cref="IDisposable"/>.
	/// </summary>
	public abstract class Closable : IDisposable
	{
		protected abstract string Id { get; }
		
		public virtual void Close()
		{
			Global.WriteLine("Closed \"{0}\"", Id);
		}

		public void Dispose() => Close();
	}
}