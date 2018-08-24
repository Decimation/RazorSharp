#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using RazorSharp.CLR.Structures;
using RazorSharp.Memory;

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
		private static readonly Dictionary<string, Delegate> Functions;
		private static readonly SigScanner                   Scanner;
		private const           string                       ClrDll = "clr.dll";

		static CLRFunctions()
		{
			Scanner = new SigScanner(Process.GetCurrentProcess());
			Scanner.SelectModule(ClrDll);
			Functions = new Dictionary<string, Delegate>();
		}

//			IntPtr thread = Kernel32.OpenThread(0x4, false, Kernel32.GetCurrentThreadId());
//			Console.WriteLine(CLRFunctions.ThreadFunctions.GetStackGuarantee(thread));

		private static void AddFunction<TDelegate>(string name, string signature) where TDelegate : Delegate
		{
			Functions.Add(name, Scanner.GetDelegate<TDelegate>(signature));
		}


		internal static class GCFunctions
		{

			#region IsHeapPointer

			private const string IsHeapPointerSignature = "48 83 EC 28 48 3B 15 2D 4F 3B 00 48 8B C2 73 20";

			private delegate int IsHeapPointerDelegate(void* __this, void* obj, int smallHeapOnly);

			private static readonly IsHeapPointerDelegate IsHeapPointerInternal;

			internal static bool IsHeapPointer(void* __this, void* obj, bool smallHeapOnly)
			{
				return IsHeapPointerInternal(__this, obj, smallHeapOnly ? 1 : 0) > 0;
			}

			#endregion

			#region IsEphemeral

			private const string IsEphemeralSignature =
				"48 3B 15 09 A1 81 00 72 0F 48 3B 15 F8 A0 81 00 73 06 B8 01 00 00 00 C3";

			private delegate long IsEphemeralDelegate(void* __this, void* obj);

			private static readonly IsEphemeralDelegate IsEphemeralInternal;

			internal static bool IsEphemeral(void* __this, void* obj)
			{
				return IsEphemeralInternal(__this, obj) > 0;
			}

			#endregion

			#region IsGCInProgress

			private const string IsGCInProgressSignature =
				"48 89 5C 24 08 48 89 74 24 10 57 48 83 EC 20 48 8B 3D DE F3 93 00 33 C0";

			private delegate long IsGCInProgressDelegate(int bConsiderGCStart = 0);

			private static readonly IsGCInProgressDelegate IsGCInProgressInternal;

			internal static bool IsGCInProgress(bool bConsiderGCStart = false)
			{
				return IsGCInProgressInternal(bConsiderGCStart ? 1 : 0) > 0;
			}

			#endregion

			#region GetGCCount

			private const string GetGCCountSignature = "48 8B 05 59 F5 82 00 48 89 44 24 10 48 8B 44 24 10 C3";

			internal delegate uint GetGCCountDelegate(void* __this);

			internal static readonly GetGCCountDelegate GetGCCountInternal;

			#endregion

			private const string AllocSignature =
				"56 57 41 54 41 56 41 57 48 83 EC 30 48 C7 44 24 20 FE FF FF FF 48 89 5C 24 60 48 89 6C 24 70 45 8B E0 4C 8B F2 E9 00 01 00 00 33 F6 E9 E3 00 00 00";

			// size_t size, DWORD flags
			//struct Object *WKS::GCHeap::Alloc(WKS::GCHeap *__hidden this, unsigned __int64, unsigned int)
			internal delegate void* AllocDelegate(void* __this, ulong size, uint flags);

			internal static readonly AllocDelegate Alloc;

			private const string GetContainingObjectSignature =
				"48 3B 15 D9 50 3C 00 48 8B C2 72 18 48 3B 05 95 CE 3B 00 73 0F 48 8B 15 34 59 3C 00 48 8B C8 E9 EC 94 B1 FF 33 C0 C3";

			internal delegate void* GetContainingObjectDelegate(void* __this, void* obj);

			internal static readonly GetContainingObjectDelegate GetContainingObject;


			static GCFunctions()
			{
				AddFunction<IsHeapPointerDelegate>("GCHeap::IsHeapPointer", IsHeapPointerSignature);
				IsHeapPointerInternal = (IsHeapPointerDelegate) Functions["GCHeap::IsHeapPointer"];

				AddFunction<IsEphemeralDelegate>("GCHeap::IsEphemeral", IsEphemeralSignature);
				IsEphemeralInternal = (IsEphemeralDelegate) Functions["GCHeap::IsEphemeral"];

				AddFunction<IsGCInProgressDelegate>("GCHeap::IsGCInProgress", IsGCInProgressSignature);
				IsGCInProgressInternal = (IsGCInProgressDelegate) Functions["GCHeap::IsGCInProgress"];

				AddFunction<GetGCCountDelegate>("GCHeap::GetGCCount", GetGCCountSignature);
				GetGCCountInternal = (GetGCCountDelegate) Functions["GCHeap::GetGCCount"];

				AddFunction<AllocDelegate>("GCHeap::Alloc", AllocSignature);
				Alloc = (AllocDelegate) Functions["GCHeap::Alloc"];

				AddFunction<GetContainingObjectDelegate>("GCHeap::GetContainingObject", GetContainingObjectSignature);
				GetContainingObject = (GetContainingObjectDelegate) Functions["GCHeap::GetContainingObject"];
			}
		}


		internal static class MethodDescFunctions
		{
			private const string SizeOfSignature =
				"0F B7 41 06 4C 8D 05 45 6D 6F 00 8B D0 83 E2 1F";

			internal delegate ulong SizeOfDelegate(MethodDesc* __this);

			internal static readonly SizeOfDelegate SizeOf;

			static MethodDescFunctions()
			{
				AddFunction<SizeOfDelegate>("MethodDesc::SizeOf", SizeOfSignature);
				SizeOf = (SizeOfDelegate) Functions["MethodDesc::SizeOf"];
			}
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

		internal static class FieldDescFunctions
		{
			private const string LoadSizeSignature = "48 83 EC 28 8B 51 0C 48 8D 05 4A 25 63 00 C1 EA 1B";

			internal delegate int LoadSizeDelegate(FieldDesc* __this);

			internal static readonly LoadSizeDelegate LoadSize;

			private const string GetStaticAddressSignature = "53 48 83 EC 20 48 8B D9 E8 8F BC 05 00 8B 53 0C";

			internal delegate void* GetStaticAddressDelegate(FieldDesc* __this, void* @base);

			internal static readonly GetStaticAddressDelegate GetStaticAddress;

			static FieldDescFunctions()
			{
				AddFunction<LoadSizeDelegate>("FieldDesc::LoadSize", LoadSizeSignature);
				LoadSize = (LoadSizeDelegate) Functions["FieldDesc::LoadSize"];

				AddFunction<GetStaticAddressDelegate>("FieldDesc::GetStaticAddress", GetStaticAddressSignature);
				GetStaticAddress = (GetStaticAddressDelegate) Functions["FieldDesc::GetStaticAddress"];
			}
		}


	}

}