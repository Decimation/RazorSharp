#region

using System;
using System.Reflection;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures.ILMethods;
using RazorSharp.Memory.Pointers;

#endregion

// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming

namespace RazorSharp
{
	internal static unsafe class ILDump
	{
		internal static void DumpILToConsole(MethodInfo mi)
		{
			Pointer<ILMethod> il = mi.GetMethodDesc().Reference.GetILHeader();
			DumpILToConsole(il.Reference.Code.ToPointer<byte>(), il.Reference.CodeSize);
		}

		internal static void DumpILToConsole(byte[] il)
		{
			fixed (byte* b = il) {
				DumpILToConsole(b, il.Length);
			}
		}

		/// <summary>
		///     https://github.com/GeorgePlotnikov/ClrAnalyzer/blob/master/Win32Native/ildump.h
		/// </summary>
		internal static void DumpILToConsole(byte* ilCode, int len)
		{
			int i, j, k;
			for (i = 0; i < len; i++) {
				Console.Write("IL_{0:X}: ", i);
				switch (ilCode[i]) {
					case 0x00:
						Console.Write("nop");
						break;
					case 0x01:
						Console.Write("break");
						break;
					case 0x02:
						Console.Write("ldarg.0");
						break;
					case 0x03:
						Console.Write("ldarg.1");
						break;
					case 0x04:
						Console.Write("ldarg.2");
						break;
					case 0x05:
						Console.Write("ldarg.3");
						break;
					case 0x06:
						Console.Write("ldloc.0");
						break;
					case 0x07:
						Console.Write("ldloc.1");
						break;
					case 0x08:
						Console.Write("ldloc.2");
						break;
					case 0x09:
						Console.Write("ldloc.3");
						break;
					case 0x0a:
						Console.Write("stloc.0");
						break;
					case 0x0b:
						Console.Write("stloc.1");
						break;
					case 0x0c:
						Console.Write("stloc.2");
						break;
					case 0x0d:
						Console.Write("stloc.3");
						break;
					case 0x0e: // ldarg.s X
						Console.Write("ldarg.s 0x{0:X}", ilCode[i + 1]);
						i += 1;
						break;
					case 0x0f: // ldarga.s X
						Console.Write("ldarga.s 0x{0:X}", ilCode[i + 1]);
						i += 1;
						break;
					case 0x10: // starg.s X
						Console.Write("starg.s 0x{0:X}", ilCode[i + 1]);
						i += 1;
						break;
					case 0x11: // ldloc.s X
						Console.Write("ldloc.s 0x{0:X}", ilCode[i + 1]);
						i += 1;
						break;
					case 0x12: // ldloca.s X
						Console.Write("ldloca.s 0x{0:X}", ilCode[i + 1]);
						i += 1;
						break;
					case 0x13: // stloc.s X
						Console.Write("stloc.s 0x{0:X}", ilCode[i + 1]);
						i += 1;
						break;
					case 0x14:
						Console.Write("ldnull");
						break;
					case 0x15:
						Console.Write("ldc.i4.m1");
						break;
					case 0x16:
						Console.Write("ldc.i4.0");
						break;
					case 0x17:
						Console.Write("ldc.i4.1");
						break;
					case 0x18:
						Console.Write("ldc.i4.2");
						break;
					case 0x19:
						Console.Write("ldc.i4.3");
						break;
					case 0x1a:
						Console.Write("ldc.i4.4");
						break;
					case 0x1b:
						Console.Write("ldc.i4.5");
						break;
					case 0x1c:
						Console.Write("ldc.i4.6");
						break;
					case 0x1d:
						Console.Write("ldc.i4.7");
						break;
					case 0x1e:
						Console.Write("ldc.i4.8");
						break;
					case 0x1f: // ldc.i4.s X
						Console.Write("ldc.i4.s 0x{0:X}", ilCode[i + 1]);
						i += 1;
						break;
					case 0x20: // ldc.i4 XXXX
						Console.Write("ldc.i4 0x{0:X}{1:X}{2:X}{3:X}", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x21: // ldc.i8 XXXXXXXX
						Console.Write("ldc.i8 0x{0:X}{1:X}{2:X}{3:X}{4:X}{5:X}{6:X}{7:X}", ilCode[i + 8], ilCode[i + 7],
						              ilCode[i + 6],
						              ilCode[i + 5], ilCode[i + 4], ilCode[i + 3], ilCode[i + 2], ilCode[i + 1]);
						i += 8;
						break;
					case 0x22: // ldc.r4 XXXX
						Console.Write("ldc.r4 float32(0x{0:X}{1:X}{2:X}{3:X})", ilCode[i + 4], ilCode[i + 3],
						              ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x23: // ldc.r8 XXXXXXXX
						Console.Write("ldc.r8 float64(0x{0:X}{1:X}{2:X}{3:X}{4:X}{5:X}{6:X}{7:X})", ilCode[i + 8],
						              ilCode[i + 7],
						              ilCode[i + 6], ilCode[i + 5], ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 8;
						break;
					case 0x25:
						Console.Write("dup");
						break;
					case 0x26:
						Console.Write("pop");
						break;
					case 0x27: // JMP <T>
						Console.Write("jmp <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x28: // call <T>
						Console.Write("call <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x29: // calli <T>
						Console.Write("calli <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x2a:
						Console.Write("ret");
						break;
					case 0x2b: // br.s X
						Console.Write("br.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x2c: // brfalse.s X
						Console.Write("brfalse.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x2d: // brtrue.s X
						Console.Write("brtrue.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x2e: // beq.s X
						Console.Write("beq.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x2f: // bgt.s X
						Console.Write("bgt.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x30: // bgt.s X
						Console.Write("bgt.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x31: // ble.s X
						Console.Write("ble.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x32: // blt.s X
						Console.Write("blt.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x33: // bne.un.s X
						Console.Write("bne.un.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x34: // bge.un.s X
						Console.Write("bge.un.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x35: // bgt.un.s X
						Console.Write("bgt.un.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x36: // ble.un.s X
						Console.Write("ble.un.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x37: // blt.un.s X
						Console.Write("blt.un.s IL_{0:X}", i + 2 + ilCode[i + 1]);

						i += 1;
						break;
					case 0x38: // br XXXX
						Console.Write("br IL_{0:X}",
						              i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                       ilCode[i + 1]));
						i += 4;
						break;
					case 0x39: // brfalse XXXX
						Console.Write("brfalse IL_{0:X}",
						              i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                       ilCode[i + 1]));
						i += 4;
						break;
					case 0x3a: // brtrue XXXX
						Console.Write("brtrue IL_{0:X}",
						              i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                       ilCode[i + 1]));
						i += 4;
						break;
					case 0x3b: // beq XXXX
						Console.Write("beq IL_{0:X}",
						              i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                       ilCode[i + 1]));
						i += 4;
						break;
					case 0x3c: // bgt XXXX
						Console.Write("bgt IL_{0:X}",
						              i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                       ilCode[i + 1]));
						i += 4;
						break;
					case 0x3d: // bgt XXXX
						Console.Write("bgt IL_{0:X}",
						              i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                       ilCode[i + 1]));
						i += 4;
						break;
					case 0x3e: // ble XXXX
						Console.Write("ble IL_{0:X}",
						              i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                       ilCode[i + 1]));
						i += 4;
						break;
					case 0x3f: // blt XXXX
						Console.Write("blt IL_{0:X}",
						              i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                       ilCode[i + 1]));
						i += 4;
						break;
					case 0x40: // bne.un XXXX
						Console.Write("bne.un IL_{0:X}",
						              i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                       ilCode[i + 1]));
						i += 4;
						break;
					case 0x41: // bge.un XXXX
						Console.Write("bge.un IL_{0:X}",
						              i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                       ilCode[i + 1]));
						i += 4;
						break;
					case 0x42: // bgt.un XXXX
						Console.Write("bgt.un IL_{0:X}",
						              i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                       ilCode[i + 1]));
						i += 4;
						break;
					case 0x43: // ble.un XXXX
						Console.Write("ble.un IL_{0:X}",
						              i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                       ilCode[i + 1]));
						i += 4;
						break;
					case 0x44: // blt.un XXXX
						Console.Write("blt.un IL_{0:X}",
						              i + 5 + ((ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						                       ilCode[i + 1]));
						i += 4;
						break;
					case 0x45: // switch NNNN NNNN*XXXX
						Console.Write("switch (0x{0:X}{1:X}{2:X}{3:X})", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						k = (ilCode[i + 4] << 24) | (ilCode[i + 3] << 16) | (ilCode[i + 2] << 8) |
						    (ilCode[i + 1] << 0);
						i += 4;
						for (j = 0; j < k; j++) {
							Console.Write(" <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
							              ilCode[i + 1]);
							i += 4;
						}

						break;
					case 0x46:
						Console.Write("ldind.i1");
						break;
					case 0x47:
						Console.Write("ldind.u1");
						break;
					case 0x48:
						Console.Write("ldind.i2");
						break;
					case 0x49:
						Console.Write("ldind.u2");
						break;
					case 0x4a:
						Console.Write("ldind.i4");
						break;
					case 0x4b:
						Console.Write("ldind.u4");
						break;
					case 0x4c:
						Console.Write("ldind.i8");
						break;
					case 0x4d:
						Console.Write("ldind.u8");
						break;
					case 0x4e:
						Console.Write("ldind.r4");
						break;
					case 0x4f:
						Console.Write("ldind.r8");
						break;
					case 0x50:
						Console.Write("ldind.ref");
						break;
					case 0x51:
						Console.Write("stind.ref");
						break;
					case 0x52:
						Console.Write("stind.i1");
						break;
					case 0x53:
						Console.Write("stind.i2");
						break;
					case 0x54:
						Console.Write("stind.i4");
						break;
					case 0x55:
						Console.Write("stind.i8");
						break;
					case 0x56:
						Console.Write("stind.r4");
						break;
					case 0x57:
						Console.Write("stind.r8");
						break;
					case 0x58:
						Console.Write("add");
						break;
					case 0x59:
						Console.Write("sub");
						break;
					case 0x5a:
						Console.Write("mul");
						break;
					case 0x5b:
						Console.Write("div");
						break;
					case 0x5c:
						Console.Write("div.un");
						break;
					case 0x5d:
						Console.Write("rem");
						break;
					case 0x5e:
						Console.Write("rem.un");
						break;
					case 0x5f:
						Console.Write("and");
						break;
					case 0x60:
						Console.Write("or");
						break;
					case 0x61:
						Console.Write("xor");
						break;
					case 0x62:
						Console.Write("shl");
						break;
					case 0x63:
						Console.Write("shr");
						break;
					case 0x64:
						Console.Write("shr.un");
						break;
					case 0x65:
						Console.Write("neg");
						break;
					case 0x66:
						Console.Write("not");
						break;
					case 0x67:
						Console.Write("conv.i1");
						break;
					case 0x68:
						Console.Write("conv.i2");
						break;
					case 0x69:
						Console.Write("conv.i4");
						break;
					case 0x6a:
						Console.Write("conv.i8");
						break;
					case 0x6b:
						Console.Write("conv.r4");
						break;
					case 0x6c:
						Console.Write("conv.r8");
						break;
					case 0x6d:
						Console.Write("conv.u4");
						break;
					case 0x6e:
						Console.Write("conv.u8");
						break;
					case 0x6f: // callvirt <T>
						Console.Write("callvirt <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x70: // cpobj <T>
						Console.Write("cpobj <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x71: // ldobj <T>
						Console.Write("ldobj <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x72: // ldstr <T>
						Console.Write("ldstr <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x73: // newobj <T>
						Console.Write("newobj <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x74: // castclass <T>
						Console.Write("castclass <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x75: // isinst <T>
						Console.Write("isinst <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x76:
						Console.Write("conv.r.un");
						break;
					case 0x79: // unbox <T>
						Console.Write("unbox <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x7a:
						Console.Write("throw");
						break;
					case 0x7b: // ldfld <T>
						Console.Write("ldfld <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x7c: // ldflda <T>
						Console.Write("ldflda <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x7d: // stfld <T>
						Console.Write("stfld <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x7e: // ldsfld <T>
						Console.Write("ldsfld <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x7f: // ldsflda <T>
						Console.Write("ldsflda <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x80: // stsfld <T>
						Console.Write("stsfld <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x81: // stobj <T>
						Console.Write("stobj <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x82:
						Console.Write("conv.ovf.i1.un");
						break;
					case 0x83:
						Console.Write("conv.ovf.i2.un");
						break;
					case 0x84:
						Console.Write("conv.ovf.i4.un");
						break;
					case 0x85:
						Console.Write("conv.ovf.i8.un");
						break;
					case 0x86:
						Console.Write("conv.ovf.u1.un");
						break;
					case 0x87:
						Console.Write("conv.ovf.u2.un");
						break;
					case 0x88:
						Console.Write("conv.ovf.u4.un");
						break;
					case 0x89:
						Console.Write("conv.ovf.u8.un");
						break;
					case 0x8a:
						Console.Write("conv.ovf.i.un");
						break;
					case 0x8b:
						Console.Write("conv.ovf.u.un");
						break;
					case 0x8c: // box <T>
						Console.Write("box <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x8d: // newarr <T>
						Console.Write("newarr <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x8e:
						Console.Write("ldlen");
						break;
					case 0x8f:
						Console.Write("ldelema <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0x90:
						Console.Write("ldelem.i1");
						break;
					case 0x91:
						Console.Write("ldelem.u1");
						break;
					case 0x92:
						Console.Write("ldelem.i2");
						break;
					case 0x93:
						Console.Write("ldelem.u2");
						break;
					case 0x94:
						Console.Write("ldelem.i4");
						break;
					case 0x95:
						Console.Write("ldelem.u4");
						break;
					case 0x96:
						Console.Write("ldelem.i8");
						break;
					case 0x97:
						Console.Write("ldelem.i");
						break;
					case 0x98:
						Console.Write("ldelem.r4");
						break;
					case 0x99:
						Console.Write("ldelem.r8");
						break;
					case 0x9a:
						Console.Write("ldelem.ref");
						break;
					case 0x9b:
						Console.Write("stelem.i");
						break;
					case 0x9c:
						Console.Write("stelem.i1");
						break;
					case 0x9d:
						Console.Write("stelem.i2");
						break;
					case 0x9e:
						Console.Write("stelem.i4");
						break;
					case 0x9f:
						Console.Write("stelem.i8");
						break;
					case 0xa0:
						Console.Write("stelem.r4");
						break;
					case 0xa1:
						Console.Write("stelem.r8");
						break;
					case 0xa2:
						Console.Write("stelem.ref");
						break;
					case 0xa3:
						Console.Write("stelem <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0xa4:
						Console.Write("stelem <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0xa5:
						Console.Write("unbox.any <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0xb3:
						Console.Write("conv.ovf.i1");
						break;
					case 0xb4:
						Console.Write("conv.ovf.u1");
						break;
					case 0xb5:
						Console.Write("conv.ovf.i2");
						break;
					case 0xb6:
						Console.Write("conv.ovf.u2");
						break;
					case 0xb7:
						Console.Write("conv.ovf.i4");
						break;
					case 0xb8:
						Console.Write("conv.ovf.u4");
						break;
					case 0xb9:
						Console.Write("conv.ovf.i8");
						break;
					case 0xba:
						Console.Write("conv.ovf.u8");
						break;
					case 0xc2: // refanyval <T>
						Console.Write("refanyval <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0xc3:
						Console.Write("ckfinite");
						break;
					case 0xc6: // mkrefany <T>
						Console.Write("mkrefany <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0xd0: // ldtoken <T>
						Console.Write("ldtoken <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0xd1:
						Console.Write("conv.u2");
						break;
					case 0xd2:
						Console.Write("conv.u1");
						break;
					case 0xd3:
						Console.Write("conv.i");
						break;
					case 0xd4:
						Console.Write("conv.ovf.i");
						break;
					case 0xd5:
						Console.Write("conv.ovf.u");
						break;
					case 0xd6:
						Console.Write("add.ovf");
						break;
					case 0xd7:
						Console.Write("add.ovf.un");
						break;
					case 0xd8:
						Console.Write("mul.ovf");
						break;
					case 0xd9:
						Console.Write("mul.ovf.un");
						break;
					case 0xda:
						Console.Write("sub.ovf");
						break;
					case 0xdb:
						Console.Write("sub.ovf.un");
						break;
					case 0xdc:
						Console.Write("endfinally");
						break;
					case 0xdd: // leave XXXX
						Console.Write("leave 0x{0:X}{1:X}{2:X}{3:X}", ilCode[i + 4], ilCode[i + 3], ilCode[i + 2],
						              ilCode[i + 1]);
						i += 4;
						break;
					case 0xde: // leave.s X
						Console.Write("leave 0x{0:X}", ilCode[i + 1]);
						i += 1;
						break;
					case 0xdf:
						Console.Write("stind.i");
						break;
					case 0xe0:
						Console.Write("conv.u");
						break;
					case 0xfe:
						i++;
						switch (ilCode[i]) {
							case 0x00:
								Console.Write("arglist");
								break;
							case 0x01:
								Console.Write("ceq");
								break;
							case 0x02:
								Console.Write("cgt");
								break;
							case 0x03:
								Console.Write("cgt.un");
								break;
							case 0x04:
								Console.Write("clt");
								break;
							case 0x05:
								Console.Write("clt.un");
								break;
							case 0x06: // ldftn <T>
								Console.Write("ldftn <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3],
								              ilCode[i + 2],
								              ilCode[i + 1]);
								i += 4;
								break;
							case 0x07: // ldvirtftn <T>
								Console.Write("ldvirtftn <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3],
								              ilCode[i + 2],
								              ilCode[i + 1]);
								i += 4;
								break;
							case 0x09: // ldarg XX
								Console.Write("ldarg 0x{0:X}{1:X}", ilCode[i + 2], ilCode[i + 1]);
								i += 2;
								break;
							case 0x0a: // ldarga XX
								Console.Write("ldarga 0x{0:X}{1:X}", ilCode[i + 2], ilCode[i + 1]);
								i += 2;
								break;
							case 0x0b: // starg XX
								Console.Write("starg 0x{0:X}{1:X}", ilCode[i + 2], ilCode[i + 1]);
								i += 2;
								break;
							case 0x0c: // ldloc XX
								Console.Write("ldloc 0x{0:X}{1:X}", ilCode[i + 2], ilCode[i + 1]);
								i += 2;
								break;
							case 0x0d: // ldloca XX
								Console.Write("ldloca 0x{0:X}{1:X}", ilCode[i + 2], ilCode[i + 1]);
								i += 2;
								break;
							case 0x0e: // stloc XX
								Console.Write("stloc 0x{0:X}{1:X}", ilCode[i + 2], ilCode[i + 1]);
								i += 2;
								break;
							case 0x0f:
								Console.Write("localloc");
								break;
							case 0x11:
								Console.Write("endfilter");
								break;
							case 0x12: // unaligned X
								Console.Write("unaligned. 0x{0:X}", ilCode[i + 1]);
								i += 1;
								break;
							case 0x13:
								Console.Write("volatile.");
								break;
							case 0x14:
								Console.Write("tail.");
								break;
							case 0x15: // initobj <T>
								Console.Write("initobj <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3],
								              ilCode[i + 2],
								              ilCode[i + 1]);
								i += 4;
								break;
							case 0x16: // incomplete?
								Console.Write("constrained. <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3],
								              ilCode[i + 2],
								              ilCode[i + 1]);
								i += 4;
								break;
							case 0x17:
								Console.Write("cpblk");
								break;
							case 0x18:
								Console.Write("initblk");
								break;
							case 0x19:
								Console.Write("no.");
								break; // incomplete?
							case 0x1a:
								Console.Write("rethrow");
								break;
							case 0x1c: // sizeof <T>
								Console.Write("sizeof <0x{0:X}{1:X}{2:X}{3:X}>", ilCode[i + 4], ilCode[i + 3],
								              ilCode[i + 2],
								              ilCode[i + 1]);
								i += 4;
								break;
							case 0x1d:
								Console.Write("refanytype");
								break;
							default:
								Console.Write("unknown ilCode 0xfe{0:X} at offset {1} in MethodGen::PrettyPrint",
								              ilCode[i], i);
								break;
						}

						break;
					default:
						Console.Write("unknown ilCode 0x{0:X} at offset {1}", ilCode[i], i);
						break;
				}

				Console.WriteLine();
			}
		}
	}
}