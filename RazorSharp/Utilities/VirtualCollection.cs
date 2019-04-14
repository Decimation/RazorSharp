#region

using System.Collections;
using System.Collections.Generic;
// ReSharper disable ReturnTypeCanBeEnumerable.Global

#endregion

namespace RazorSharp.Utilities
{
	/// <inheritdoc />
	/// <summary>
	///     Represents a collection implemented by delegates.
	/// </summary>
	public class VirtualCollection<T> : IEnumerable<T>
	{
		/// <summary>
		///     Retrieves an item with the name <paramref name="name" />
		/// </summary>
		public delegate T GetItem(string name);

		/// <summary>
		///     Retrieves the items as an array.
		/// </summary>
		public delegate T[] GetItems();

		private readonly GetItem  m_fnGetItem;
		private readonly GetItems m_fnGetItems;

		internal VirtualCollection(GetItem fnGetItem, GetItems fnGetItems)
		{
			m_fnGetItem  = fnGetItem;
			m_fnGetItems = fnGetItems;
		}

		public T this[string name] => m_fnGetItem(name);

		public T this[int index] => m_fnGetItems()[index];

		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>) ToArray()).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public T[] ToArray()
		{
			return m_fnGetItems();
		}
	}
}