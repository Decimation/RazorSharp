using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using RazorSharp.Utilities.Security;

// ReSharper disable ReturnTypeCanBeEnumerable.Local

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Metadata.JitIL
{
	public static class InspectIL
	{
		/// <summary>
		/// <para>Key: <see cref="OpCode.Value"/></para>
		/// <para>Value: <see cref="OpCode"/></para>
		/// </summary>
		private static readonly Dictionary<short, OpCode> _opCodes = new Dictionary<short, OpCode>();

		static InspectIL()
		{
			foreach (var opCode in GetAllOpCodes()) {
				_opCodes.Add(opCode.Value, opCode);
			}
		}

		/// <summary>
		/// Gets the <see cref="OpCode"/> where <see cref="OpCode.Value"/> matches <paramref name="value"/>
		/// </summary>
		/// <param name="value"><see cref="OpCode.Value"/></param>
		/// <returns>Corresponding <see cref="OpCode"/></returns>
		private static OpCode GetOpCode(short value) => _opCodes[value];

		public static Instruction[] GetInstructions(MethodBase methodBase)
		{
			var methodBody = methodBase.GetMethodBody();

			byte[] bytes = methodBody != null ? methodBody.GetILAsByteArray() : new byte[] { };

			return GetInstructions(bytes);
		}

		private static MethodBase Resolve(int token, MethodBase methodBase)
		{
			Type[] genericMethodArgs = null;

			if (methodBase.IsGenericMethod) {
				genericMethodArgs = methodBase.GetGenericArguments();
			}

			if (methodBase.DeclaringType != null) {
				Type[] genericTypeArgs = methodBase.DeclaringType.GetGenericArguments();

				return methodBase.Module.ResolveMethod(token, genericTypeArgs, genericMethodArgs);
			}

			throw Guard.CorILFail();
		}

		public static Instruction[] GetInstructions(byte[] bytes)
		{
			var instructions = new List<Instruction>();

			int offset = 0;

			const short  CODE    = 0xFE;
			const ushort CODE_OR = 0xFE00;

			while (offset < bytes.Length) {
				var instruction = new Instruction {Offset = offset};

				int origOffset = offset;

				short code = bytes[offset++];

				if (code == CODE) {
					code = (short) (bytes[offset++] | CODE_OR);
				}

				instruction.OpCode = GetOpCode(code);

				switch (instruction.OpCode.OperandType) {
					case OperandType.InlineI:
						int value = BitConverter.ToInt32(bytes, offset);
						instruction.Operand =  value;
						offset              += sizeof(int);
						break;

					case OperandType.ShortInlineR:
					case OperandType.InlineBrTarget:
						offset += sizeof(int);
						break;

					case OperandType.InlineR:
						offset += sizeof(long);
						break;

					case OperandType.InlineI8:
						long lvalue = BitConverter.ToInt64(bytes, offset);
						instruction.Operand =  lvalue;
						offset              += sizeof(long);
						break;

					case OperandType.InlineTok:
					case OperandType.InlineType:
					case OperandType.InlineSig:
					case OperandType.InlineField:
					case OperandType.InlineMethod:
						int token = BitConverter.ToInt32(bytes, offset);
						instruction.Operand =  token;
						offset              += sizeof(int);
						break;

					case OperandType.InlineNone:
						break;

					case OperandType.InlineString:
						int mdString = BitConverter.ToInt32(bytes, offset);

						instruction.Operand =  mdString;
						offset              += sizeof(int);
						break;

					case OperandType.InlineSwitch:
						int count = BitConverter.ToInt32(bytes, offset) + 1;
						offset += sizeof(int) * count;
						break;


					case OperandType.InlineVar:
						offset += sizeof(short);
						break;

					case OperandType.ShortInlineVar:
					case OperandType.ShortInlineBrTarget:
					case OperandType.ShortInlineI:
						offset += sizeof(byte);
						break;

					default: throw Guard.NotImplementedFail(nameof(GetInstructions));
				}


				var size = offset - origOffset;
				var raw  = bytes.Skip(origOffset).Take(size).ToArray();

				instructions.Add(instruction);
			}

			return instructions.ToArray();
		}


		// Commit with old ILString
		// 7bff50a8777f9ff528e381d0b740d7e7bdcb760a
		// https://github.com/GeorgePlotnikov/ClrAnalyzer/blob/master/Win32Native/ildump.h


		private static OpCode[] GetAllOpCodes()
		{
			FieldInfo[] opCodesFields = typeof(OpCodes).GetFields();
			var         opCodes       = new OpCode[opCodesFields.Length];

			for (int i = 0; i < opCodes.Length; i++) {
				opCodes[i] = (OpCode) opCodesFields[i].GetValue(null);
			}

			return opCodes;
		}
	}
}