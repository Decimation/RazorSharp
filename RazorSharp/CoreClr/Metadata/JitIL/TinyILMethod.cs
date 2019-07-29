using System.Runtime.InteropServices;
using RazorSharp.CoreClr.Meta;
using RazorSharp.Memory.Pointers;

// ReSharper disable FieldCanBeMadeReadOnly.Local

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Metadata.JitIL
{
	/// <summary>
	///     <para>
	///         CLR <see cref="TinyILMethod" />. Functionality is implemented in this <c>struct</c> and exposed via
	///         <see cref="MetaIL" />
	///     </para>
	///     <para>Internal name: <c>COR_ILMETHOD_TINY</c></para>
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
	internal unsafe struct TinyILMethod /* : IMAGE_COR_ILMETHOD_TINY */
	{
		#region Structs

		/// <summary>
		///     <para>Internal name: <c>IMAGE_COR_ILMETHOD_TINY</c></para>
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
		private struct IMAGE_COR_ILMETHOD_TINY
		{
			[FieldOffset(default)]
			internal byte m_flagsAndCodeSize;
		}

		#endregion

		/// <summary>
		///     This value is inherited from <see cref="IMAGE_COR_ILMETHOD_TINY" />
		/// </summary>
		[FieldOffset(default)]
		private IMAGE_COR_ILMETHOD_TINY m_value;

		/// <summary>
		///     Contains both <see cref="CodeSize" /> and <see cref="CorILMethodFlags" />
		/// </summary>
		private byte FlagsAndCodeSize => m_value.m_flagsAndCodeSize;

		internal bool IsTiny {
			get {
				bool v = (FlagsAndCodeSize & ((uint) CorILMethodFlags.FormatMask >> 1)) ==
				         (uint) CorILMethodFlags.TinyFormat;
				return v;
			}
		}

		/// <summary>
		///     <code>
		/// return(((BYTE*) this) + sizeof(struct tagCOR_ILMETHOD_TINY));
		/// </code>
		/// </summary>
		internal Pointer<byte> Code {
			get {
				fixed (TinyILMethod* thisPtr = &this) {
					var value = (byte*) thisPtr;

					return value + sizeof(IMAGE_COR_ILMETHOD_TINY);
				}
			}
		}

		internal uint CodeSize => (uint) FlagsAndCodeSize >> (int) (CorILMethodFlags.FormatShift - 1);

		internal const uint MaxStackSize = 8;
		internal const uint Token        = 0;
	}
}