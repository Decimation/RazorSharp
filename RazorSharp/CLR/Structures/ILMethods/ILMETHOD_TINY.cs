using System.Runtime.InteropServices;
using RazorSharp.Pointers;
// ReSharper disable MemberCanBeMadeStatic.Global

namespace RazorSharp.CLR.Structures.ILMethods
{

	/// <summary>
	/// Internal name: <code>COR_ILMETHOD_TINY</code>
	///     <para>Used when the method is tiny (less than 64 bytes), and there are no local vars</para>
	///     <code>typedef struct tagCOR_ILMETHOD_TINY : IMAGE_COR_ILMETHOD_TINY</code>
	///     <para>Corresponding files:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/inc/corhlpr.h</description>
	///         </item>
	///     </list>
	///     <para>Lines of interest:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/inc/corhlpr.h: 473</description>
	///         </item>
	///     </list>
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct COR_ILMETHOD_TINY /* : IMAGE_COR_ILMETHOD_TINY */
	{
		/// <summary>
		/// This value is inherited from <see cref="IMAGE_COR_ILMETHOD_TINY"/>
		/// </summary>
		[FieldOffset(0)] private readonly IMAGE_COR_ILMETHOD_TINY m_inheritedValue;

		private byte Flags_CodeSize => m_inheritedValue.Flags_CodeSize;

		public bool IsTiny {
			get {
				return (Flags_CodeSize & (((uint) CorILMethodFlags.FormatMask) >> 1)) ==
				       (uint) CorILMethodFlags.TinyFormat;
			}
		}

		/// <summary>
		/// <code>
		/// return(((BYTE*) this) + sizeof(struct tagCOR_ILMETHOD_TINY));
		/// </code>
		/// </summary>
		public Pointer<byte> Code {
			get { return (((byte*) Unsafe.AddressOf(ref this)) + sizeof(IMAGE_COR_ILMETHOD_TINY)); }
		}

		public uint CodeSize {
			get { return ((uint) Flags_CodeSize) >> ((int) (CorILMethodFlags.FormatShift - 1)); }
		}

		public uint MaxStack       => 8;
		public uint LocalVarSigTok => 0;
	}

	/// <summary>
	///     <para>Used when the method is tiny (less than 64 bytes), and there are no local vars</para>
	///     <para>Corresponding files:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/inc/corhdr.h</description>
	///         </item>
	///     </list>
	///     <para>Lines of interest:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/inc/corhdr.h: 1230</description>
	///         </item>
	///     </list>
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	internal struct IMAGE_COR_ILMETHOD_TINY
	{
		[FieldOffset(0)] internal readonly byte Flags_CodeSize;
	}


}