using System;
using System.Runtime.InteropServices;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities.Security;

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Metadata.JitIL
{
	[Flags]
	public enum CorILMethodFlags : byte
	{
		/// <summary>
		///     Call default constructor on all local vars
		/// </summary>
		InitLocals = 0x0010,

		/// <summary>
		///     There is another attribute after this one
		/// </summary>
		MoreSects = 0x0008,

		/// <summary>
		///     Not used.
		/// </summary>
		CompressedIL = 0x0040,

		/// <summary>
		///     Indicates the format for the <see cref="ILMethod" /> header
		/// </summary>
		FormatShift = 3,
		FormatMask = (1 << FormatShift) - 1,

		/// <summary>
		///     Use this code if the code size is even
		/// </summary>
		TinyFormat = 0x0002,
		SmallFormat = 0x0000,
		FatFormat   = 0x0003,

		/// <summary>
		///     use this code if the code size is odd
		/// </summary>
		TinyFormat1 = 0x0006
	}
	
	/// <summary>
	///     <para>Aggregates both <see cref="FatILMethod" /> and <see cref="TinyILMethod" /></para>
	///     <para>Internal name: <c>COR_ILMETHOD</c></para>
	///     <para>Corresponding files:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/inc/corhlpr.h</description>
	///         </item>
	///     </list>
	///     <para>Lines of interest:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/inc/corhlpr.h: 595</description>
	///         </item>
	///     </list>
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct ILMethod
	{
		/**
		 * union
	     * {
	     *     COR_ILMETHOD_TINY       Tiny;
	     *     COR_ILMETHOD_FAT        Fat;
	     * };
	     *     // Code follows the Header, then immediately after the code comes
	     *     // any sections (COR_ILMETHOD_SECT).
		 */

		[FieldOffset(default)]
		private TinyILMethod m_tiny;

		[FieldOffset(default)]
		private FatILMethod m_fat;

		private Pointer<TinyILMethod> Tiny => Unsafe.AddressOf(ref m_tiny);
		private Pointer<FatILMethod>  Fat  => Unsafe.AddressOf(ref m_fat);

		internal bool IsTiny => Tiny.Reference.IsTiny;

		internal bool IsFat => Fat.Reference.IsFat;

		internal byte[] RawIL => Code.Copy(CodeSize);

		internal CorILMethodFlags Flags {
			get {
				// todo: I don't know if the type has to be Fat or not, but just to be safe...
				if (!IsFat)
					throw Guard.CorILFail("IL method type must be Fat");

				return Fat.Reference.Flags;
			}
		}


		internal Pointer<byte> Code {
			get {
				Pointer<byte> code = IsTiny ? Tiny.Reference.Code : Fat.Reference.Code;
				return code;
			}
		}

		internal int CodeSize => (int) (IsTiny ? Tiny.Reference.CodeSize : Fat.Reference.CodeSize);

		internal int MaxStackSize => (int) (IsTiny ? TinyILMethod.MaxStackSize : Fat.Reference.MaxStackSize);

		/// <summary>
		/// LocalSignatureMetadataToken
		/// </summary>
		internal int Token => (int) (IsTiny ? TinyILMethod.Token : Fat.Reference.Token);
	}
}