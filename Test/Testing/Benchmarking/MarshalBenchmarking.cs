using System;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace Test.Testing.Benchmarking
{

	public class MarshalBenchmarking
	{
		private IntPtr _hMem;
		private int _length;

		[GlobalSetup]
		public void Setup()
		{
			_length = 1;
		}

		[Benchmark]
		public void New()
		{
			new object();
		}

		[Benchmark]
		public void Marshal_AllocHGlobal()
		{
			_hMem = Marshal.AllocHGlobal(_length);
		}

		[Benchmark]
		public void Marshal_FreeHGlobal()
		{
			Marshal.FreeHGlobal(_hMem);
		}
	}

}