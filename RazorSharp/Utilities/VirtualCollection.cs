#region

using System.Collections;
using System.Collections.Generic;

#endregion

namespace RazorSharp.Utilities
{
	public class VirtualCollection<T> : IEnumerable<T>
	{
		public delegate T GetItem(string name);

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
			foreach (var v in ToArray()) yield return v;
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