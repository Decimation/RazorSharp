#region

using System;
using RazorSharp.CoreClr.Enums;
using RazorSharp.CoreClr.Structures;

#endregion

// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr
{
	internal static unsafe class Constants
	{
		/// <summary>
		///     De facto value representing an invalid value or a failure
		/// </summary>
		internal const int INVALID_VALUE = -1;


		/// <summary>
		///     <para>Minimum GC object heap size</para>
		///     <para>Sources:</para>
		///     <list type="bullet">
		///         <item>
		///             <description>/src/vm/object.h: 119</description>
		///         </item>
		///     </list>
		/// </summary>
		internal static readonly int MinObjectSize = 2 * IntPtr.Size + sizeof(ObjHeader);


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
					return 0;

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
					return INVALID_VALUE;

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

			throw new Exception($"Size for CorElementType {t} is unknown");
		}

		internal static bool IsPrimitive(CorElementType cet)
		{
			return cet >= CorElementType.Boolean && cet <= CorElementType.R8
			       || cet == CorElementType.I || cet == CorElementType.U
			       || cet == CorElementType.Ptr || cet == CorElementType.FnPtr;
		}

		private static CorElementType TypeToCorType(Type t)
		{
			switch (Type.GetTypeCode(t)) {
				case TypeCode.Empty:
				case TypeCode.DBNull:
					goto case default;
				case TypeCode.Boolean:
					return CorElementType.Boolean;
				case TypeCode.Char:
					return CorElementType.Char;
				case TypeCode.SByte:
					return CorElementType.I1;
				case TypeCode.Byte:
					return CorElementType.U1;
				case TypeCode.Int16:
					return CorElementType.I2;
				case TypeCode.UInt16:
					return CorElementType.U2;
				case TypeCode.Int32:
					return CorElementType.I4;
				case TypeCode.UInt32:
					return CorElementType.U4;
				case TypeCode.Int64:
					return CorElementType.I8;
				case TypeCode.UInt64:
					return CorElementType.U8;
				case TypeCode.Single:
					return CorElementType.R4;
				case TypeCode.Double:
					return CorElementType.R8;
				case TypeCode.DateTime:
				case TypeCode.Decimal:
					return CorElementType.ValueType;
				case TypeCode.Object:
				case TypeCode.String:
					return CorElementType.Class;
				default:
					throw new ArgumentOutOfRangeException(
						$"{t.Name} has not been mapped to {nameof(CorElementType)}.");
			}
		}

		internal static CorElementType TypeToCorType<T>()
		{
			return TypeToCorType(typeof(T));
		}

		internal static int RidFromToken(int tk)
		{
			// #define RidFromToken(tk) ((RID) ((tk) & 0x00ffffff))
			return tk & 0x00ffffff;
		}

		internal static long TypeFromToken(int tk)
		{
			// #define TypeFromToken(tk) ((ULONG32)((tk) & 0xff000000))
			return tk & 0xff000000;
		}

		internal static int TokenFromRid(int rid, CorTokenType tktype)
		{
			return rid | (int) tktype;
		}
	}
}