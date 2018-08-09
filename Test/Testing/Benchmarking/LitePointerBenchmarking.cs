#region

using BenchmarkDotNet.Attributes;
using RazorSharp.Pointers;

#endregion

namespace Test.Testing.Benchmarking
{

	public class LitePointerBenchmarking
	{
		private Pointer<string> lpString;
		private string          value = "foo";

		[GlobalSetup]
		public void Setup()
		{
			lpString = new Pointer<string>(ref value);
		}

		[Benchmark]
		public void Test_Value()
		{
			var x = lpString.Value;
		}

		[Benchmark]
		public void Test_Index()
		{
			var x = lpString[0];
		}

		[Benchmark]
		public void Test_Increment()
		{
			lpString++;
		}

		[Benchmark]
		public void Test_Decrement()
		{
			lpString--;
		}
	}

}