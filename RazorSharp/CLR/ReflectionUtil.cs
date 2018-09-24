#region

using System.Reflection;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.CLR
{

	internal static unsafe class ReflectionUtil
	{
		/**
		 * one_byte_opcodes = new OpCode [0xe1];
			two_bytes_opcodes = new OpCode [0x1f];

			var fields = GetOpCodeFields ();

			for (int i = 0; i < fields.Length; i++) {
				var opcode = (OpCode) fields [i].GetValue (null);
				if (opcode.OpCodeType == OpCodeType.Nternal)
					continue;

				if (opcode.Size == 1)
					one_byte_opcodes [opcode.Value] = opcode;
				else
					two_bytes_opcodes [opcode.Value & 0xff] = opcode;
			}
		 */

		internal static Pointer<byte> GetPointerForPointerField<TInstance>(Pointer<FieldDesc> pFd, ref TInstance inst)
		{
			object value = pFd.Reference.GetValue(inst);
			return Pointer.Unbox(value);
		}

		/*internal static readonly OpCode[] SingleByteOpCodes = new OpCode[0xE1];
		internal static readonly OpCode[] DoubleByteOpCodes = new OpCode[0x1F];

		static ReflectionUtil()
		{
			CacheOpCodes();
		}


		internal static void CacheOpCodes()
		{
			var opCodes = GetAllOpCodes();
			for (int i = 0; i < opCodes.Length; i++) {
				if (opCodes[i].OpCodeType == OpCodeType.Nternal) continue;
				if (opCodes[i].Size == sizeof(byte)) {
					SingleByteOpCodes[opCodes[i].Value] = opCodes[i];
				}
				else {
					DoubleByteOpCodes[opCodes[i].Value & 0xFF] = opCodes[i];
				}
			}
		}

		internal static int GetFullOpCodeSize(OpCode op)
		{
			int      size = op.Size;

			switch (op.OperandType) {
//				case OperandType.InlineSwitch:
//					return size + (1 + ((Instruction[]) op.).Length) * 4;
				case OperandType.InlineI8:
				case OperandType.InlineR:
					return size + 8;
				case OperandType.InlineBrTarget:
				case OperandType.InlineField:
				case OperandType.InlineI:
				case OperandType.InlineMethod:
				case OperandType.InlineString:
				case OperandType.InlineTok:
				case OperandType.InlineType:
				case OperandType.ShortInlineR:
				case OperandType.InlineSig:
					return size + 4;

//				case OperandType.InlineArg:
				case OperandType.InlineVar:
					return size + 2;
				case OperandType.ShortInlineBrTarget:
				case OperandType.ShortInlineI:

//				case OperandType.ShortInlineArg:
				case OperandType.ShortInlineVar:
					return size + 1;
				default:
					return size;
			}
		}

		internal static OpCode[] GetOpCodes(Pointer<byte> ilCode, int codeSize)
		{
			var matchingOpcodes = new List<OpCode>();


			for (int i = 0; i < codeSize; i++) {
				// byte op = il.ReadByte ();
				// return op != 0xfe
				// 	? one_byte_opcodes [op]
				// 	: two_bytes_opcodes [il.ReadByte ()];
				byte op = ilCode.Read<byte>(i);
				matchingOpcodes.Add(op != 0xFE ? SingleByteOpCodes[op] : DoubleByteOpCodes[ilCode.Read<byte>(i + 1)]);
			}


			return matchingOpcodes.ToArray();
		}

		internal static OpCode[] GetOpCodes(byte[] rgIL)
		{
			fixed (byte* b = rgIL) {
				return GetOpCodes(b, rgIL.Length);
			}
		}

		internal static OpCode[] GetAllOpCodes()
		{
			var fields = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static);
			OpCode[] rgOpCodes = (from v in fields where v.FieldType == typeof(OpCode) select (OpCode) v.GetValue(null))
				.ToArray();
			return rgOpCodes;
		}*/
	}

}