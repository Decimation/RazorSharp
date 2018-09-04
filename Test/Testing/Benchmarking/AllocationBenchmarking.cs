using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using RazorInvoke.Libraries;

namespace Test.Testing.Benchmarking
{

	public class AllocationBenchmarking
	{
		private const int ALLOC_SIZE = 100;

		[Benchmark]
		public void AllocHGlobal_AndFree()
		{
			var p = Marshal.AllocHGlobal(ALLOC_SIZE);
			Marshal.FreeHGlobal(p);
		}

		[Benchmark]
		public void Malloc_AndFree()
		{
			var p = Msvcrt.Malloc(ALLOC_SIZE);
			Msvcrt.Free(p);
		}
	}

}