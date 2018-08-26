using BenchmarkDotNet.Attributes;

namespace Test.Testing.Benchmarking
{

	public class MiscBenchmarking
	{
		private readonly int[] m_rgInt32 = new int[10];

		[Benchmark]
		public void For()
		{
			for (int i = 0; i < m_rgInt32.Length; i++) {
				int x = m_rgInt32[i];
			}
		}

		[Benchmark]
		public void ForEmpty()
		{
			for (int i = 0; i < m_rgInt32.Length; i++) {

			}
		}

		[Benchmark]
		public void ForEachEmpty()
		{
			foreach (var v in m_rgInt32) {

			}
		}

		[Benchmark]
		public void ForEach()
		{
			foreach (var v in m_rgInt32) {
				int x = v;
			}
		}
	}

}