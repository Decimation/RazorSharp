using System.Collections.Generic;

namespace RazorSharp.Analysis
{
	internal static class InspectUtil
	{
		/// <summary>
		/// <see cref="InspectOptions.Sizes"/>
		/// </summary>
		private const string SIZES_STR = "Size";

		/// <summary>
		/// <see cref="InspectOptions.Addresses"/>
		/// </summary>
		private const string ADDRESSES_STR = "Address";

		/// <summary>
		/// <see cref="InspectOptions.FieldOffsets"/>
		/// </summary>
		private const string FIELD_OFFSETS_STR = "Field Offset";

		/// <summary>
		/// <see cref="InspectOptions.Types"/>
		/// </summary>
		private const string TYPES_STR = "Type";

		/// <summary>
		/// <see cref="InspectOptions.Values"/>
		/// </summary>
		private const string VALUES_STR = "Value";

		/// <summary>
		/// <see cref="InspectOptions.MemoryOffsets"/>
		/// </summary>
		private const string MEM_OFFSETS_STR = "Memory Offset";


		internal static readonly Dictionary<InspectOptions, string> ColumnNameMap;

		static InspectUtil()
		{
			ColumnNameMap = new Dictionary<InspectOptions, string>
			{
				{InspectOptions.Sizes, SIZES_STR},
				{InspectOptions.Addresses, ADDRESSES_STR},
				{InspectOptions.FieldOffsets, FIELD_OFFSETS_STR},
				{InspectOptions.Types, TYPES_STR},
				{InspectOptions.Values, VALUES_STR},
				{InspectOptions.MemoryOffsets, MEM_OFFSETS_STR},
			};
		}
		
		

		internal static bool HasFlagFast(this InspectOptions value, InspectOptions flag)
		{
			return (value & flag) == flag;
		}
	}
}