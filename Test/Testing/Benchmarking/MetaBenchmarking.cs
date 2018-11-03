using BenchmarkDotNet.Attributes;
using RazorSharp.CLR;
using RazorSharp.CLR.Meta;

namespace Test.Testing.Benchmarking
{

	public class MetaBenchmarking
	{
		[Benchmark]
		public void Runtime()
		{
			var f = typeof(string).GetFieldDesc("m_firstChar");
		}

		[Benchmark]
		public void Meta()
		{
			MetaType mt = new MetaType(typeof(string).GetMethodTable());
			var      f  = mt["m_firstChar"];
		}

		private MetaType m_type = new MetaType(typeof(string).GetMethodTable());

		[Benchmark]
		public void Meta2()
		{
			var f = m_type["m_firstChar"];
		}
	}

}