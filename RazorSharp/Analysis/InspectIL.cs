#region

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures.ILMethods;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities;

// ReSharper disable ReturnTypeCanBeEnumerable.Global

#endregion

// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming

namespace RazorSharp.Analysis
{
	internal static unsafe class InspectIL
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
		public static OpCode GetOpCode(short value) => _opCodes[value];
		
		public static Instruction[] GetInstructions(MethodBase methodBase)
		{
			var methodBody = methodBase.GetMethodBody();

			byte[] bytes = methodBody != null ? methodBody.GetILAsByteArray() : new byte[] { };

			return GetInstructions(bytes);
		}

		private static MethodBase Resolve(int token,MethodBase methodBase)
		{
			Type[] genericMethodArguments = null;
			if (methodBase.IsGenericMethod) {
				genericMethodArguments = methodBase.GetGenericArguments();
			}

			var genericArguments = methodBase.DeclaringType.GetGenericArguments();

			return methodBase.Module.ResolveMethod(token, genericArguments,
			                                                   genericMethodArguments);
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
						instruction.Data = token;
						offset += sizeof(int);
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

					default:
						throw new NotImplementedException();
				}

				instructions.Add(instruction);
			}

			return instructions.ToArray();
		}
		

		/*internal static string ILString(MethodInfo mi)
		{
			Pointer<ILMethod> il = mi.GetMethodDesc().Reference.GetILHeader();
			return ILString(il.Reference.Code.ToPointer<byte>(), il.Reference.CodeSize);
		}

		internal static string ILString(byte[] il)
		{
			fixed (byte* b = il) {
				return ILString(b, il.Length);
			}
		}

		/// <summary>
		///     https://github.com/GeorgePlotnikov/ClrAnalyzer/blob/master/Win32Native/ildump.h
		/// </summary>
		internal static string ILString(byte* ilCode, int len)
		{
			// todo: don't use literals here
			int i, j, k;
			var sb = new StringBuilder();

			for (i = 0; i < len; i++) {
				sb.AppendFormat("IL_{0:X}: ", i);
				switch (ilCode[i]) {
					case 0x00:
						sb.Append("nop");
						break;
					case 0x01:
						sb.Append("break");
						break;
					case 0x02:
						sb.Append("ldarg.0");
						break;
					case 0x03:
						sb.Append("ldarg.1");
						break;
					case 0x04:
						sb.Append("ldarg.2");
						break;
					case 0x05:
						sb.Append("ldarg.3");
						break;
					case 0x06:
						sb.Append("ldloc.0");
						break;
					case 0x07:
						sb.Append("ldloc.1");
						break;
					case 0x08:
						sb.Append("ldloc.2");
						break;
					case 0x09:
						sb.Append("ldloc.3");
						break;
					case 0x0a:
						sb.Append("stloc.0");
						break;
					case 0x0b:
						sb.Append("stloc.1");
						break;
					case 0x0c:
						sb.Append("stloc.2");
						break;
					case 0x0d:
						sb.Append("stloc.3");
						break;
					case 0x0e: // ldarg.s X
						sb.AppendFormat("ldarg.s 0x{0:X}", ilCode[i + 1]);
						i += 1;
						break;
					case 0x0f: // ldarga.s X
						sb.AppendFormat("ldarga.s 0x{0:X}", ilCode[i + 1]);
						i += 1;
						break;
					case 0x10: // starg.s X
						sb.AppendFormat("starg.s 0x{0:X}", ilCode[i + 1]);
						i += 1;
						break;
					case 0x11: // ldloc.s X
						sb.AppendFormat("ldloc.s 0x{0:X}", ilCode[i + 1]);
						i += 1;
						break;
					case 0x12: // ldloca.s X
						sb.AppendFormat("ldloca.s 0x{0:X}", ilCode[i + 1]);
						i += 1;
						break;
					case 0x13: // stloc.s X
						sb.AppendFormat("stloc.s 0x{0:X}", ilCode[i + 1]);
						i += 1;
						break;
					case 0x14:
						sb.Append("ldnull");
						break;
					case 0x15:
						sb.Append("ldc.i4.m1");
						break;
					case 0x16:
						sb.Append("ldc.i4.0");
						break;
					case 0x17:
						sb.Append("ldc.i4.1");
						break;
					case 0x18:
						sb.Append("ldc.i4.2");
						break;
					case 0x19:
						sb.Append("ldc.i4.3");
						break;
					case 0x1a:
						sb.Append("ldc.i4.4");
						break;
					case 0x1b:
						sb.Append("ldc.i4.5");
						break;
					case 0x1c:
						sb.Append("ldc.i4.6");
						break;
					case 0x1d:
						sb.Append("ldc.i4.7");
						break;
					case 0x1e:
						sb.Append("ldc.i4.8");
						break;
					case 0x1f: // ldc.i4.s X
						sb.AppendFormat("ldc.i4.s 0x{0:X}", ilCode[i + 1]);
						i += 1;
						break;
					case 0x20: // ldc.i4 XXXX
						sb.AppendFormat("ldc.i4 0x{0:X}{1:X}{2:X}{3:X}", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x21: // ldc.i8 XXXXXXXX
						sb.AppendFormat("ldc.i8 0x{0:X}{1:X}{2:X}{3:X}{4:X}{5:X}{6:X}{7:X}", ilCode[i + 8],
						                ilCode[i + 7],
						                ilCode[i + 6],
						                ilCode[i + 5], ilCode[i + 4], ilCode[i + 3], ilCode[i + 2], ilCode[i + 1]);
						i += 8;
						break;
					case 0x22: // ldc.r4 XXXX
						sb.AppendFormat("ldc.r4 float32(0x{0:X}{1:X}{2:X}{3:X})", ilCode[i + 4], ilCode[i + 3],
						                ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x23: // ldc.r8 XXXXXXXX
						sb.AppendFormat("ldc.r8 float64(0x{0:X}{1:X}{2:X}{3:X}{4:X}{5:X}{6:X}{7:X})", ilCode[i + 8],
						                ilCode[i + 7],
						                ilCode[i + 6], ilCode[i + 5], ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 8;
						break;
					case 0x25:
						sb.Append("dup");
						break;
					case 0x26:
						sb.Append("pop");
						break;
					case 0x27: // JMP <T>
						sb.AppendFormat("jmp <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x28: // call <T>
						sb.AppendFormat("call <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x29: // calli <T>
						sb.AppendFormat("calli <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x2a:
						sb.Append("ret");
						break;
					case 0x2b: // br.s X
						sb.AppendFormat("br.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x2c: // brfalse.s X
						sb.AppendFormat("brfalse.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x2d: // brtrue.s X
						sb.AppendFormat("brtrue.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x2e: // beq.s X
						sb.AppendFormat("beq.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x2f: // bgt.s X
						sb.AppendFormat("bgt.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x30: // bgt.s X
						sb.AppendFormat("bgt.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x31: // ble.s X
						sb.AppendFormat("ble.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x32: // blt.s X
						sb.AppendFormat("blt.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x33: // bne.un.s X
						sb.AppendFormat("bne.un.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x34: // bge.un.s X
						sb.AppendFormat("bge.un.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x35: // bgt.un.s X
						sb.AppendFormat("bgt.un.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x36: // ble.un.s X
						sb.AppendFormat("ble.un.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x37: // blt.un.s X
						sb.AppendFormat("blt.un.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x38: // br XXXX
						sb.AppendFormat("br IL_{0:X}",
						                i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                         ilCode[i + 1]));
						i += 4;
						break;
					case 0x39: // brfalse XXXX
						sb.AppendFormat("brfalse IL_{0:X}",
						                i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                         ilCode[i + 1]));
						i += 4;
						break;
					case 0x3a: // brtrue XXXX
						sb.AppendFormat("brtrue IL_{0:X}",
						                i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                         ilCode[i + 1]));
						i += 4;
						break;
					case 0x3b: // beq XXXX
						sb.AppendFormat("beq IL_{0:X}",
						                i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                         ilCode[i + 1]));
						i += 4;
						break;
					case 0x3c: // bgt XXXX
						sb.AppendFormat("bgt IL_{0:X}",
						                i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                         ilCode[i + 1]));
						i += 4;
						break;
					case 0x3d: // bgt XXXX
						sb.AppendFormat("bgt IL_{0:X}",
						                i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                         ilCode[i + 1]));
						i += 4;
						break;
					case 0x3e: // ble XXXX
						sb.AppendFormat("ble IL_{0:X}",
						                i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                         ilCode[i + 1]));
						i += 4;
						break;
					case 0x3f: // blt XXXX
						sb.AppendFormat("blt IL_{0:X}",
						                i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                         ilCode[i + 1]));
						i += 4;
						break;
					case 0x40: // bne.un XXXX
						sb.AppendFormat("bne.un IL_{0:X}",
						                i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                         ilCode[i + 1]));
						i += 4;
						break;
					case 0x41: // bge.un XXXX
						sb.AppendFormat("bge.un IL_{0:X}",
						                i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                         ilCode[i + 1]));
						i += 4;
						break;
					case 0x42: // bgt.un XXXX
						sb.AppendFormat("bgt.un IL_{0:X}",
						                i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                         ilCode[i + 1]));
						i += 4;
						break;
					case 0x43: // ble.un XXXX
						sb.AppendFormat("ble.un IL_{0:X}",
						                i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                         ilCode[i + 1]));
						i += 4;
						break;
					case 0x44: // blt.un XXXX
						sb.AppendFormat("blt.un IL_{0:X}",
						                i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                         ilCode[i + 1]));
						i += 4;
						break;
					case 0x45: // switch NNNN NNNN*XXXX
						sb.AppendFormat("switch (0x{0:X}{1:X}{2:X}{3:X})", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						k = (ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						    (ilCode[i + 1] << 0);
						i += 4;
						for (j = 0; j < k; j++) {
							sb.AppendFormat(" <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
							                ilCode[i + 1]);
							i += 4;
						}

						break;
					case 0x46:
						sb.Append("ldind.i1");
						break;
					case 0x47:
						sb.Append("ldind.u1");
						break;
					case 0x48:
						sb.Append("ldind.i2");
						break;
					case 0x49:
						sb.Append("ldind.u2");
						break;
					case 0x4a:
						sb.Append("ldind.i4");
						break;
					case 0x4b:
						sb.Append("ldind.u4");
						break;
					case 0x4c:
						sb.Append("ldind.i8");
						break;
					case 0x4d:
						sb.Append("ldind.u8");
						break;
					case 0x4e:
						sb.Append("ldind.r4");
						break;
					case 0x4f:
						sb.Append("ldind.r8");
						break;
					case 0x50:
						sb.Append("ldind.ref");
						break;
					case 0x51:
						sb.Append("stind.ref");
						break;
					case 0x52:
						sb.Append("stind.i1");
						break;
					case 0x53:
						sb.Append("stind.i2");
						break;
					case 0x54:
						sb.Append("stind.i4");
						break;
					case 0x55:
						sb.Append("stind.i8");
						break;
					case 0x56:
						sb.Append("stind.r4");
						break;
					case 0x57:
						sb.Append("stind.r8");
						break;
					case 0x58:
						sb.Append("add");
						break;
					case 0x59:
						sb.Append("sub");
						break;
					case 0x5a:
						sb.Append("mul");
						break;
					case 0x5b:
						sb.Append("div");
						break;
					case 0x5c:
						sb.Append("div.un");
						break;
					case 0x5d:
						sb.Append("rem");
						break;
					case 0x5e:
						sb.Append("rem.un");
						break;
					case 0x5f:
						sb.Append("and");
						break;
					case 0x60:
						sb.Append("or");
						break;
					case 0x61:
						sb.Append("xor");
						break;
					case 0x62:
						sb.Append("shl");
						break;
					case 0x63:
						sb.Append("shr");
						break;
					case 0x64:
						sb.Append("shr.un");
						break;
					case 0x65:
						sb.Append("neg");
						break;
					case 0x66:
						sb.Append("not");
						break;
					case 0x67:
						sb.Append("conv.i1");
						break;
					case 0x68:
						sb.Append("conv.i2");
						break;
					case 0x69:
						sb.Append("conv.i4");
						break;
					case 0x6a:
						sb.Append("conv.i8");
						break;
					case 0x6b:
						sb.Append("conv.r4");
						break;
					case 0x6c:
						sb.Append("conv.r8");
						break;
					case 0x6d:
						sb.Append("conv.u4");
						break;
					case 0x6e:
						sb.Append("conv.u8");
						break;
					case 0x6f: // callvirt <T>
						sb.AppendFormat("callvirt <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3],
						                ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x70: // cpobj <T>
						sb.AppendFormat("cpobj <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x71: // ldobj <T>
						sb.AppendFormat("ldobj <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x72: // ldstr <T>
						sb.AppendFormat("ldstr <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x73: // newobj <T>
						sb.AppendFormat("newobj <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x74: // castclass <T>
						sb.AppendFormat("castclass <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3],
						                ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x75: // isinst <T>
						sb.AppendFormat("isinst <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x76:
						sb.Append("conv.r.un");
						break;
					case 0x79: // unbox <T>
						sb.AppendFormat("unbox <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x7a:
						sb.Append("throw");
						break;
					case 0x7b: // ldfld <T>
						sb.AppendFormat("ldfld <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x7c: // ldflda <T>
						sb.AppendFormat("ldflda <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x7d: // stfld <T>
						sb.AppendFormat("stfld <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x7e: // ldsfld <T>
						sb.AppendFormat("ldsfld <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x7f: // ldsflda <T>
						sb.AppendFormat("ldsflda <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x80: // stsfld <T>
						sb.AppendFormat("stsfld <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x81: // stobj <T>
						sb.AppendFormat("stobj <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x82:
						sb.Append("conv.ovf.i1.un");
						break;
					case 0x83:
						sb.Append("conv.ovf.i2.un");
						break;
					case 0x84:
						sb.Append("conv.ovf.i4.un");
						break;
					case 0x85:
						sb.Append("conv.ovf.i8.un");
						break;
					case 0x86:
						sb.Append("conv.ovf.u1.un");
						break;
					case 0x87:
						sb.Append("conv.ovf.u2.un");
						break;
					case 0x88:
						sb.Append("conv.ovf.u4.un");
						break;
					case 0x89:
						sb.Append("conv.ovf.u8.un");
						break;
					case 0x8a:
						sb.Append("conv.ovf.i.un");
						break;
					case 0x8b:
						sb.Append("conv.ovf.u.un");
						break;
					case 0x8c: // box <T>
						sb.AppendFormat("box <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x8d: // newarr <T>
						sb.AppendFormat("newarr <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x8e:
						sb.Append("ldlen");
						break;
					case 0x8f:
						sb.AppendFormat("ldelema <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0x90:
						sb.Append("ldelem.i1");
						break;
					case 0x91:
						sb.Append("ldelem.u1");
						break;
					case 0x92:
						sb.Append("ldelem.i2");
						break;
					case 0x93:
						sb.Append("ldelem.u2");
						break;
					case 0x94:
						sb.Append("ldelem.i4");
						break;
					case 0x95:
						sb.Append("ldelem.u4");
						break;
					case 0x96:
						sb.Append("ldelem.i8");
						break;
					case 0x97:
						sb.Append("ldelem.i");
						break;
					case 0x98:
						sb.Append("ldelem.r4");
						break;
					case 0x99:
						sb.Append("ldelem.r8");
						break;
					case 0x9a:
						sb.Append("ldelem.ref");
						break;
					case 0x9b:
						sb.Append("stelem.i");
						break;
					case 0x9c:
						sb.Append("stelem.i1");
						break;
					case 0x9d:
						sb.Append("stelem.i2");
						break;
					case 0x9e:
						sb.Append("stelem.i4");
						break;
					case 0x9f:
						sb.Append("stelem.i8");
						break;
					case 0xa0:
						sb.Append("stelem.r4");
						break;
					case 0xa1:
						sb.Append("stelem.r8");
						break;
					case 0xa2:
						sb.Append("stelem.ref");
						break;
					case 0xa3:
						sb.AppendFormat("stelem <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0xa4:
						sb.AppendFormat("stelem <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0xa5:
						sb.AppendFormat("unbox.any <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3],
						                ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0xb3:
						sb.Append("conv.ovf.i1");
						break;
					case 0xb4:
						sb.Append("conv.ovf.u1");
						break;
					case 0xb5:
						sb.Append("conv.ovf.i2");
						break;
					case 0xb6:
						sb.Append("conv.ovf.u2");
						break;
					case 0xb7:
						sb.Append("conv.ovf.i4");
						break;
					case 0xb8:
						sb.Append("conv.ovf.u4");
						break;
					case 0xb9:
						sb.Append("conv.ovf.i8");
						break;
					case 0xba:
						sb.Append("conv.ovf.u8");
						break;
					case 0xc2: // refanyval <T>
						sb.AppendFormat("refanyval <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3],
						                ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0xc3:
						sb.Append("ckfinite");
						break;
					case 0xc6: // mkrefany <T>
						sb.AppendFormat("mkrefany <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3],
						                ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0xd0: // ldtoken <T>
						sb.AppendFormat("ldtoken <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0xd1:
						sb.Append("conv.u2");
						break;
					case 0xd2:
						sb.Append("conv.u1");
						break;
					case 0xd3:
						sb.Append("conv.i");
						break;
					case 0xd4:
						sb.Append("conv.ovf.i");
						break;
					case 0xd5:
						sb.Append("conv.ovf.u");
						break;
					case 0xd6:
						sb.Append("add.ovf");
						break;
					case 0xd7:
						sb.Append("add.ovf.un");
						break;
					case 0xd8:
						sb.Append("mul.ovf");
						break;
					case 0xd9:
						sb.Append("mul.ovf.un");
						break;
					case 0xda:
						sb.Append("sub.ovf");
						break;
					case 0xdb:
						sb.Append("sub.ovf.un");
						break;
					case 0xdc:
						sb.Append("endfinally");
						break;
					case 0xdd: // leave XXXX
						sb.AppendFormat("leave 0x{0:X}{1:X}{2:X}{3:X}", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						                ilCode[i + 1]);
						i += 4;
						break;
					case 0xde: // leave.s X
						sb.AppendFormat("leave 0x{0:X}", ilCode[i + 1]);
						i += 1;
						break;
					case 0xdf:
						sb.Append("stind.i");
						break;
					case 0xe0:
						sb.Append("conv.u");
						break;
					case 0xfe:
						i++;
						switch (ilCode[i]) {
							case 0x00:
								sb.Append("arglist");
								break;
							case 0x01:
								sb.Append("ceq");
								break;
							case 0x02:
								sb.Append("cgt");
								break;
							case 0x03:
								sb.Append("cgt.un");
								break;
							case 0x04:
								sb.Append("clt");
								break;
							case 0x05:
								sb.Append("clt.un");
								break;
							case 0x06: // ldftn <T>
								sb.AppendFormat("ldftn <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3],
								                ilCode[i + 2],
								                ilCode[i + 1]);
								i += 4;
								break;
							case 0x07: // ldvirtftn <T>
								sb.AppendFormat("ldvirtftn <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3],
								                ilCode[i + 2],
								                ilCode[i + 1]);
								i += 4;
								break;
							case 0x09: // ldarg XX
								sb.AppendFormat("ldarg 0x{0:X}{1:X}", ilCode[i + 2], ilCode[i + 1]);
								i += 2;
								break;
							case 0x0a: // ldarga XX
								sb.AppendFormat("ldarga 0x{0:X}{1:X}", ilCode[i + 2], ilCode[i + 1]);
								i += 2;
								break;
							case 0x0b: // starg XX
								sb.AppendFormat("starg 0x{0:X}{1:X}", ilCode[i + 2], ilCode[i + 1]);
								i += 2;
								break;
							case 0x0c: // ldloc XX
								sb.AppendFormat("ldloc 0x{0:X}{1:X}", ilCode[i + 2], ilCode[i + 1]);
								i += 2;
								break;
							case 0x0d: // ldloca XX
								sb.AppendFormat("ldloca 0x{0:X}{1:X}", ilCode[i + 2], ilCode[i + 1]);
								i += 2;
								break;
							case 0x0e: // stloc XX
								sb.AppendFormat("stloc 0x{0:X}{1:X}", ilCode[i + 2], ilCode[i + 1]);
								i += 2;
								break;
							case 0x0f:
								sb.Append("localloc");
								break;
							case 0x11:
								sb.Append("endfilter");
								break;
							case 0x12: // unaligned X
								sb.AppendFormat("unaligned. 0x{0:X}", ilCode[i + 1]);
								i += 1;
								break;
							case 0x13:
								sb.Append("volatile.");
								break;
							case 0x14:
								sb.Append("tail.");
								break;
							case 0x15: // initobj <T>
								sb.AppendFormat("initobj <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3],
								                ilCode[i + 2],
								                ilCode[i + 1]);
								i += 4;
								break;
							case 0x16: // incomplete?
								sb.AppendFormat("constrained. <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3],
								                ilCode[i + 2],
								                ilCode[i + 1]);
								i += 4;
								break;
							case 0x17:
								sb.Append("cpblk");
								break;
							case 0x18:
								sb.Append("initblk");
								break;
							case 0x19:
								sb.Append("no.");
								break; // incomplete?
							case 0x1a:
								sb.Append("rethrow");
								break;
							case 0x1c: // sizeof <T>
								sb.AppendFormat("sizeof <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3],
								                ilCode[i + 2],
								                ilCode[i + 1]);
								i += 4;
								break;
							case 0x1d:
								sb.Append("refanytype");
								break;
							default:
								sb.AppendFormat("unknown ilCode 0xfe{0:X} at offset {1} in MethodGen::PrettyPrint",
								                ilCode[i], i);
								break;
						}

						break;
					default:
						sb.AppendFormat("unknown ilCode 0x{0:X} at offset {1}", ilCode[i], i);
						break;
				}

				sb.AppendLine();
			}

			return sb.ToString();
		}*/
	}
}