#region

using BenchmarkDotNet.Attributes;
using ObjectLayoutInspector;
using RazorSharp.Analysis;
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
		public void GetLayout()
		{
			TypeLayout.GetLayout<Dummy>();
		}

		[Benchmark]
		public void ObjectLayout()
		{
			new ObjectLayout<Dummy>(ref _dummy);
		}

		[Benchmark]
		public void GetFieldDescs()
		{
			Runtime.GetFieldDescs<Dummy>();
		}
	}

}