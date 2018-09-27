#region

using System.Runtime.InteropServices;
using RazorSharp.Pointers;

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.CLR.Structures.ILMethods
{

	/// <summary>
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

		[FieldOffset(0)] private TinyILMethod m_tiny;
		[FieldOffset(0)] private FatILMethod  m_fat;

		public Pointer<TinyILMethod> Tiny => Unsafe.AddressOf(ref m_tiny);
		public Pointer<FatILMethod>  Fat  => Unsafe.AddressOf(ref m_fat);

		public bool IsTiny => Tiny.Reference.IsTiny;
		public bool IsFat  => Fat.Reference.IsFat;

		public void WriteIL(byte[] rgOpCodes)
		{
			for (int i = 0; i < rgOpCodes.Length; i++) {
				Code.ForceWrite(rgOpCodes[i], i);
			}
		}

		/*public OpCode[] OpCodes {
			get { return ReflectionUtil.GetOpCodes(Code, CodeSize); }
		}

		public void PrintIL()
		{
			var table  = new ConsoleTable("Address", "Name", "Value", "Size");
			var ilCode = Code;
			foreach (var opCode in OpCodes) {
				table.AddRow(Hex.ToHex(ilCode.Address), opCode.Name, Hex.ToHex(opCode.Value), opCode.Size);
				ilCode.Add(opCode.Size);
			}

			Console.WriteLine(table.ToMarkDownString());
		}

		public void WriteOpCodes(OpCode[] opCodes)
		{
			var ilCode      = Code;
			var runningSize = 0;
			foreach (var opCode in opCodes) {
				switch (opCode.Size) {
					case sizeof(byte):

						ilCode.ForceWrite<byte>((byte) opCode.Value);

						break;
					case sizeof(short):

						ilCode.ForceWrite<short>(opCode.Value);
						break;
				}

				ilCode.Add(opCode.Size);
				runningSize += opCode.Size;
				if (runningSize > CodeSize) throw new Exception("Code overflow");
			}
		}*/

		/// <summary>
		///     <remarks>
		///         Equals <see cref="System.Reflection.MethodBody.GetILAsByteArray()" />
		///     </remarks>
		/// </summary>
		/// <returns></returns>
		public byte[] GetILAsByteArray()
		{
			return Code.CopyOut(CodeSize);
		}

		/// <summary>
		///     Points to the JIT IL code
		/// </summary>
		public Pointer<byte> Code => IsTiny ? Tiny.Reference.Code : Fat.Reference.Code;

		/// <summary>
		///     Length/size of the IL code (<see cref="Code" />)
		/// </summary>
		public int CodeSize => (int) (IsTiny ? Tiny.Reference.CodeSize : Fat.Reference.CodeSize);

		/// <summary>
		///     <remarks>
		///         Equals <see cref="System.Reflection.MethodBody.MaxStackSize" />
		///     </remarks>
		/// </summary>
		public int MaxStack => (int) (IsTiny ? Tiny.Reference.MaxStack : Fat.Reference.MaxStack);

		/// <summary>
		///     <remarks>
		///         Equals <see cref="System.Reflection.MethodBody.LocalSignatureMetadataToken" />
		///     </remarks>
		/// </summary>
		public int LocalVarSigTok => (int) (IsTiny ? Tiny.Reference.LocalVarSigTok : Fat.Reference.LocalVarSigTok);
	}

}