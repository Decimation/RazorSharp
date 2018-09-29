#region

using BenchmarkDotNet.Attributes;
using static RazorSharp.Utilities.RazorContract;

#endregion

namespace Test.Testing.Benchmarking
{

	public class ContractBenchmarking
	{
		[Benchmark]
		public void RequiresClassType()
		{
			RequiresClassType<string>();
		}
	}

}