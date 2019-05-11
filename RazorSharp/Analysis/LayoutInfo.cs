using System;
using System.Collections.Generic;
using System.Linq;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;
using RazorSharp.Utilities;
using SimpleSharp;

namespace RazorSharp.Analysis
{
	public unsafe class LayoutInfo
	{
		
		private readonly IReadableStructure[] m_fields;
		
		private readonly MetaType m_type;

		private ConsoleTable m_table;
		
		/// <summary>
		/// Controls the options for layout inspection
		/// </summary>
		public InspectOptions Options { get; set; }

		public ConsoleTable Table => m_table;
		
		
		internal LayoutInfo(Type t, InspectOptions options)
		{
			const string NAME_COL_STR = "Name";
			
			m_table = new ConsoleTable(NAME_COL_STR);

			m_type  = t.GetMetaType();
			Options = options;

			// Handle special fields
			
			var fieldBuf = Options.HasFlagFast(InspectOptions.InternalStructures)
				? m_type.MemoryFields
				: m_type.InstanceFields;

			if (Options.HasFlagFast(InspectOptions.Padding)) {
				var list = fieldBuf.ToList();
				list.AddRange(m_type.Padding);
				m_fields = list.ToArray();
			}
			else {
				m_fields = fieldBuf.ToArray();
			}

			foreach (var field in m_fields) {
				m_table.AddRow(field.Name);
			}
		}

		

		/// <summary>
		/// Sorts the <see cref="ConsoleTable"/> by <paramref name="options"/>
		/// </summary>
		public void SortNatural(InspectOptions options)
		{
			m_table.SortNatural(InspectUtil.StringName[options]);
		}


		private void AttachColumn(string columnName, params object[] rowValues)
		{
			m_table.AttachEnd(columnName, rowValues);
		}

		private void HandleOption(InspectOptions options, Func<IReadableStructure, object> func)
		{
			if (Options.HasFlagFast(options)) {
				int lim      = m_fields.Length;
				var addrList = new object[lim];

				for (int i = 0; i < lim; i++) {
					addrList[i] = func(m_fields[i]);
				}

				AttachColumn(InspectUtil.StringName[options], addrList);
			}
		}

		public void Populate<T>(ref T value)
		{
			// We can't use HandleOption due to the ref qualifier (it's needed)
			if (Options.HasFlagFast(InspectOptions.Addresses)) {
				var addrList = new object[m_fields.Length];

				for (int i = 0; i < m_fields.Length; i++) {
					addrList[i] = m_fields[i].GetAddress(ref value);
				}

				AttachColumn(InspectUtil.StringName[InspectOptions.Addresses], addrList);
			}

			var valueCpy = value;

			HandleOption(InspectOptions.Values, field => field.GetValue(valueCpy));
		}

		public void Populate()
		{
			HandleOption(InspectOptions.Sizes, field => field.Size);
			HandleOption(InspectOptions.FieldOffsets, field => field.Offset);
			HandleOption(InspectOptions.MemoryOffsets, field => field.MemoryOffset);
			HandleOption(InspectOptions.Types, field => field.TypeName);

			if (Options.HasFlagFast(InspectOptions.FieldOffsets)) {
				
				//SortNatural(InspectOptions.FieldOffsets);
				
			}
		}

		
		public override string ToString()
		{
			return m_table.ToString();
		}
	}
}