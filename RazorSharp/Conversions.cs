#region

using RazorSharp.Memory;
using RazorSharp.Pointers;
using System;
using RazorCommon.Diagnostics;
using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

#endregion

namespace RazorSharp
{
	// todo: WIP

	public static unsafe class Conversions
	{
		public enum ConversionType
		{
			LOW_LEVEL,
			LIGHT,
			AS,
			UNION
		}

		private delegate int UnionCastFunction(long value);

		private delegate void Jmp();

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TFrom">T1</typeparam>
		/// <typeparam name="TTo">T2</typeparam>
		/// <returns></returns>
		public static TTo UnionCast<TFrom, TTo>(TFrom value)
		{
			throw new NotImplementedException();
			
			Conditions.Assert(Unsafe.SizeOf<TFrom>() == 8);
			Conditions.Assert(Unsafe.SizeOf<TTo>() == 4);

			string[] asm =
			{
				"push 	 rbp",
				"mov 	 rbp, rsp",
				"movss 	 DWORD [rbp-20], xmm0",
				"mov 	 QWORD [rbp-8], 0",
				"movss   xmm0, DWORD [rbp-20]",
				"movss   DWORD [rbp-8], xmm0",
				"movsd   xmm0, QWORD [rbp-8]",
				"pop     rbp",
				"ret"
			};


			var fnCode = Mem.AllocCode(asm);


			


			Mem.FreeCode(fnCode);

			return default;
		}

		public static unsafe TTo[] ConvertArray<TTo>(byte[] mem)
		{
			fixed (byte* ptr = mem) {
				Pointer<TTo> memPtr = ptr;
				return memPtr.CopyOut(mem.Length / memPtr.ElementSize);
			}
		}

		public static TTo Convert<TFrom, TTo>(TFrom t, ConversionType c = ConversionType.LOW_LEVEL)
		{
			switch (c) {
				case ConversionType.LOW_LEVEL:
					return ProxyCast<TFrom, TTo>(t);
				case ConversionType.LIGHT:
					return (TTo) System.Convert.ChangeType(t, typeof(TTo));
				case ConversionType.AS:
				//return CSUnsafe.As<TFrom, TTo>(ref t);
				default:
					return default;
			}

			return default;
		}

		public static TTo Convert<TTo>(byte[] mem) where TTo : struct
		{
			Pointer<byte> alloc = Mem.AllocUnmanaged<byte>(mem.Length);
			alloc.WriteAll(mem);
			var read = alloc.ReadAny<TTo>();
			Mem.Free(alloc);
			return read;
		}

		public static TProxy ProxyCast<TOld, TProxy>(TOld value)
		{
			return Unsafe.AddressOf(ref value).ReadAs<TOld, TProxy>();
		}
	}
}