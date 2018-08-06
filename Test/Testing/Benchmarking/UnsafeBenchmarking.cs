using BenchmarkDotNet.Attributes;
using RazorSharp;

namespace Test.Testing.Benchmarking
{
	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	public class UnsafeBenchmarking
	{
		private Dummy _dummy;

		[GlobalSetup]
		public void Setup()
		{
			_dummy = new Dummy();
		}

		[Benchmark]
		public void OffsetOf()
		{
			Unsafe.OffsetOf(ref _dummy, _dummy.Decimal);
		}

		[Benchmark]
		public void OffsetOfByName()
		{
			Unsafe.OffsetOf<Dummy>("_decimal");
		}




		//[Benchmark]
		public void SizeOf_Test()
		{
			// Inline Unsafe.SizeOf<T>

			CSUnsafe.SizeOf<int>();
		}
	}

}