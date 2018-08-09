#region

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using RazorCommon;

#endregion

namespace Test.Testing.Benchmarking
{

	public class CollectionsBenchmarking
	{
		private byte[]      Bytes     = {0xD, 0xE, 0xA, 0xD, 0xB, 0xE, 0xE, 0xF};
		private List<int>   List      = new List<int> {1, 2, 3, 4, 5};
		private List<int[]> ArrayList = new List<int[]> {new[] {1, 2, 3}, new[] {4, 5, 6}};

		[Benchmark]
		public void CollectionsToString_Bytes()
		{
			Collections.ToString(mem: Bytes);
		}

		public static void StringJoin() { }

		[Benchmark]
		public void CollectionsToString_List()
		{
			Collections.ListToString(List);
		}

		[Benchmark]
		public void CollectionsToString_ArrayList()
		{
			Collections.ListToString(ArrayList);
		}
	}

}