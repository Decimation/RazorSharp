using BenchmarkDotNet.Attributes;
using RazorSharp.Common;

namespace Test.Testing.Benchmarking
{

	public class ToStringBenchmarking
	{
		private readonly int[][] m_arrInline = {new[] {0}, new[] {1, 2}};
		private readonly int[]   m_arr       = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16};

		[Benchmark]
		public void Inline_ToString_EmbeddedILists()
		{
			Collections.InlineString(m_arrInline);
		}

		[Benchmark]
		public void Old_ToString_EmbeddedILists()
		{
			Collections.ToString(m_arrInline);
		}

//		[Benchmark]
		public void Inline_ToString_ByteArray()
		{
			Collections.InlineString(m_arr, ToStringOptions.Hex);
		}

//		[Benchmark]
		public void Old_ToString_ByteArray()
		{
			Collections.ToString(m_arr, ToStringOptions.Hex);
		}
	}

}