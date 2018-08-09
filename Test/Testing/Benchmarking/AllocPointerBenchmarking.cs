#region

using BenchmarkDotNet.Attributes;
using RazorSharp.Pointers;

#endregion

namespace Test.Testing.Benchmarking
{

	public class AllocPointerBenchmarking
	{
		[Params(10)] public int ArrayLength;

		private AllocExPointer<int> m_allocPtr;

		[GlobalSetup]
		public void Setup()
		{
			m_allocPtr = new AllocExPointer<int>(ArrayLength);
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			m_allocPtr.Dispose();
		}

		[Benchmark]
		public void IteratePtr()
		{
			for (int i = 0; i < m_allocPtr.Count; i++) {
				var v = m_allocPtr[i];
			}
		}
	}

}