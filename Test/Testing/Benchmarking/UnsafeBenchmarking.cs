using BenchmarkDotNet.Attributes;

namespace Test.Testing.Benchmarking
{
	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	public class UnsafeBenchmarking
	{

		[Benchmark]
		public void SizeOf_Test()
		{
			// Inline Unsafe.SizeOf<T>

			CSUnsafe.SizeOf<int>();
		}
	}

}