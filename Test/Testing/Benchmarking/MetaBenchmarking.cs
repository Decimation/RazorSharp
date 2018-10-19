using BenchmarkDotNet.Attributes;
using RazorSharp.CLR.Meta;

namespace Test.Testing.Benchmarking
{

	public class MetaBenchmarking
	{
		[Benchmark]
		public void Runtime()
		{
			var f = RazorSharp.CLR.Runtime.GetFieldDesc<string>("m_firstChar");
		}

		[Benchmark]
		public void Meta()
		{
			MetaType mt = new MetaType(RazorSharp.CLR.Runtime.MethodTableOf<string>());
			var      f  = mt["m_firstChar"];
		}

		private MetaType m_type = new MetaType(RazorSharp.CLR.Runtime.MethodTableOf<string>());

		[Benchmark]
		public void Meta2()
		{
			var f = m_type["m_firstChar"];
		}
	}

}