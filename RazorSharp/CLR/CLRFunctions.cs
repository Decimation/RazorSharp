#region

using System;
using System.Runtime.CompilerServices;
using RazorSharp.CLR.Structures;
using RazorSharp.Memory;
using RazorSharp.Utilities.Exceptions;

// ReSharper disable IdentifierTypo

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.CLR
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion


	/// <summary>
	///     Some CLR functions are too complex to replicate in C# so we'll use sigscanning to execute them
	///     <remarks>
	///         All functions are WKS, not SVR
	///     </remarks>
	/// </summary>
	internal static unsafe class CLRFunctions
	{
		internal const string ClrDll = "clr.dll";

		private static bool s_bFunctionsCached = false;

		internal static void AddAll()
		{
			if (!s_bFunctionsCached) {
				MethodDescFunctions.AddFunctions();
				FieldDescFunctions.AddFunctions();
				GCFunctions.AddFunctions();
				JITFunctions.AddFunctions();

				s_bFunctionsCached = true;
			}
		}

		static CLRFunctions()
		{
			AddAll();
			SignatureCall.DynamicBind(typeof(CLRFunctions));
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Cache<T>(string fnName, byte[] rgSignature, long offsetGuess)
		{
			SignatureCall.CacheFunction<T>(fnName, rgSignature, offsetGuess);
		}

		private static class GCFunctions
		{
			internal static void AddFunctions()
			{
				Cache<GCHeap>("IsHeapPointer", Functions[0], FN_ISHEAPPOINTER_OFFSET);
				Cache<GCHeap>("IsEphemeral", Functions[1], FN_ISEPHEMERAL_OFFSET);
				Cache<GCHeap>("IsGCInProgress", Functions[2], FN_ISGCINPROGRESS_OFFSET);
				Cache<GCHeap>("get_GCCount", Functions[3], FN_GETGCCOUNT_OFFSET);
			}


			private const long FN_ISHEAPPOINTER_OFFSET  = 0x58E260;
			private const long FN_ISEPHEMERAL_OFFSET    = 0x129100;
			private const long FN_ISGCINPROGRESS_OFFSET = 0x3C3C;
			private const long FN_GETGCCOUNT_OFFSET     = 0x123C60;


			private static readonly byte[][] Functions =
			{
				/* 0 IsHeapPointer */
				new byte[]
				{
					0x48, 0x83, 0xEC, 0x28, 0x48, 0x3B, 0x15, 0x2D, 0x4F, 0x3B, 0x00, 0x48, 0x8B, 0xC2, 0x73, 0x20
				},

				/* 1 IsEphemeral */
				new byte[]
				{
					0x48, 0x3B, 0x15, 0x09, 0xA1, 0x81, 0x00, 0x72, 0x0F, 0x48, 0x3B, 0x15, 0xF8, 0xA0, 0x81, 0x00,
					0x73, 0x06, 0xB8, 0x01, 0x00, 0x00, 0x00, 0xC3
				},

				/* 2 IsGCInProgress */
				new byte[]
				{
					0x48, 0x89, 0x5C, 0x24, 0x08, 0x48, 0x89, 0x74, 0x24, 0x10, 0x57, 0x48, 0x83, 0xEC, 0x20, 0x48,
					0x8B, 0x3D, 0xDE, 0xF3, 0x93, 0x00, 0x33, 0xC0
				},

				/* 3 GetGCCount */
				new byte[]
				{
					0x48, 0x8B, 0x05, 0x59, 0xF5, 0x82, 0x00, 0x48, 0x89, 0x44, 0x24, 0x10, 0x48, 0x8B, 0x44, 0x24,
					0x10, 0xC3
				},
			};

		}

		private static class JITFunctions
		{
			internal static void AddFunctions()
			{
				SignatureCall.CacheFunction(typeof(CLRFunctions), "JIT_GetRuntimeType", Functions[0],
					FN_JITGETRUNTIMETYPE_OFFSET);
			}

			private const long FN_JITGETRUNTIMETYPE_OFFSET = 0x104D0;

			private static readonly byte[][] Functions =
			{
				/* 0 JIT_GetRuntimeType */
				new byte[]
				{
					0x48, 0x83, 0xEC, 0x28, 0x4C, 0x8B, 0xC1, 0xF6, 0xC1, 0x02, 0x75, 0x2E, 0x48, 0x8B, 0x41, 0x20,
					0x48, 0x8B, 0x50, 0x08, 0xF6, 0xC2, 0x01, 0x74, 0x09, 0x48, 0x8B, 0x42, 0xFF,
				},
			};
		}


		private static class FieldDescFunctions
		{
			internal static void AddFunctions()
			{
				Cache<FieldDesc>("GetModule", Functions[0], FN_GETMODULE_OFFSET);
				Cache<FieldDesc>("get_LoadSize", Functions[1], FN_GETLOADSIZE_OFFSET);
				Cache<FieldDesc>("get_EnclosingMethodTable", Functions[2], FN_GETMETHODTABLE_OFFSET);
			}

			private const long FN_GETMODULE_OFFSET      = 0x4109D4;
			private const long FN_GETLOADSIZE_OFFSET    = 0x102278;
			private const long FN_GETMETHODTABLE_OFFSET = 0x21214;

			private static readonly byte[][] Functions =
			{
				/* 0 GetModule */
				new byte[]
				{
					0x48, 0x83, 0xEC, 0x28, 0xE8, 0x37, 0x08, 0xC1, 0xFF, 0x48, 0x8B, 0xC8, 0x48, 0x83, 0xC4, 0x28,
					0xE9, 0x47, 0xEB, 0xBF, 0xFF, 0xCC, 0x90, 0x90
				},

				/* 1 LoadSize */
				new byte[]
				{
					0x48, 0x83, 0xEC, 0x28, 0x8B, 0x51, 0x0C, 0x48, 0x8D, 0x05, 0x4A, 0x25, 0x63, 0x00, 0xC1, 0xEA, 0x1B
				},

				/* 2 MethodTable */
				new byte[]
				{
					0x53, 0x48, 0x83, 0xEC, 0x20, 0x83, 0x3D, 0x30, 0x1F, 0x92, 0x00, 0x00, 0x48, 0x8B, 0xD9, 0x0F,
					0x85, 0x33, 0x64, 0x2D, 0x00, 0x48, 0x8B, 0x03, 0x48, 0x03, 0xC3
				},
			};
		}

		private static class MethodDescFunctions
		{
			internal static void AddFunctions()
			{
				Cache<MethodDesc>("get_IsConstructor", Functions[0], FN_GETISCTOR_OFFSET);
				Cache<MethodDesc>("get_Token", Functions[1], FN_GETTOKEN_OFFSET);
				Cache<MethodDesc>("get_IsPointingToNativeCode", Functions[2],
					FN_GETISPOINTINGTONATIVECODE_OFFSET);
				Cache<MethodDesc>("get_SizeOf", Functions[3], FN_GETSIZEOF_OFFSET);
				Cache<MethodDesc>("Reset", Functions[4], FN_RESET_OFFSET);
				Cache<MethodDesc>("get_EnclosingMethodTable", Functions[5],
					FN_GETMETHODTABLE_OFFSET);
			}

			private const long FN_GETISCTOR_OFFSET                 = 0xAF920;
			private const long FN_GETTOKEN_OFFSET                  = 0x12810;
			private const long FN_GETISPOINTINGTONATIVECODE_OFFSET = 0x1A6CC4;
			private const long FN_GETSIZEOF_OFFSET                 = 0x390E0;
			private const long FN_RESET_OFFSET                     = 0x424714;
			private const long FN_GETMETHODTABLE_OFFSET            = 0xA260;

			private static readonly byte[][] Functions =
			{
				/* 0 IsCtor */
				new byte[]
				{
					0x48, 0x89, 0x5C, 0x24, 0x08, 0x57, 0x48, 0x83, 0xEC, 0x20, 0x48, 0x8B, 0xF9, 0xE8, 0x4E, 0x32,
					0xF6, 0xFF, 0x33, 0xDB, 0x0F, 0xBA, 0xE0, 0x0C, 0x0F, 0x82, 0x39, 0xBA, 0x0F, 0x00
				},

				/* 1 MemberDef */
				new byte[]
				{
					0x53, 0x48, 0x83, 0xEC, 0x20, 0x83, 0x3D, 0x34, 0x09, 0x93, 0x00, 0x00, 0x48, 0x8B, 0xD9, 0x0F,
					0x85, 0x43, 0xC3, 0x2D, 0x00, 0x0F, 0xB6, 0x43, 0x02, 0x0F, 0xB7, 0x0B
				},

				/* 2 IsPointingToNativeCode */
				new byte[]
				{
					0x48, 0x89, 0x5C, 0x24, 0x08, 0x57, 0x48, 0x83, 0xEC, 0x20, 0x8A, 0x41, 0x03, 0x48, 0x8B, 0xF9,
					0xA8, 0x01, 0x0F, 0x85, 0xFC, 0xC5, 0xF6, 0xFF, 0x33, 0xC0, 0xEB, 0x01, 0xCC
				},

				/* 3 SizeOf */
				new byte[]
				{
					0x0F, 0xB7, 0x41, 0x06, 0x4C, 0x8D, 0x05, 0x45, 0x6D, 0x6F, 0x00, 0x8B, 0xD0, 0x83, 0xE2, 0x1F
				},

				/* 4 Reset */
				new byte[]
				{
					0x48, 0x89, 0x5C, 0x24, 0x08, 0x57, 0x48, 0x83, 0xEC, 0x20, 0xBA, 0x00, 0x18, 0x00, 0x00, 0x45,
					0x33, 0xC0, 0x48, 0x8B, 0xF9, 0xE8, 0xD2, 0x34, 0xD8, 0xFF, 0xBA, 0x00, 0x20, 0x00, 0x00, 0x45,
					0x33, 0xC0, 0x48, 0x8B, 0xCF
				},

				/* 5 MethodTable */
				new byte[]
				{
					0x53, 0x48, 0x83, 0xEC, 0x20, 0x83, 0x3D, 0xE4, 0x8E, 0x93, 0x00, 0x00, 0x48, 0x8B, 0xD9, 0x0F,
					0x85, 0xFC, 0x47, 0x2E, 0x00, 0x0F, 0xB6, 0x43, 0x02, 0xC1, 0xE0, 0x03, 0x48, 0x63, 0xD0, 0x48,
					0x2B, 0xDA, 0x48, 0x83, 0xEB, 0x18, 0x48, 0x8B, 0x03, 0x48, 0x03, 0xC3, 0xA8, 0x01, 0x0F, 0x85, 0xE7
				}
			};
		}


		[CLRSigcall]
		internal static Type JIT_GetRuntimeType(void* __struct)
		{
			throw new SigcallException();
		}
	}

}