using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using RazorSharp.Memory;

namespace Test.Testing.Benchmarking
{

	public class CollectionBenchmarking
	{
		private AllocCollection<int> m_collection;
		private List<int>            m_list;

		[GlobalSetup]
		public void Setup()
		{
			m_list       = new List<int>(10);
			m_collection = new AllocCollection<int>(10);

			m_list.Add(0);
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			m_collection.Dispose();
		}

		[Benchmark]
		public void Index_List()
		{
			var v = m_list[0];
		}

		[Benchmark]
		public void Index_Alloc()
		{
			var v = m_collection[0];
		}


	}

}