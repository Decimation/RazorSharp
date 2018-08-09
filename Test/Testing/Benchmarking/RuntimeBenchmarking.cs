#region

using BenchmarkDotNet.Attributes;
using RazorSharp.Runtime;

#endregion

namespace Test.Testing.Benchmarking
{

	public unsafe class RuntimeBenchmarking
	{
		private Dummy _dummy;


		[GlobalSetup]
		public void Setup()
		{
			_dummy = new Dummy();
		}

		[Benchmark]
		public void MethodTable_HeapObject()
		{
			var mt = (**Runtime.GetHeapObject(ref _dummy)).MethodTable;
		}

		[Benchmark]
		public void MethodTable_ReadMethodTable()
		{
			var mt = Runtime.ReadMethodTable(ref _dummy);
		}

		[Benchmark]
		public void MethodTable_MethodTableOf()
		{
			var mt = Runtime.MethodTableOf<Dummy>();
		}
	}

}