namespace RazorSharp.CoreClr.Metadata.Enums
{
	
//	[Flags]
	public enum CorInterfaceAttr
	{
		Dual        = 0, // Interface derives from IDispatch.
		Vtable      = 1, // Interface derives from IUnknown.
		Dispatch    = 2, // Interface is a dispinterface.
		Inspectable = 3, // Interface derives from IInspectable.
		Last        = 4, // The last member of the enum.
	}
	
	public enum CorElementType : byte
	{
		End  = 0x00,
		Void = 0x01,

		/// <summary>
		///     bool
		/// </summary>
		Boolean = 0x02,

		/// <summary>
		///     char
		/// </summary>
		Char = 0x03,

		/// <summary>
		///     sbyte
		/// </summary>
		I1 = 0x04,

		/// <summary>
		///     byte
		/// </summary>
		U1 = 0x05,

		/// <summary>
		///     short
		/// </summary>
		I2 = 0x06,

		/// <summary>
		///     ushort
		/// </summary>
		U2 = 0x07,

		/// <summary>
		///     int
		/// </summary>
		I4 = 0x08,

		/// <summary>
		///     uint
		/// </summary>
		U4 = 0x09,

		/// <summary>
		///     long
		/// </summary>
		I8 = 0x0A,

		/// <summary>
		///     ulong
		/// </summary>
		U8 = 0x0B,

		/// <summary>
		///     float
		/// </summary>
		R4 = 0x0C,

		/// <summary>
		///     double
		/// </summary>
		R8 = 0x0D,

		/// <summary>
		///     Note: strings don't actually map to this. They map to <see cref="Class" />
		/// </summary>
		String = 0x0E,

		Ptr   = 0x0F,
		ByRef = 0x10,

		/// <summary>
		///     Struct type
		/// </summary>
		ValueType = 0x11,

		/// <summary>
		///     Reference type (i.e. string, object)
		/// </summary>
		Class = 0x12,

		Var         = 0x13,
		Array       = 0x14,
		GenericInst = 0x15,
		TypedByRef  = 0x16,
		I           = 0x18,
		U           = 0x19,
		FnPtr       = 0x1B,
		Object      = 0x1C,
		SzArray     = 0x1D,
		MVar        = 0x1E,
		CModReqd    = 0x1F,
		CModOpt     = 0x20,
		Internal    = 0x21,
		Max         = 0x22,
		Modifier    = 0x40,
		Sentinel    = 0x41,
		Pinned      = 0x45
	}

	public enum CorTokenType : uint
	{
		Module                 = 0x00000000,
		TypeRef                = 0x01000000,
		TypeDef                = 0x02000000,
		FieldDef               = 0x04000000,
		MethodDef              = 0x06000000,
		ParamDef               = 0x08000000,
		InterfaceImpl          = 0x09000000,
		MemberRef              = 0x0a000000,
		CustomAttribute        = 0x0c000000,
		Permission             = 0x0e000000,
		Signature              = 0x11000000,
		Event                  = 0x14000000,
		Property               = 0x17000000,
		MethodImpl             = 0x19000000,
		ModuleRef              = 0x1a000000,
		TypeSpec               = 0x1b000000,
		Assembly               = 0x20000000,
		AssemblyRef            = 0x23000000,
		File                   = 0x26000000,
		ExportedType           = 0x27000000,
		ManifestResource       = 0x28000000,
		GenericParam           = 0x2a000000,
		MethodSpec             = 0x2b000000,
		GenericParamConstraint = 0x2c000000,
		String                 = 0x70000000,
		Name                   = 0x71000000,

		BaseType = 0x72000000 // Leave this on the high end value. This does not correspond to metadata table
	}
}