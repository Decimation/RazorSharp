using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Environments;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;
using Runtime = RazorSharp.CLR.Runtime;

namespace Test.Testing.Benchmarking
{

	public class RuntimeBenchmarking
	{
		[Benchmark]
		public void GetFieldsByMethodTable()
		{
			Runtime.GetFieldDescs<Dummy>();
		}



		[Benchmark]
		public void GetFieldsByReflection()
		{
			Runtime.GetFields<Dummy>();
		}
	}

}