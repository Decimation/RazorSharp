using System.Collections.Generic;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.Import
{
	public sealed class ImportMap
	{
		private readonly Dictionary<string, Pointer<byte>> m_imports;

		public ImportMap()
		{
			m_imports = new Dictionary<string, Pointer<byte>>();
		}

		public Pointer<byte> this[string key] => m_imports[key];

		internal void Add(string key, Pointer<byte> value) => m_imports.Add(key, value);

		internal void Clear() => m_imports.Clear();
		
		public const string FIELD_NAME = "Imports";
	}
}