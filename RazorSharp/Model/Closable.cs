using System;
using System.Diagnostics;
using RazorSharp.Core;

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
			Global.WriteLine("{0}::{1}", Id, nameof(Close));
		}

		public void Dispose() => Close();
	}
}