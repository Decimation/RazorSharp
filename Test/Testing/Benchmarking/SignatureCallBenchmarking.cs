using System;
using BenchmarkDotNet.Attributes;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Memory;
using static RazorSharp.Memory.SignatureCall;

namespace Test.Testing.Benchmarking
{

	public class SignatureCallBenchmarking
	{
		[Benchmark]
		public static void TranspileAllKnown()
		{
			SignatureCall.TranspileAllKnown();
		}

		[Benchmark]
		public void TranspileFieldDesc()
		{
			SignatureCall.TranspileIndependent<FieldDesc>();
		}
	}

}