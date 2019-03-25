#region

using System;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Utilities;
using RazorSharp.Pointers;
using RazorSharp.Utilities.Exceptions;

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Structures.ILMethods
{
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
	internal struct ILMethod
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

		[FieldOffset(0)]
		private TinyILMethod m_tiny;

		[FieldOffset(0)]
		private FatILMethod m_fat;

		private Pointer<TinyILMethod> Tiny => Unsafe.AddressOf(ref m_tiny);
		private Pointer<FatILMethod>  Fat  => Unsafe.AddressOf(ref m_fat);

		internal bool IsTiny => Tiny.Reference.IsTiny;
		internal bool IsFat  => Fat.Reference.IsFat;

		internal void WriteIL(byte[] rgOpCodes)
		{
			Code.SafeWrite(rgOpCodes);
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


		internal byte[] GetILAsByteArray()
		{
			return Code.CopyOut(CodeSize);
		}

		internal CorILMethodFlags Flags {
			get {
				// todo: I don't know if the type has to be Fat or not, but just to be safe...
				if (!IsFat) 
					throw new Exception("IL method type must be Fat");

				return Fat.Reference.Flags;
			}
		}


		internal Pointer<byte> Code {
			get {
				var code = IsTiny ? Tiny.Reference.Code : Fat.Reference.Code;
				return code;
			}
		}


		internal int CodeSize => (int) (IsTiny ? Tiny.Reference.CodeSize : Fat.Reference.CodeSize);

		internal int MaxStack => (int) (IsTiny ? Tiny.Reference.MaxStack : Fat.Reference.MaxStack);

		internal int LocalVarSigTok => (int) (IsTiny ? Tiny.Reference.LocalVarSigTok : Fat.Reference.LocalVarSigTok);

		internal ConsoleTable ToTable()
		{
			var table = new ConsoleTable("Info", "Value");

			table.AddRow("Type", IsTiny ? "Tiny" : "Fat");
			table.AddRow("Code", Code.ToString("P"));
			table.AddRow("Code mem", Collections.CreateString(GetILAsByteArray(), ToStringOptions.Hex));
			table.AddRow("Code size", CodeSize);
			table.AddRow("Max stack", MaxStack);
			table.AddRow("Local sig token", LocalVarSigTok);
			table.AddRow("Flags", IsFat ? EnumUtil.CreateString(Flags) : "-");

			return table;
		}
		
		public override string ToString()
		{
			return ToTable().ToMarkDownString();
		}
	}
}