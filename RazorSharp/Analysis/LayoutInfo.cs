using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;
using RazorSharp.Utilities;
using SimpleSharp;
using SimpleSharp.Diagnostics;
using Unsafe = RazorSharp.Memory.Unsafe;

// ReSharper disable ReturnTypeCanBeEnumerable.Local

namespace RazorSharp.Analysis
{
	public class LayoutInfo
	{
		private IReadableStructure[] m_fields;

		private readonly MetaType m_type;

		private readonly ConsoleTable m_table;

		/// <summary>
		/// Controls the options for layout inspection
		/// </summary>
		public InspectOptions Options { get; set; }

		private const string NAME_COL_STR = "Name";

		private int m_heapSize;
		
		private readonly int m_stackSize;

		internal LayoutInfo(Type type, InspectOptions options)
		{
			m_table     = new ConsoleTable();
			m_type      = type.GetMetaType();
			Options     = options;

			m_stackSize = m_type.IsStruct ? m_type.NumInstanceFieldBytes : IntPtr.Size;
			
			UpdateFields();
		}

		private void UpdateFields()
		{
			UpdateFields(GetBasicFields());
		}

		private void UpdateFields(List<IReadableStructure> fields)
		{
			m_table.Columns.Clear();
			m_table.Rows.Clear();
			m_table.AddColumn(NAME_COL_STR);
			m_fields = fields.ToArray();

			foreach (var field in m_fields) {
				m_table.AddRow(field.Name);
			}
		}

		private List<IReadableStructure> GetBasicFields()
		{
			// Handle special fields

			var fieldBuf = Options.HasFlagFast(InspectOptions.InternalStructures)
				? m_type.MemoryFields
				: m_type.InstanceFields;

			if (Options.HasFlagFast(InspectOptions.Padding)) {
				var list = fieldBuf.ToList();
				list.AddRange(m_type.Padding);
				return list;
			}

			return fieldBuf.ToList();
		}

		private IReadableStructure[] GetElementFields(object value) => m_type.GetElementFields(value);


		/// <summary>
		/// Sorts the <see cref="ConsoleTable"/> by <paramref name="options"/>
		/// </summary>
		public void SortNatural(InspectOptions options)
		{
			m_table.SortNatural(InspectUtil.ColumnNameMap[options]);
		}

		private void AttachColumn(string columnName, params object[] rowValues)
		{
			m_table.AttachEnd(columnName, rowValues);
		}

		private void HandleOption(InspectOptions options, Func<IReadableStructure, object> func)
		{
			if (HasOption(options)) {
				int lim      = m_fields.Length;
				var addrList = new object[lim];

				for (int i = 0; i < lim; i++) {
					var field = m_fields[i];
					addrList[i] = func(field);
				}

				AttachColumn(InspectUtil.ColumnNameMap[options], addrList);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool HasOption(InspectOptions options)
		{
			return Options.HasFlagFast(options) && !m_table.Columns.Contains(InspectUtil.ColumnNameMap[options]);
		}

		public void Populate<T>(ref T value)
		{
			// Make sure it's the same type
			Conditions.Require(value.GetType().MetadataToken == m_type.RuntimeType.MetadataToken);

			if (!Runtime.IsStruct(value)) {
				m_heapSize = Unsafe.SizeOf(value, SizeOfOptions.Heap);
			}

			// Reload fields
			if (Options.HasFlagFast(InspectOptions.ArrayOrString)) {
				var basicFields = GetBasicFields();
				basicFields.AddRange(GetElementFields(value));
				UpdateFields(basicFields);
				Populate();
			}

			// We can't use HandleOption due to the ref qualifier (it's needed)
			if (HasOption(InspectOptions.Addresses)) {
				var addrList = new object[m_fields.Length];

				for (int i = 0; i < m_fields.Length; i++) {
					addrList[i] = m_fields[i].GetAddress(ref value);
				}

				AttachColumn(InspectUtil.ColumnNameMap[InspectOptions.Addresses], addrList);
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

			if (Options.HasFlagFast(InspectOptions.MemoryOffsets)) {
				//SortNatural(InspectOptions.MemoryOffsets);
			}
			else if (Options.HasFlagFast(InspectOptions.FieldOffsets)) {
				SortNatural(InspectOptions.FieldOffsets);
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			if (Options.HasFlagFast(InspectOptions.AuxiliaryInfo)) {
				sb.AppendFormat("{0}:\n", m_type.Name);
				if (m_heapSize != 0) {
					sb.AppendFormat("Heap size: {0}\n", m_heapSize);
				}

				sb.AppendFormat("Stack size: {0}\n", m_stackSize);
			}
			
			sb.Append(m_table);

			return sb.ToString();
		}
	}
}