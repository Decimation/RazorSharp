#region

using BenchmarkDotNet.Attributes;
using RazorSharp.Runtime;
using RazorSharp.Runtime.CLRTypes;

#endregion

namespace Test.Testing.Benchmarking
{

	public unsafe class MethodTableAcquisitionBenchmarking
	{
		private string String = "foo";

		[Benchmark]
		public void MethodTable_Read()
		{
			var mt = Runtime.ReadMethodTable(ref String);
		}

		[Benchmark]
		public void MethodTable_TypeHandle()
		{
			var mt = (MethodTable*) typeof(string).TypeHandle.Value;
		}

		[Benchmark]
		public void MethodTable_MethodTableOf()
		{
			var mt = Runtime.MethodTableOf<string>();
		}
	}

}