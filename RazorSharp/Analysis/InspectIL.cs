#region

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using RazorSharp.CoreClr;
using RazorSharp.Utilities;

// ReSharper disable ReturnTypeCanBeEnumerable.Global

#endregion

// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming

namespace RazorSharp.Analysis
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
			foreach (var opCode in ReflectionUtil.GetAllOpCodes()) {
				_opCodes.Add(opCode.Value, opCode);
			}
		}

		/// <summary>
		/// Gets the <see cref="OpCode"/> where <see cref="OpCode.Value"/> matches <paramref name="value"/>
		/// </summary>
		/// <param name="value"><see cref="OpCode.Value"/></param>
		/// <returns>Corresponding <see cref="OpCode"/></returns>
		internal static OpCode GetOpCode(short value) => _opCodes[value];

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

			throw new Exception();
		}

		public static Instruction[] GetInstructions(byte[] bytes)
		{
			var instructions = new List<Instruction>();

			int offset = 0;

			const short  CODE    = 0xFE;
			const ushort CODE_OR = 0xFE00;

			while (offset < bytes.Length) {
				var instruction = new Instruction {Offset = offset};

				short code = bytes[offset++];
				if (code == CODE) {
					code = (short) (bytes[offset++] | CODE_OR);
				}

				instruction.OpCode = GetOpCode(code);

				switch (instruction.OpCode.OperandType) {
					case OperandType.InlineBrTarget:
					case OperandType.InlineField:
					case OperandType.InlineI:
					case OperandType.InlineTok:
					case OperandType.InlineType:
					case OperandType.InlineSig:
					case OperandType.ShortInlineR:
						offset += sizeof(int);
						break;

					case OperandType.InlineR:
					case OperandType.InlineI8:
						offset += sizeof(long);
						break;

					case OperandType.InlineMethod:
						int token = BitConverter.ToInt32(bytes, offset);
						instruction.Data =  token;
						offset           += sizeof(int);
						break;

					case OperandType.InlineNone:
						break;

					case OperandType.InlineString:
						int mdString = BitConverter.ToInt32(bytes, offset);

						instruction.Data =  mdString;
						offset           += sizeof(int);
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

					default: throw new NotImplementedException();
				}

				instructions.Add(instruction);
			}

			return instructions.ToArray();
		}


		// Commit with old ILString
		// 7bff50a8777f9ff528e381d0b740d7e7bdcb760a
		// https://github.com/GeorgePlotnikov/ClrAnalyzer/blob/master/Win32Native/ildump.h
	}
}