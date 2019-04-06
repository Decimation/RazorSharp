#region

using RazorSharp.CoreClr.Structures;

#endregion

namespace RazorSharp.CoreClr.Enums
{
	/// <summary>
	///     <para>Sources:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>
	///                 /src/System.Private.CoreLib/src/System/Reflection/MdImport.cs: 22
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 /src/inc/corhdr.h: 863
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 /src/vm/siginfo.cpp: 63
	///             </description>
	///         </item>
	///     </list>
	///     <remarks>
	///         Use with <see cref="FieldDesc.CorType" />
	///     </remarks>
	/// </summary>
	public enum CorElementType
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
}