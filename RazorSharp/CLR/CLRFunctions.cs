#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using RazorCommon;
using RazorSharp.CLR.Structures;
using RazorSharp.Memory;
using RazorSharp.Utilities.Exceptions;

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
	public static unsafe class CLRFunctions
	{
		internal static readonly Dictionary<string, Delegate> Functions;
		private static readonly  SigScanner                   Scanner;
		internal const           string                       ClrDll = "clr.dll";

		internal static readonly Dictionary<string, byte[]> FunctionMap;

		static CLRFunctions()
		{
			Scanner = new SigScanner();
			Scanner.SelectModule(ClrDll);
			Functions   = new Dictionary<string, Delegate>();
			FunctionMap = new Dictionary<string, byte[]>();

			MethodDescFunctions.AddFunctions();
			FieldDescFunctions.AddFunctions();
			GCFunctions.AddFunctions();


//			SignatureCall.TranspileIndependent(typeof(CLRFunctions));
		}

//			IntPtr thread = Kernel32.OpenThread(0x4, false, Kernel32.GetCurrentThreadId());
//			Console.WriteLine(CLRFunctions.ThreadFunctions.GetStackGuarantee(thread));

		internal static void AddFunction<TDelegate>(string name, string signature) where TDelegate : Delegate
		{
			if (!Functions.ContainsKey(name)) {
				Functions.Add(name, Scanner.GetDelegate<TDelegate>(signature));
			}
		}


		internal static class GCFunctions
		{

			internal static void AddFunctions()
			{
				FunctionMap.Add("IsHeapPointer", Functions[0]);
				FunctionMap.Add("IsEphemeral", Functions[1]);
				FunctionMap.Add("IsGCInProgress", Functions[2]);
				FunctionMap.Add("get_GCCount", Functions[3]);
			}

			private static readonly byte[][] Functions =
			{
				/* IsHeapPointer */
				new byte[]
					{ 0x48, 0x83, 0xEC, 0x28, 0x48, 0x3B, 0x15, 0x2D, 0x4F, 0x3B, 0x00, 0x48, 0x8B, 0xC2, 0x73, 0x20 },

				/* IsEphemeral */
				new byte[]{ 0x48, 0x3B, 0x15, 0x09, 0xA1, 0x81, 0x00, 0x72, 0x0F, 0x48, 0x3B, 0x15, 0xF8, 0xA0, 0x81, 0x00, 0x73, 0x06, 0xB8, 0x01, 0x00, 0x00, 0x00, 0xC3 },

				/* IsGCInProgress */
				new byte[]{ 0x48, 0x89, 0x5C, 0x24, 0x08, 0x48, 0x89, 0x74, 0x24, 0x10, 0x57, 0x48, 0x83, 0xEC, 0x20, 0x48, 0x8B, 0x3D, 0xDE, 0xF3, 0x93, 0x00, 0x33, 0xC0 },

				/* GetGCCount */
				new byte[]{ 0x48, 0x8B, 0x05, 0x59, 0xF5, 0x82, 0x00, 0x48, 0x89, 0x44, 0x24, 0x10, 0x48, 0x8B, 0x44, 0x24, 0x10, 0xC3 },

			};



			private const string AllocSignature =
				"56 57 41 54 41 56 41 57 48 83 EC 30 48 C7 44 24 20 FE FF FF FF 48 89 5C 24 60 48 89 6C 24 70 45 8B E0 4C 8B F2 E9 00 01 00 00 33 F6 E9 E3 00 00 00";

			// size_t size, DWORD flags
			//struct Object *WKS::GCHeap::Alloc(WKS::GCHeap *__hidden this, unsigned __int64, unsigned int)
			internal delegate void* AllocDelegate(void* __this, ulong size, uint flags);

			internal static readonly AllocDelegate Alloc;



		}


		internal static class ObjectFunctions
		{
			private const string AllocateObjectSignature =
				"48 89 5C 24 10 48 89 6C 24 20 56 57 41 54 41 56 41 57 48 81 EC 80 00 00 00";

			internal delegate void* AllocateObjectDelegate(MethodTable* mt, bool fHandleCom = true);

			internal static readonly AllocateObjectDelegate AllocateObject;

			private const string GetSizeSignature =
				"4C 8B 01 49 83 E0 FC 41 F7 00 00 00 00 80 41 8B 40 04 74 0E 8B 51 08 41 0F B7 08 48 0F AF D1";

			internal delegate ulong GetSizeDelegate(void* __this);

			internal static readonly GetSizeDelegate GetSize;

			static ObjectFunctions()
			{
				AddFunction<AllocateObjectDelegate>("AllocateObject", AllocateObjectSignature);
				AllocateObject = (AllocateObjectDelegate) Functions["AllocateObject"];

				AddFunction<GetSizeDelegate>("GetSize", GetSizeSignature);
				GetSize = (GetSizeDelegate) Functions["GetSize"];
			}

		}


		internal static class FieldDescFunctions
		{
			internal static void AddFunctions()
			{
				FunctionMap.Add("GetModule", Functions[0]);
				FunctionMap.Add("get_LoadSize", Functions[1]);
				FunctionMap.Add("GetStubFieldInfo", Functions[2]);
				FunctionMap.Add("get_MethodTableOfEnclosingClass", Functions[3]);
			}

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

				/* 2 GetStubFieldInfo */
				new byte[]
				{
					0x48, 0x89, 0x5C, 0x24, 0x10, 0x57, 0x48, 0x83, 0xEC, 0x60, 0x48, 0x8B, 0x05, 0x07, 0x0F, 0x84,
					0x00, 0x33, 0xDB, 0x48, 0x8B, 0xF9, 0x48, 0x8B, 0x80, 0x78, 0x03, 0x00, 0x00, 0x48, 0x85, 0xC0,
					0x0F, 0x84, 0x72, 0x4C, 0x08, 0x00
				},

				/* 3 MethodTableOfEnclosingClass */
				new byte[]
				{
					0x53, 0x48, 0x83, 0xEC, 0x20, 0x83, 0x3D, 0x30, 0x1F, 0x92, 0x00, 0x00, 0x48, 0x8B, 0xD9, 0x0F,
					0x85, 0x33, 0x64, 0x2D, 0x00, 0x48, 0x8B, 0x03, 0x48, 0x03, 0xC3
				},
			};
		}

		internal static class MethodDescFunctions
		{


			internal static void AddFunctions()
			{
				FunctionMap.Add("get_IsCtor", Functions[0]);
				FunctionMap.Add("get_MemberDef", Functions[1]);
				FunctionMap.Add("get_IsPointingToNativeCode", Functions[2]);
				FunctionMap.Add("get_SizeOf", Functions[3]);
				FunctionMap.Add("Reset", Functions[4]);
				FunctionMap.Add("get_MethodTable", Functions[5]);
			}

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


		internal static class StringFunctions
		{
			private const string NewStringSignature =
				"48 8B C4 55 57 41 56 48 8D A8 78 FE FF FF 48 81 EC 70 02 00 00 48 C7 44 24 30 FE FF FF FF 48 89 58 10 48 89 70 18 48 8B 05 A3 69 87 00";

			private delegate void* NewStringDelegate(byte* charConstPtr);

			private static readonly NewStringDelegate NewStringInternal;

			static StringFunctions()
			{
				AddFunction<NewStringDelegate>("StringObject::NewString", NewStringSignature);

				NewStringInternal = (NewStringDelegate) Functions["StringObject::NewString"];
			}

			internal static string NewString(byte* charConstPtr)
			{
				void* str = NewStringInternal(charConstPtr);
				return CSUnsafe.AsRef<string>(&str);
			}
		}


		[CLRSigcall("48 83 EC 28 4C 8B C1 F6 C1 02 75 2E 48 8B 41 20 48 8B 50 08 F6 C2 01 74 09 48 8B 42 FF")]
		public static Type JIT_GetRuntimeType(void* __struct)
		{
			//return Memory.Memory.Read<Type>((IntPtr)LowLevelFunctions.JIT_GetRuntimeType(__struct));
			throw new NotTranspiledException();
		}
	}

}