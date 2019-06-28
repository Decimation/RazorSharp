using System;
using RazorSharp.CoreClr.Meta.Interfaces;
using RazorSharp.CoreClr.Meta.Virtual;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;

namespace RazorSharp.Analysis
{
	/// <summary>
	/// Controls how objects are inspected. Field/structure names are implicitly enabled.
	/// </summary>
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
		/// <remarks><see cref="MethodTableField"/>, <see cref="ObjectHeaderField"/>; not a column</remarks>
		/// </summary>
		InternalStructures = 32,

		/// <summary>
		/// Display field offsets relative to the address returned by
		/// <see cref="Unsafe.AddressOfHeap{T}(T,OffsetOptions)"/> with <see cref="OffsetOptions.NONE"/>
		/// </summary>
		MemoryOffsets = 64,
		
		/// <summary>
		/// Display padding.
		/// <remarks><see cref="PaddingField"/>; not a column</remarks>
		/// </summary>
		Padding = 128,
		
		/// <summary>
		/// Display <see cref="string"/> or array elements.
		/// <remarks>
		/// <see cref="ElementField"/>; not a column
		/// <para>This has significant overhead as fields and data have to be reloaded.</para>
		/// </remarks>
		/// </summary>
		ArrayOrString = 256,
		
		/// <summary>
		/// Display miscellaneous/auxiliary info such as <see cref="Unsafe.HeapSize{T}(T)"/>
		/// and <see cref="Unsafe.SizeOf{T}()"/>
		/// <remarks>Requires a value to compute heap size; not a column</remarks>
		/// </summary>
		AuxiliaryInfo = 512,
		
		
		Recursive = 1024, // todo
	}
}