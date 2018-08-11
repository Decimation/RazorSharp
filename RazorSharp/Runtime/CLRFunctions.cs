#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using RazorSharp.Memory;
using RazorSharp.Runtime.CLRTypes;

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.Runtime
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion


	/// <summary>
	/// Some CLR functions are too complex to replicate in C# so we'll use sigscanning to execute them
	/// </summary>
	internal static unsafe class CLRFunctions
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

		private static void AddFunction<TDelegate>(string name, string signature) where TDelegate : Delegate
		{
			Functions.Add(name, Scanner.GetDelegate<TDelegate>(signature));
		}

		internal static class ThreadFunctions
		{
			private const string GetStackLowerBoundSignature =
				"48 83 EC 58 48 8D 54 24 20 48 8D 4C 24 20 E8 A5 B7 F5 FF";

			public delegate void* GetStackLowerBoundDelegate();

			public static readonly GetStackLowerBoundDelegate GetStackLowerBound;

			private const string GetStackGuaranteeSignature =
				"48 83 EC 28 83 3D DD 41 93 00 00 0F 85 E3 6C 37 00 B8 00 10 00 00";

			internal delegate int GetStackGuaranteeDelegate(IntPtr __this);

			internal static readonly GetStackGuaranteeDelegate GetStackGuarantee;

			static ThreadFunctions()
			{
				AddFunction<GetStackLowerBoundDelegate>("Thread::GetStackLowerBound", GetStackLowerBoundSignature);
				GetStackLowerBound = (GetStackLowerBoundDelegate) Functions["Thread::GetStackLowerBound"];

				// var thread = Kernel32.OpenThread(0x4, false, Kernel32.GetCurrentThreadId());
				// Console.WriteLine("Stack size: {0}", CLRFunctions.ThreadFunctions.GetStackGuarantee(thread));
				AddFunction<GetStackGuaranteeDelegate>("Thread::GetStackGuarantee", GetStackGuaranteeSignature);
				GetStackGuarantee = (GetStackGuaranteeDelegate) Functions["Thread::GetStackGuarantee"];
			}

		}

		internal static class MethodDescFunctions
		{
			private const string GetMultiCallableAddrOfCodeSignature =
				"57 48 83 EC 40 48 C7 44 24 20 FE FF FF FF 48 89 5C 24 50 48 8B F9 E8 1D 00 00 00";

			internal delegate void* GetMultiCallableAddrOfCodeDelegate(MethodDesc* __this);

			/// <summary>
			/// https://github.com/dotnet/coreclr/blob/fcb04373e2015ae12b55f33fdd0dd4580110db98/src/vm/runtimehandles.cpp#L1732
			/// https://github.com/dotnet/coreclr/blob/c10efe004d8720a799bf666d3fac3b800f204848/src/vm/method.cpp#L2067
			/// </summary>
			internal static readonly GetMultiCallableAddrOfCodeDelegate GetMultiCallableAddrOfCode;

			static MethodDescFunctions()
			{
				// Sigscan

				AddFunction<GetMultiCallableAddrOfCodeDelegate>("MethodDesc::GetMultiCallableAddrOfCode",
					GetMultiCallableAddrOfCodeSignature);

				// Set up the delegate
				GetMultiCallableAddrOfCode =
					(GetMultiCallableAddrOfCodeDelegate) Functions["MethodDesc::GetMultiCallableAddrOfCode"];

				AddFunction<GetNameDelegate>("MethodDesc::GetName", GetNameSignature);
				GetName = (GetNameDelegate) Functions["MethodDesc::GetName"];
			}


			private const string GetNameSignature =
				"48 8B C4 57 48 83 EC 40 48 C7 40 D8 FE FF FF FF 48 89 58 10 48 89 68 18 48 89 70 20 48 8B F9 33 ED";

			// LPCUTF8
			internal delegate byte* GetNameDelegate(MethodDesc* __this);

			internal static readonly GetNameDelegate GetName;
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

				//Logger.Log("StringObject functions loaded");
			}

			internal static string NewString(byte* charConstPtr)
			{
				void* str = NewStringInternal(charConstPtr);
				return CSUnsafe.AsRef<string>(&str);
			}
		}

		internal static class FieldDescFunctions
		{
			private const string GetNameSignature =
				"48 89 5C 24 08 48 89 74 24 18 57 48 83 EC 20 48 8B D9 E8 85 4F EA FF";

			// LPCUTF8
			internal delegate byte* GetNameDelegate(FieldDesc* __this);

			internal static readonly GetNameDelegate GetName;

			private const string LoadSizeSignature = "48 83 EC 28 8B 51 0C 48 8D 05 4A 25 63 00 C1 EA 1B";

			internal delegate int LoadSizeDelegate(FieldDesc* __this);

			internal static readonly LoadSizeDelegate LoadSize;


			static FieldDescFunctions()
			{
				AddFunction<GetNameDelegate>("FieldDesc::GetName", GetNameSignature);
				GetName = (GetNameDelegate) Functions["FieldDesc::GetName"];

				AddFunction<LoadSizeDelegate>("FieldDesc::LoadSize", LoadSizeSignature);
				LoadSize = (LoadSizeDelegate) Functions["FieldDesc::LoadSize"];
			}
		}


	}

}