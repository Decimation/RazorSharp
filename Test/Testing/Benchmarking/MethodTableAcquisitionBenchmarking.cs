#region

using BenchmarkDotNet.Attributes;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;

#endregion

namespace Test.Testing.Benchmarking
{

	public unsafe class MethodTableAcquisitionBenchmarking
	{
		private string String = "foo";

		[Benchmark]
		public void MethodTable_Read()
		{
			MethodTable* mt = Runtime.ReadMethodTable(ref String);
		}

		[Benchmark]
		public void MethodTable_TypeHandle()
		{
			MethodTable* mt = (MethodTable*) typeof(string).TypeHandle.Value;
		}

		[Benchmark]
		public void MethodTable_MethodTableOf()
		{
			MethodTable* mt = Runtime.MethodTableOf<string>();
		}
	}

}