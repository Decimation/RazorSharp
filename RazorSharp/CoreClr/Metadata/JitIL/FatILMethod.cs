using System.Runtime.InteropServices;
using RazorSharp.CoreClr.Meta;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;

// ReSharper disable FieldCanBeMadeReadOnly.Local

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Metadata.JitIL
{
	/// <summary>
	///     <para>
	///         CLR <see cref="FatILMethod" />. Functionality is implemented in this <c>struct</c> and exposed via
	///         <see cref="MetaIL" />
	///     </para>
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
	internal unsafe struct FatILMethod /* : IMAGE_COR_ILMETHOD_FAT */
	{
		#region Structs

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
		[StructLayout(LayoutKind.Sequential)]
		private struct IMAGE_COR_ILMETHOD_FAT
		{
			/// <summary>
			///     <para>Flags</para>
			///     <para>unsigned Flags : 12;</para>
			///     <para>Size in dwords of this structure (currently 3)</para>
			///     <para>unsigned Size : 4;</para>
			///     <para>Maximum number of items (I4, I, I8, obj ...), on the operand stack</para>
			///     <para>unsigned MaxStack : 16;</para>
			/// </summary>
			internal uint m_dword1;

			internal uint m_codeSize;

			internal uint m_sigTok;
		}

		#endregion

		/// <summary>
		///     This value is inherited from <see cref="IMAGE_COR_ILMETHOD_FAT" />
		/// </summary>
		[FieldOffset(0)]
		private readonly IMAGE_COR_ILMETHOD_FAT m_value;

		// public uint Flags    => m_inlineValue.m_dword1 & 0xFFF;
		// public uint Size     => (m_inlineValue.m_dword1 >> 4) & 0xFFF;
		// public uint MaxStack => (m_inlineValue.m_dword1 >> 16) & 0xFFF;

		internal uint CodeSize => m_value.m_codeSize;
		internal uint Token    => m_value.m_sigTok;


		/// <summary>
		///     Max stack size
		///     <code>
		///  return VAL16(*(USHORT*)((BYTE*)this+2));
		///  </code>
		/// </summary>
		internal uint MaxStackSize => *(ushort*) ((byte*) Unsafe.AddressOf(ref this) + 2);

		/// <summary>
		///     <code>
		/// /* return Flags; */
		/// BYTE* p = (BYTE*)this;
		/// return ((unsigned)*(p+0)) | (( ((unsigned)*(p+1)) &amp; 0x0F) &lt;&lt; 8);
		/// </code>
		/// </summary>
		internal CorILMethodFlags Flags {
			get {
				var p = (byte*) Unsafe.AddressOf(ref this);
				return (CorILMethodFlags) (((uint) *p + 0) | ((((uint) *p + 1) & 0x0F) << 8));
			}
		}


		/// <summary>
		///     <code>
		/// return (*(BYTE*)this &amp; CorILMethod_FormatMask) == CorILMethod_FatFormat;
		/// </code>
		/// </summary>
		internal bool IsFat => (CorILMethodFlags) (*(byte*) Unsafe.AddressOf(ref this) &
		                                           (byte) CorILMethodFlags.FormatMask) == CorILMethodFlags.FatFormat;

		/// <summary>
		///     <code>
		/// /* return Size; */
		/// BYTE* p = (BYTE*)this;
		/// return *(p+1) &gt;&gt; 4;
		/// </code>
		/// </summary>
		private int Size {
			get {
				var p = (byte*) Unsafe.AddressOf(ref this);
				return *(p + 1) >> 4;
			}
		}

		/// <summary>
		///     <code>
		/// return(((BYTE*) this) + 4*GetSize());
		/// </code>
		/// </summary>
		internal Pointer<byte> Code {
			get {
				var ptr = (byte*) Unsafe.AddressOf(ref this) + sizeof(int) * Size;
				return ptr;
			}
		}
	}
}