using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using RazorSharp.Pointers;

namespace Test.Testing
{

	public class ArrayPointerBenchmarking
	{
		[Params(10)] public int ArrayLength;

		private int[]             m_array;
		private ArrayPointer<int> m_ptr;

		[Benchmark]
		public void IteratePointer()
		{
			for (int i = 0; i < m_ptr.Count; i++) {
				var v = m_ptr[i];
			}
		}

		[Benchmark]
		public void PostFixIncrPointer()
		{
			for (int i = 0; i < m_ptr.Count; i++) {
				m_ptr++;
			}
		}

		[Benchmark]
		public void PostFixDecrPointer()
		{
			for (int i = m_ptr.Count - 1; i >= 0; i--) {
				m_ptr--;
			}
		}

		[Benchmark]
		public void IterateArray()
		{
			for (int i = 0; i < m_array.Length; i++) {
				var v = m_array[i];
			}
		}

		[GlobalSetup]
		public void Setup()
		{
			m_array = new int[ArrayLength];
			m_ptr   = m_array;
		}


	}

}