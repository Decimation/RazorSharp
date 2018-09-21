using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorSharp.Pointers;

namespace RazorSharp.CLR.Structures.ILMethods
{

	/// <summary>
	/// <para>Corresponding files:</para>
	/// <list type="bullet">
	///<item>
	///             <description>/src/inc/corhlpr.h</description>
	///         </item>
	/// </list>
	/// <para>Lines of interest:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/inc/corhlpr.h: 595</description>
	///         </item>
	///     </list>
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct COR_ILMETHOD
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

		[FieldOffset(0)] private COR_ILMETHOD_TINY m_tiny;
		[FieldOffset(0)] private COR_ILMETHOD_FAT  m_fat;

		public Pointer<COR_ILMETHOD_TINY> Tiny => Unsafe.AddressOf(ref m_tiny);
		public Pointer<COR_ILMETHOD_FAT>  Fat  => Unsafe.AddressOf(ref m_fat);

		public bool IsTiny => Tiny.Reference.IsTiny;
		public bool IsFat => Fat.Reference.IsFat;

		public Pointer<byte> Code {
			get { return IsTiny ? Tiny.Reference.Code : Fat.Reference.Code; }
		}

		public int CodeSize {
			get { return (int) (IsTiny ? Tiny.Reference.CodeSize : Fat.Reference.CodeSize); }
		}

		public int MaxStack {
			get { return (int) (IsTiny ? Tiny.Reference.MaxStack : Fat.Reference.MaxStack); }
		}

		public int LocalVarSigTok {

			get { return (int) (IsTiny ? Tiny.Reference.LocalVarSigTok : Fat.Reference.LocalVarSigTok); }
		}
	}

}