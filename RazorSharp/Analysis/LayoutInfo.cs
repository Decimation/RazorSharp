using System;
using System.Collections.Generic;
using System.Linq;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures;
using SimpleSharp;

namespace RazorSharp.Analysis
{
	public unsafe class LayoutInfo
	{
		private ConsoleTable m_table;

		public LayoutInfo()
		{
			m_table = new ConsoleTable("Name");
		}

		internal ConsoleTable Table => m_table;

		public void Sort() { }

		

		// This must be called after all the columns are created
		public void AddInternalStructures(GadgetOptions options) { }

		public void AddFieldNames(MetaField[] fields)
		{
			foreach (var field in fields) {
				m_table.AddRow(field.Name);
			}
		}

		public void AttachColumn(string columnName, params object[] rowValues)
		{
			m_table.AttachEnd(columnName, rowValues);
		}

		public override string ToString()
		{
			return m_table.ToString();
		}
	}
}