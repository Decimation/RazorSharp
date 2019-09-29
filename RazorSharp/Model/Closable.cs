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
			Debug.WriteLine(String.Format("Closed \"{0}\"", Id), "Closable");
		}

		public void Dispose() => Close();
	}
}