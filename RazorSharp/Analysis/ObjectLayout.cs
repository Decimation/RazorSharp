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
		private const    char         Omitted = '-';
		public           bool         FieldsOnly { get; set; }
		public           bool         FullOffset { get; set; }

		public ObjectLayout(ref T t)
		{
			if (typeof(T).IsArray) {
				throw new Exception($"Layout of arrays ({typeof(T).Name}) cannot be calculated");
			}

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
			m_value  = t;

			Create();
		}

		private string GetOffsetString(int baseOfs, int rightOfs, int leftOfs)
		{
			string ofsStr;
			if (!FieldsOnly) {
				ofsStr = FullOffset ? $"{rightOfs}-{leftOfs}" : rightOfs.ToString();
			}
			else {
				// Offset relative to the fields (-IntPtr.Size)
				ofsStr = FullOffset ? $"{rightOfs - baseOfs}-{leftOfs - baseOfs}" : (rightOfs - baseOfs).ToString();
			}

			return ofsStr;
		}

		private void CreateInternalInfo()
		{
			const string objHeaderType   = "ObjHeader";
			const string methodTableType = "MethodTable*";
			const string objHeaderName   = "(Object header)";
			const string methodTableName = "(MethodTable ptr)";

			if (!FieldsOnly) {
				m_table.AddRow(-IntPtr.Size, Hex.ToHex(m_addr - IntPtr.Size), IntPtr.Size, objHeaderType,
					objHeaderName, Omitted);
				m_table.AddRow(0, Hex.ToHex(m_addr), IntPtr.Size, methodTableType, methodTableName, Omitted);
			}
		}


		private void Create()
		{
			const string bytePaddingType = "Byte";
			const string bytePaddingName = "(padding)";

			int baseOfs = 0;
			if (!typeof(T).IsValueType) {
				baseOfs += IntPtr.Size;
				CreateInternalInfo();
			}


			foreach (var v in m_layout.Fields) {
				var rightOfs = baseOfs + v.Offset;
				var leftOfs  = rightOfs + (v.Size - 1);

				string ofsStr = GetOffsetString(baseOfs, rightOfs, leftOfs);

				if (v.GetType() != typeof(Padding)) {
					FieldLayout fl = (FieldLayout) v;
					m_table.AddRow(ofsStr, Hex.ToHex(m_addr + v.Offset + baseOfs), v.Size,
						fl.FieldInfo.FieldType.Name,
						fl.FieldInfo.Name, fl.FieldInfo.GetValue(m_value));
				}
				else {
					m_table.AddRow(ofsStr, Hex.ToHex(m_addr + v.Offset + baseOfs), v.Size, bytePaddingType,
						bytePaddingName, 0);
				}
			}

			if (typeof(T) == typeof(string)) {
				// TypeLayout says strings have 2 bytes of padding but that isn't true
				m_table.Rows.RemoveAt(m_table.Rows.Count - 1);

				var       str        = m_value as string;
				const int charOffset = 6;


				// ReSharper disable once PossibleNullReferenceException
				for (int i = 0; i < str.Length - 1; i++) {
					var rightOfs = baseOfs + charOffset + (i * sizeof(char));
					var leftOfs  = rightOfs + (sizeof(char) - 1);

					string ofsStr = GetOffsetString(baseOfs, rightOfs, leftOfs);

					var addr = m_addr + charOffset + (i * sizeof(char)) + baseOfs;
					m_table.AddRow(ofsStr, Hex.ToHex(addr), sizeof(char), typeof(char).Name, Omitted,
						Memory.Memory.Read<char>(addr));
				}
			}
		}

		public override string ToString()
		{
			return Inspector<T>.CreateLabelString("Memory layout:", m_table);
		}
	}

}