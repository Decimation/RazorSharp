using System;
using System.Collections.Generic;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;

namespace RazorSharp.Analysis
{
	[Flags]
	public enum InspectOptions
	{
//		FieldNames = 0,

		None = 0,

		/// <summary>
		/// Display the size of the field (<see cref="IReadableStructure.Size"/>)
		/// </summary>
		Sizes = 1,

		/// <summary>
		/// Display the address of the field (<see cref="IReadableStructure.GetAddress{T}(ref T)"/>)
		/// <remarks>Requires a value</remarks>
		/// </summary>
		Addresses = 2,

		/// <summary>
		/// The offset (<see cref="IReadableStructure.Offset"/>) relative to the address returned by
		/// <see cref="Unsafe.AddressOfHeap{T}(T,OffsetOptions)"/> with <see cref="OffsetOptions.FIELDS"/>
		/// </summary>
		FieldOffsets = 4,

		/// <summary>
		/// Display the type of the field (<see cref="IReadableStructure.TypeName"/>)
		/// </summary>
		Types = 8,

		/// <summary>
		/// Display the value of the field (<see cref="IReadableStructure.GetValue"/>)
		/// <remarks>Requires a value</remarks>
		/// </summary>
		Values = 16,

		/// <summary>
		/// Display internal runtime structures such as <see cref="MethodTable"/> and <see cref="ObjHeader"/>
		/// <remarks><see cref="MethodTableField"/>, <see cref="ObjectHeaderField"/></remarks>
		/// </summary>
		InternalStructures = 32,

		/// <summary>
		/// Display field offsets relative to the address returned by
		/// <see cref="Unsafe.AddressOfHeap{T}(T,OffsetOptions)"/> with <see cref="OffsetOptions.NONE"/>
		/// </summary>
		MemoryOffsets = 64,
		
		Padding = 128,
	}

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
		private const string VALUES_STR = "Sizes";

		/// <summary>
		/// <see cref="InspectOptions.MemoryOffsets"/>
		/// </summary>
		private const string MEM_OFFSETS_STR = "Memory Offset";

		internal static readonly Dictionary<InspectOptions, string> StringName;

		static InspectUtil()
		{
			StringName = new Dictionary<InspectOptions, string>
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