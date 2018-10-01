#region

using System;
using System.Reflection;
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
	///     Some CLR functions are too complex to replicate in C# so we'll use <see cref="SigcallAttribute" />
	///     to execute them.
	///     <remarks>
	///         All functions are WKS, not SVR
	///     </remarks>
	/// </summary>
	internal static unsafe class ClrFunctions
	{
		/// <summary>
		///     <c>clr.dll</c>
		/// </summary>
		internal const string ClrDll = "clr.dll";

		private static bool s_bFunctionsCached = false;

		internal static void AddAll()
		{
			if (!s_bFunctionsCached) {
				MethodDescFunctions.Init();
				FieldDescFunctions.Init();
				GCFunctions.Init();
				JITFunctions.Init();

				s_bFunctionsCached = true;
			}
		}

		static ClrFunctions()
		{
			s_setStableEntryPointInterlocked =
				SigScanner.QuickScanDelegate<SetStableEntryPointInterlockedDelegate>(ClrDll,
					s_rgStableEntryPointInterlockedSignature);
			AddAll();
			SignatureCall.DynamicBind(typeof(ClrFunctions));
		}


		#region Functions

		private static class GCFunctions
		{
			private static readonly Cache<GCHeap> s_gcHeapCache;

			internal static void Init()
			{
				SignatureCall.Cache(s_gcHeapCache);
			}

			static GCFunctions()
			{
				s_gcHeapCache = new Cache<GCHeap>(Functions);
				s_gcHeapCache.AddCache("IsHeapPointer", false, FN_ISHEAPPOINTER_OFFSET);
				s_gcHeapCache.AddCache("IsEphemeral", false, FN_ISEPHEMERAL_OFFSET);
				s_gcHeapCache.AddCache("IsGCInProgress", false, FN_ISGCINPROGRESS_OFFSET);
				s_gcHeapCache.AddCache("GCCount", true, FN_GETGCCOUNT_OFFSET);
			}


			#region Offsets and signatures

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

			#endregion

		}

		private static class JITFunctions
		{
			internal static void Init()
			{
				SignatureCall.CacheFunction(typeof(ClrFunctions), "JIT_GetRuntimeType", Functions[0],
					FN_JITGETRUNTIMETYPE_OFFSET);
			}

			#region Offsets and signatures

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

			#endregion

		}

		private static class FieldDescFunctions
		{
			private static readonly Cache<FieldDesc> s_fieldDescCache;

			static FieldDescFunctions()
			{
				s_fieldDescCache = new Cache<FieldDesc>(Functions);
				s_fieldDescCache.AddCache("GetModule", false, FN_GETMODULE_OFFSET);
				s_fieldDescCache.AddCache("LoadSize", true, FN_GETLOADSIZE_OFFSET);
				s_fieldDescCache.AddCache("EnclosingMethodTable", true, FN_GETMETHODTABLE_OFFSET);
			}

			internal static void Init()
			{
				SignatureCall.Cache(s_fieldDescCache);
			}

			#region Offsets and signatures

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

			#endregion

		}

		private static class MethodDescFunctions
		{
			private static readonly Cache<MethodDesc> s_methodDescCache;

			static MethodDescFunctions()
			{
				s_methodDescCache = new Cache<MethodDesc>(Functions);
				s_methodDescCache.AddCache("IsConstructor", true, FN_GETISCTOR_OFFSET);
				s_methodDescCache.AddCache("Token", true, FN_GETTOKEN_OFFSET);
				s_methodDescCache.AddCache("IsPointingToNativeCode", true, FN_GETISPOINTINGTONATIVECODE_OFFSET);
				s_methodDescCache.AddCache("SizeOf", true, FN_GETSIZEOF_OFFSET);
				s_methodDescCache.AddCache("Reset", false, FN_RESET_OFFSET);
				s_methodDescCache.AddCache("EnclosingMethodTable", true, FN_GETMETHODTABLE_OFFSET);
				s_methodDescCache.AddCache("NativeCode", true, FN_GETNATIVECODE_OFFSET);
				s_methodDescCache.AddCache("PreImplementedCode", true, FN_GETPREIMPLEMENTEDCODE_OFFSET);
				s_methodDescCache.AddCache("GetILHeader", false, FN_GETILHEADER_OFFSET);
				s_methodDescCache.AddCache("RVA", true, FN_GETRVA_OFFSET);
				s_methodDescCache.AddCache("SetStableEntryPointInterlocked", false, FN_SETSTABLEENTRYPOINT_OFFSET);
			}


			internal static void Init()
			{
				SignatureCall.Cache(s_methodDescCache);
			}

			#region Offsets and signatures

			private const long FN_GETISCTOR_OFFSET                 = 0xAF920;
			private const long FN_GETTOKEN_OFFSET                  = 0x12810;
			private const long FN_GETISPOINTINGTONATIVECODE_OFFSET = 0x1A6CC4;
			private const long FN_GETSIZEOF_OFFSET                 = 0x390E0;
			private const long FN_RESET_OFFSET                     = 0x424714;
			private const long FN_GETMETHODTABLE_OFFSET            = 0xA260;
			private const long FN_GETNATIVECODE_OFFSET             = 0x12280;
			private const long FN_GETPREIMPLEMENTEDCODE_OFFSET     = 0x5A92C;
			private const long FN_GETILHEADER_OFFSET               = 0x19BBB4;
			private const long FN_GETRVA_OFFSET                    = 0x1B960;
			private const long FN_SETSTABLEENTRYPOINT_OFFSET       = 0x119F98;

			private static readonly byte[][] Functions =
			{
				/* 0 IsConstructor */
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
				},

				/* 6 NativeCode */
				new byte[]
				{
					0x48, 0x89, 0x5C, 0x24, 0x08, 0x57, 0x48, 0x83, 0xEC, 0x20, 0x8B, 0x05, 0xC0, 0x0E, 0x93, 0x00
				},

				/* 7 PreImplementedCode */
				new byte[]
				{
					0x48, 0x89, 0x5C, 0x24, 0x08, 0x57, 0x48, 0x83, 0xEC, 0x20, 0x48, 0x8B, 0xF9, 0xE8, 0x42, 0x79,
					0xFB, 0xFF, 0x48, 0x8B, 0xD8
				},

				/* 8 GetILHeader */
				new byte[]
				{
					0x48, 0x89, 0x5C, 0x24, 0x08, 0x48, 0x89, 0x74, 0x24, 0x10, 0x57, 0x48, 0x83, 0xEC, 0x20, 0x8B,
					0xDA, 0x48, 0x8B, 0xF9, 0xE8, 0xAB, 0x3D, 0xE7, 0xFF, 0x48, 0x8B, 0xCF
				},

				/* RVA */
				new byte[]
				{
					0x48, 0x89, 0x5C, 0x24, 0x08, 0x55, 0x56, 0x57, 0x48, 0x83, 0xEC, 0x20, 0x48, 0x8B, 0xF9, 0xE8,
					0xCC, 0xFF, 0xFF, 0xFF, 0x33, 0xED
				},

				s_rgStableEntryPointInterlockedSignature
			};

			#endregion

		}

		#endregion


		#region SetStableEntryPoint

		/// <summary>
		///     We implement <see cref="SetStableEntryPointInterlockedDelegate" /> as a <see cref="Delegate" /> initially because
		///     <see cref="MethodDesc.SetStableEntryPointInterlocked" /> has not been bound yet, and in order to bind it
		///     we have to use this function.
		/// </summary>
		/// <param name="__this"><c>this</c> pointer of a <see cref="MethodDesc" /></param>
		/// <param name="pCode">Entry point</param>
		private delegate long SetStableEntryPointInterlockedDelegate(MethodDesc* __this, ulong pCode);

		private static readonly SetStableEntryPointInterlockedDelegate s_setStableEntryPointInterlocked;

		private static readonly byte[] s_rgStableEntryPointInterlockedSignature =
		{
			0x48, 0x89, 0x5C, 0x24, 0x10, 0x48, 0x89, 0x74, 0x24, 0x18, 0x57, 0x48, 0x83, 0xEC, 0x20, 0x48, 0x8B, 0xFA,
			0x48, 0x8B, 0xF1, 0xE8, 0xEE, 0x6A, 0xF0, 0xFF
		};

		/// <summary>
		///     <remarks>
		///         Equal to <see cref="MethodDesc.SetStableEntryPoint" />, but this is implemented via a <see cref="Delegate" />
		///     </remarks>
		/// </summary>
		/// <param name="mi"></param>
		/// <param name="pCode"></param>
		internal static void SetStableEntryPoint(MethodInfo mi, IntPtr pCode)
		{
			MethodDesc* pMd = (MethodDesc*) mi.MethodHandle.Value;
			s_setStableEntryPointInterlocked(pMd, (ulong) pCode);
		}

		#endregion

		/// <summary>
		///     Returns the corresponding <see cref="Type" /> for a <see cref="MethodTable" /> pointer.
		/// </summary>
		/// <param name="__struct"><see cref="MethodTable" /> pointer</param>
		/// <returns></returns>
		/// <exception cref="SigcallException">Method has not been bound</exception>
		[ClrSigcall]
		internal static Type JIT_GetRuntimeType(void* __struct)
		{
			throw new SigcallException();
		}
	}

}