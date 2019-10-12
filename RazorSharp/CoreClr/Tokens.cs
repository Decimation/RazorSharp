using System;
using RazorSharp.CoreClr.Metadata.Enums;
using RazorSharp.Utilities.Security;

namespace RazorSharp.CoreClr
{
	internal static class Tokens
	{
		private const int RID_FROM_TOKEN = 0x00FFFFFF;

		private const uint TYPE_FROM_TOKEN = 0xFF000000;

		internal static int TokenFromRid(int rid, CorTokenType tktype) => rid | (int) tktype;

		internal static int RidFromToken(int tk) => tk & RID_FROM_TOKEN;

		internal static long TypeFromToken(int tk) => tk & TYPE_FROM_TOKEN;

		/// <summary>
		///     <para>Sources:</para>
		///     <list type="bullet">
		///         <item>
		///             <description>src/vm/siginfo.cpp: 63</description>
		///         </item>
		///     </list>
		///     <exception cref="Exception">If size is unknown</exception>
		/// </summary>
		internal static int SizeOfCorElementType(CorElementType t)
		{
			switch (t) {
				case CorElementType.Void:
					return default;

				case CorElementType.Boolean:
					return sizeof(bool);

				case CorElementType.Char:
					return sizeof(char);

				case CorElementType.I1:
					return sizeof(sbyte);
				case CorElementType.U1:
					return sizeof(byte);

				case CorElementType.I2:
					return sizeof(short);
				case CorElementType.U2:
					return sizeof(ushort);

				case CorElementType.I4:
					return sizeof(int);
				case CorElementType.U4:
					return sizeof(uint);

				case CorElementType.I8:
					return sizeof(long);
				case CorElementType.U8:
					return sizeof(ulong);

				case CorElementType.R4:
					return sizeof(float);
				case CorElementType.R8:
					return sizeof(double);

				case CorElementType.String:
				case CorElementType.Ptr:
				case CorElementType.ByRef:
				case CorElementType.Class:
				case CorElementType.Array:
				case CorElementType.I:
				case CorElementType.U:
				case CorElementType.FnPtr:
				case CorElementType.Object:
				case CorElementType.SzArray:
				case CorElementType.End:
					return IntPtr.Size;


				case CorElementType.ValueType:
				case CorElementType.Var:
				case CorElementType.GenericInst:
				case CorElementType.CModReqd:
				case CorElementType.CModOpt:
				case CorElementType.Internal:
				case CorElementType.MVar:
					return Constants.INVALID_VALUE;

				case CorElementType.TypedByRef:
					return IntPtr.Size * 2;

				case CorElementType.Max:
				case CorElementType.Modifier:
				case CorElementType.Sentinel:
				case CorElementType.Pinned:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(t), t, null);
			}

			throw Guard.ClrFail($"Size for CorElementType {t} is unknown");
		}
	}
}