#region

using BenchmarkDotNet.Attributes;
using RazorSharp.CLR;
using RazorSharp.CLR.Meta;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;

#endregion

namespace Test.Testing.Benchmarking
{

	public class MetaBenchmarking
	{
		[Benchmark]
		public void Runtime()
		{
			Pointer<FieldDesc> f = typeof(string).GetFieldDesc("m_firstChar");
		}

		[Benchmark]
		public void Meta()
		{
			MetaType  mt = new MetaType(typeof(string).GetMethodTable());
			MetaField f  = mt["m_firstChar"];
		}

		private MetaType m_type = new MetaType(typeof(string).GetMethodTable());

		[Benchmark]
		public void Meta2()
		{
			MetaField f = m_type["m_firstChar"];
		}
	}

}