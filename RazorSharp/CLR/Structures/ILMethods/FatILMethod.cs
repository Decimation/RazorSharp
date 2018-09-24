#region

#region

using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp.Pointers;

#endregion

// ReSharper disable InconsistentNaming

#endregion

namespace RazorSharp.CLR.Structures.ILMethods
{

	/// <summary>
	///     <para>Internal name: <c>COR_ILMETHOD_FAT</c></para>
	///     <para>This structure is the 'fat' layout, where no compression is attempted.</para>
	///     <para>Note that this structure can be added on at the end, thus making it extensible</para>
	///     <code>typedef struct tagCOR_ILMETHOD_FAT : IMAGE_COR_ILMETHOD_FAT</code>
	///     <para>Corresponding files:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/inc/corhlpr.h</description>
	///         </item>
	///     </list>
	///     <para>Lines of interest:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/inc/corhlpr.h: 510</description>
	///         </item>
	///     </list>
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct FatILMethod /* : IMAGE_COR_ILMETHOD_FAT */
	{
		/// <summary>
		///     This value is inherited from <see cref="IMAGE_COR_ILMETHOD_FAT" />
		/// </summary>
		[FieldOffset(0)] private readonly IMAGE_COR_ILMETHOD_FAT m_inheritedValue;

		// public uint Flags    => m_inlineValue.m_dword1 & 0xFFF;
		// public uint Size     => (m_inlineValue.m_dword1 >> 4) & 0xFFF;
		// public uint MaxStack => (m_inlineValue.m_dword1 >> 16) & 0xFFF;

		public uint CodeSize       => m_inheritedValue.m_codeSize;
		public uint LocalVarSigTok => m_inheritedValue.m_sigTok;


		/// <summary>
		///     Max stack size
		///     <code>
		///  return VAL16(*(USHORT*)((BYTE*)this+2));
		///  </code>
		/// </summary>
		public uint MaxStack => *(ushort*) ((byte*) Unsafe.AddressOf(ref this) + 2);

		/// <summary>
		///     <code>
		/// /* return Flags; */
		/// BYTE* p = (BYTE*)this;
		/// return ((unsigned)*(p+0)) | (( ((unsigned)*(p+1)) &amp; 0x0F) &lt;&lt; 8);
		/// </code>
		/// </summary>
		public uint FlagsValue {
			get {
				byte* p = (byte*) Unsafe.AddressOf(ref this);
				return ((uint) *p + 0) | ((((uint) *p + 1) & 0x0F) << 8);
			}
		}

		public CorILMethodFlags Flags => (CorILMethodFlags) FlagsValue;

		/// <summary>
		///     <code>
		/// return (*(BYTE*)this &amp; CorILMethod_FormatMask) == CorILMethod_FatFormat;
		/// </code>
		/// </summary>
		public bool IsFat => (CorILMethodFlags) (*(byte*) Unsafe.AddressOf(ref this) &
		                                         (byte) CorILMethodFlags.FormatMask) ==
		                     CorILMethodFlags.FatFormat;

		/// <summary>
		///     <code>
		/// /* return Size; */
		/// BYTE* p = (BYTE*)this;
		/// return *(p+1) &gt;&gt; 4;
		/// </code>
		/// </summary>
		public int Size {
			get {
				byte* p = (byte*) Unsafe.AddressOf(ref this);
				return (*p + 1) >> 4;
			}
		}

		/// <summary>
		///     <code>
		/// return(((BYTE*) this) + 4*GetSize());
		/// </code>
		/// </summary>
		public Pointer<byte> Code => (byte*) Unsafe.AddressOf(ref this) + 4 * Size;

		public override string ToString()
		{
			ConsoleTable table = new ConsoleTable("Field", "Value");
			table.AddRow("Flags", Runtime.CreateFlagsString(FlagsValue, Flags));
			table.AddRow("Size", Size);
			table.AddRow("Code size", CodeSize);
			table.AddRow("Code", Hex.ToHex(Code.Address));
			table.AddRow("IsFat", IsFat.Prettify());
			table.AddRow("Max stack size", MaxStack);
			table.AddRow("Local var sig tok", LocalVarSigTok);
			return table.ToMarkDownString();
		}
	}

	/// <summary>
	///     <para>Internal name: <c>IMAGE_COR_ILMETHOD_FAT</c></para>
	///     <para>This structure is the 'fat' layout, where no compression is attempted.</para>
	///     <para>Note that this structure can be added on at the end, thus making it extensible</para>
	///     <para>Corresponding files:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/inc/corhdr.h</description>
	///         </item>
	///     </list>
	///     <para>Lines of interest:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/inc/corhdr.h: 1238</description>
	///         </item>
	///     </list>
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	internal struct IMAGE_COR_ILMETHOD_FAT
	{
		/**
		 * 	unsigned Flags    : 12;     // Flags see code:CorILMethodFlags
			unsigned Size     :  4;     // size in DWords of this structure (currently 3)
			unsigned MaxStack : 16;     // maximum number of items (I4, I, I8, obj ...), on the operand stack
			DWORD   CodeSize;           // size of the code
			mdSignature   LocalVarSigTok;     // token that indicates the signature of the local vars (0 means none)
		 */

		// unsigned Flags    	: 12;     // Flags see code:CorILMethodFlags
		// unsigned Size     	:  4;     // size in DWords of this structure (currently 3)
		// unsigned MaxStack 	: 16;     // maximum number of items (I4, I, I8, obj ...), on the operand stack
		[FieldOffset(0)] internal readonly uint m_dword1;

		[FieldOffset(4)] internal readonly uint m_codeSize;
		[FieldOffset(8)] internal readonly uint m_sigTok;
	}


}