using System;
using BenchmarkDotNet.Attributes;
using RazorSharp.CLR;
using RazorSharp.Memory;
using static RazorSharp.Memory.SignatureCall;

namespace Test.Testing.Benchmarking
{

	public class SignatureCallBenchmarking
	{
		struct GCHeap_t
		{
			public uint GCCount {
				[Sigcall("clr.dll", "48 8B 05 59 F5 82 00 48 89 44 24 10 48 8B 44 24 10 C3")]
				get;
			}

			public bool IsGCInProgress {
				[Sigcall("clr.dll", "48 89 5C 24 08 48 89 74 24 10 57 48 83 EC 20 48 8B 3D DE F3 93 00 33 C0")]
				get;
			}
		}

		private const string GetGCCountSignature = "48 8B 05 59 F5 82 00 48 89 44 24 10 48 8B 44 24 10 C3";

		internal delegate uint GetGCCountDelegate(IntPtr __this);

		internal static GetGCCountDelegate GetGCCountInternal;

		[Benchmark]
		public static void OldFashion()
		{
			CLRFunctions.Functions.Clear();
			CLRFunctions.AddFunction<GetGCCountDelegate>("GCHeap::GetGCCount", GetGCCountSignature);
			GetGCCountInternal = (GetGCCountDelegate) CLRFunctions.Functions["GCHeap::GetGCCount"];
		}

		[Benchmark]
		public static void TranspileOne()
		{
			Transpile<GCHeap_t>("GCCount", true);
		}


//		[Benchmark]
		public static void TranspileBenchmarkAll()
		{
			Transpile<GCHeap_t>();
		}
	}

}