namespace RazorSharp.Native.Enums
{
	/// <summary>
	/// IMAGEHLP_SYMBOL_TYPE_INFO
	/// </summary>
	internal enum ImageHelpSymbolTypeInfo : uint
	{
		/// <summary>
		///    The symbol tag. The data type is DWORD.
		/// </summary>
		TI_GET_SYMTAG,

		/// <summary>
		///    The symbol name. The data type is WCHAR*. The caller must free the buffer.
		/// </summary>
		TI_GET_SYMNAME,

		/// <summary>
		///    The length of the type. The data type is ULONG64.
		/// </summary>
		TI_GET_LENGTH,

		/// <summary>
		///    The type. The data type is DWORD.
		/// </summary>
		TI_GET_TYPE,

		/// <summary>
		///    The type index. The data type is DWORD.
		/// </summary>
		TI_GET_TYPEID,

		/// <summary>
		///    The base type for the type index. The data type is DWORD.
		/// </summary>
		TI_GET_BASETYPE,

		/// <summary>
		///    The type index for index of an array type. The data type is DWORD.
		/// </summary>
		TI_GET_ARRAYINDEXTYPEID,

		/// <summary>
		///    The type index of all children. The data type is a pointer to a
		///    TI_FINDCHILDREN_PARAMS structure. The Count member should be initialized
		///    with the number of children.
		/// </summary>
		TI_FINDCHILDREN,

		/// <summary>
		///    The data kind. The data type is DWORD.
		/// </summary>
		TI_GET_DATAKIND,

		/// <summary>
		///    The address offset. The data type is DWORD.
		/// </summary>
		TI_GET_ADDRESSOFFSET,

		/// <summary>
		///    The offset of the type in the parent. Members can use this to get their
		///    offset in a structure. The data type is DWORD.
		/// </summary>
		TI_GET_OFFSET,

		/// <summary>
		///    The value of a constant or enumeration value. The data type is VARIANT.
		/// </summary>
		TI_GET_VALUE,

		/// <summary>
		///    The count of array elements. The data type is DWORD.
		/// </summary>
		TI_GET_COUNT,

		/// <summary>
		///    The number of children. The data type is DWORD.
		/// </summary>
		TI_GET_CHILDRENCOUNT,

		/// <summary>
		///    The bit position of a bitfield. The data type is DWORD.
		/// </summary>
		TI_GET_BITPOSITION,

		/// <summary>
		///    A value that indicates whether the base class is virtually inherited. The
		///    data type is BOOL.
		/// </summary>
		TI_GET_VIRTUALBASECLASS,

		/// <summary>
		///    The symbol interface of the type of virtual table, for a user-defined type. The data type is DWORD.
		/// </summary>
		TI_GET_VIRTUALTABLESHAPEID,

		/// <summary>
		///    The offset of the virtual base pointer. The data type is DWORD.
		/// </summary>
		TI_GET_VIRTUALBASEPOINTEROFFSET,

		/// <summary>
		///    The type index of the class parent. The data type is DWORD.
		/// </summary>
		TI_GET_CLASSPARENTID,

		/// <summary>
		///    A value that indicates whether the type index is nested. The data type is
		///    DWORD.
		/// </summary>
		TI_GET_NESTED,

		/// <summary>
		///    The symbol index for a type. The data type is DWORD.
		/// </summary>
		TI_GET_SYMINDEX,

		/// <summary>
		///    The lexical parent of the type. The data type is DWORD.
		/// </summary>
		TI_GET_LEXICALPARENT,

		/// <summary>
		///    The index address. The data type is ULONG64.
		/// </summary>
		TI_GET_ADDRESS,

		/// <summary>
		///    The offset from the this pointer to its actual value. The data type is
		///    DWORD.
		/// </summary>
		TI_GET_THISADJUST,

		/// <summary>
		///    The UDT kind. The data type is DWORD.
		/// </summary>
		TI_GET_UDTKIND,

		/// <summary>
		///    The equivalency of two types. The data type is DWORD. The value is S_OK is
		///    the two types are equivalent, and S_FALSE otherwise.
		/// </summary>
		TI_IS_EQUIV_TO,

		/// <summary>
		///    The calling convention. The data type is DWORD.
		/// </summary>
		TI_GET_CALLING_CONVENTION,

		/// <summary>
		///    The equivalency of two symbols. This is not guaranteed to be accurate. The
		///    data type is DWORD. The value is S_OK is the two types are equivalent, and
		///    S_FALSE otherwise.
		/// </summary>
		TI_IS_CLOSE_EQUIV_TO,

		/// <summary>
		///    The element where the valid request bitfield should be stored. The data
		///    type is ULONG64.
		///
		///    This value is only used with the SymGetTypeInfoEx function.
		/// </summary>
		TI_GTIEX_REQS_VALID,

		/// <summary>
		///    The offset in the virtual function table of a virtual function. The data
		///    type is DWORD.
		/// </summary>
		TI_GET_VIRTUALBASEOFFSET,

		/// <summary>
		///    The index into the virtual base displacement table. The data type is DWORD.
		/// </summary>
		TI_GET_VIRTUALBASEDISPINDEX,

		/// <summary>
		///    Indicates whether a pointer type is a reference. The data type is Boolean.
		/// </summary>
		TI_GET_IS_REFERENCE,

		/// <summary>
		///    Indicates whether the user-defined data type is an indirect virtual base.
		///    The data type is BOOL.
		/// </summary>
		TI_GET_INDIRECTVIRTUALBASECLASS,
		IMAGEHLP_SYMBOL_TYPE_INFO_MAX,
	}
}