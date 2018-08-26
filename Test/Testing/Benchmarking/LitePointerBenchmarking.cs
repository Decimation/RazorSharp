#region

using BenchmarkDotNet.Attributes;
using RazorSharp.Pointers;

#endregion

namespace Test.Testing.Benchmarking
{

	public class LitePointerBenchmarking
	{
		private Pointer<string> _lpString;
		private string          _value = "foo";

		[GlobalSetup]
		public void Setup()
		{
			_lpString = new Pointer<string>(ref _value);
		}

		[Benchmark]
		public void Test_Value()
		{
			string x = _lpString.Value;
		}

		[Benchmark]
		public void Test_Index()
		{
			string x = _lpString[0];
		}

		[Benchmark]
		public void Test_Increment()
		{
			_lpString++;
		}

		[Benchmark]
		public void Test_Decrement()
		{
			_lpString--;
		}
	}

}