using System.Linq;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Validators;

namespace Test.Testing.Benchmarking
{

	public class AllowNonOptimized : ManualConfig
	{
		public AllowNonOptimized()
		{
			Add(JitOptimizationsValidator.DontFailOnError); // ALLOW NON-OPTIMIZED DLLS

			Add(DefaultConfig.Instance.GetLoggers().ToArray());         // manual config has no loggers by default
			Add(DefaultConfig.Instance.GetExporters().ToArray());       // manual config has no exporters by default
			Add(DefaultConfig.Instance.GetColumnProviders().ToArray()); // manual config has no columns by default
		}
	}


	public class AddressBenchmarking
	{
		private int m_val;

		[Benchmark]
		public unsafe void CSUnsafe()
		{
			Unsafe.AsPointer(ref m_val);
		}

		[Benchmark]
		public void RazorSharp()
		{
			global::RazorSharp.Unsafe.AddressOf(ref m_val);
		}

		[Benchmark]
		public unsafe void Native()
		{
			fixed (int* ptr = &m_val) { }
		}
	}

}