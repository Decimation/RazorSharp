using BenchmarkDotNet.Attributes;
using RazorSharp.Memory;
using RazorSharp.Pointers;

namespace Test.Testing.Benchmarking
{

	public class AllocPoolBenchmarking
	{
		private Pointer<byte> m_pAlloc;

		[IterationSetup]
		public void Setup()
		{
			m_pAlloc = AllocPool.Alloc<byte>(10);
		}

		[IterationCleanup]
		public void Cleanup()
		{
			AllocPool.Free(m_pAlloc);
		}

		[Benchmark]
		public void getOffset()
		{
			AllocPool.GetOffset(m_pAlloc);
		}

		[Benchmark]
		public void getLength()
		{
			AllocPool.GetLength(m_pAlloc);
		}

		[Benchmark]
		public void getSize()
		{
			AllocPool.GetSize(m_pAlloc);
		}


		[Benchmark]
		public void ReAlloc()
		{
			m_pAlloc=AllocPool.ReAlloc(m_pAlloc, 20);
		}

		[Benchmark]
		public void isAllocated()
		{
			AllocPool.IsAllocated(m_pAlloc);
		}



		[Benchmark]
		public void getOrigin()
		{
			AllocPool.GetOrigin(m_pAlloc);
		}
	}

}