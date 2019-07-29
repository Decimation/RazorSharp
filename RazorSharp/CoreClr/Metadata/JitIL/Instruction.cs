using System;
using System.Reflection;
using System.Reflection.Emit;
using SimpleSharp.Strings.Formatting;

namespace RazorSharp.CoreClr.Metadata.JitIL
{
	/// <summary>
	/// Represents an IL instruction.
	/// </summary>
	public struct Instruction
	{
		public int Offset { get; internal set; }

		public OpCode OpCode { get; internal set; }

		public object Operand { get; internal set; }

		public bool IsMethodCall => Operand is MethodInfo;

		public bool IsConstructorCall => Operand is MethodInfo m && m.IsConstructor;

		public override string ToString()
		{
			string dataString;

			if (Operand != null) {
				if (!Hex.TryCreateHex(Operand, out dataString)) {
					dataString = Operand.ToString();
				}
			}
			else {
				dataString = String.Empty;
			}

			return String.Format("IL_{0:X}: {1} (opcode: {2:X}) {3}", Offset, OpCode, OpCode.Value, dataString);

//			return String.Format("IL_{0:X}: {1} ({2:X}, {3}) {4}", Offset, OpCode, OpCode.Value,OpCode.OperandType , dataString);
		}
	}
}