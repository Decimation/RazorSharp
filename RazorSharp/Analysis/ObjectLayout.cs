using System;
using ObjectLayoutInspector;
using RazorCommon;
using RazorSharp.Pointers;

namespace RazorSharp.Analysis
{

	/// <summary>
	/// Displays the layout of objects in memory.<para></para>
	///
	/// If a reference type is specified, the heap layout is displayed.<para></para>
	///
	/// If a value type is specified, the stack layout is displayed.<para></para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public unsafe class ObjectLayout<T>
	{
		private readonly IntPtr       m_addr;
		private readonly ConsoleTable m_table;
		private readonly TypeLayout   m_layout;
		private readonly T            m_value;
		private const    string       Omitted = "-";
		public           bool         FieldsOnly { get; set; }
		public           bool         FullOffset { get; set; }

		public ObjectLayout(ref T t)
		{
			FieldsOnly = true;
			FullOffset = false;

			var addr = Unsafe.AddressOf(ref t);
			m_addr = addr;
			if (!typeof(T).IsValueType) {
				// Point to heap
				m_addr = *(IntPtr*) addr;
			}

			m_table  = new ConsoleTable("Offset", "Address", "Size", "Type", "Name", "Value");
			m_layout = TypeLayout.GetLayout<T>();

			m_value = t;

			Create();
		}

		private void Create()
		{
			const string objHeaderType   = "ObjHeader";
			const string methodTableType = "MethodTable*";
			const string objHeaderName   = "(Object header)";
			const string methodTableName = "(MethodTable ptr)";
			const string bytePaddingType = "Byte";
			const string bytePaddingName = "(padding)";

			int baseOfs = 0;
			if (!typeof(T).IsValueType & !FieldsOnly) {
				m_table.AddRow(-IntPtr.Size, Hex.ToHex(m_addr - IntPtr.Size), IntPtr.Size, objHeaderType,
					objHeaderName, Omitted);
				m_table.AddRow(0, Hex.ToHex(m_addr), IntPtr.Size, methodTableType, methodTableName, Omitted);
				baseOfs += IntPtr.Size;
			}

			foreach (var v in m_layout.Fields) {
				var rightOfs = baseOfs + v.Offset;
				var leftOfs  = rightOfs + (v.Size - 1);

				string ofsStr;
				if (FullOffset)
					ofsStr = $"{rightOfs}-{leftOfs}";
				else {
					ofsStr = rightOfs.ToString();
				}

				if (v.GetType() != typeof(Padding)) {
					FieldLayout fl = (FieldLayout) v;
					m_table.AddRow(ofsStr, Hex.ToHex(m_addr + v.Offset + baseOfs), v.Size, fl.FieldInfo.FieldType.Name,
						fl.FieldInfo.Name, fl.FieldInfo.GetValue(m_value));
				}
				else {
					m_table.AddRow(ofsStr, Hex.ToHex(m_addr + v.Offset + baseOfs), v.Size, bytePaddingType, bytePaddingName, 0);
				}
			}

			// TypeLayout says strings have 2 bytes of padding but that isn't true
			if (typeof(T) == typeof(string)) {
				m_table.Rows.RemoveAt(m_table.Rows.Count - 1);
			}
		}

		public override string ToString()
		{
			return Inspector<T>.CreateLabelString("Memory layout:", m_table);
		}
	}

}