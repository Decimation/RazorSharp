namespace RazorSharp
{

	public class VirtualCollection<T>
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

		public T[] ToArray()
		{
			return m_fnGetItems();
		}
	}

}