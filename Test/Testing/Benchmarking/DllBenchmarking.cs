#region

using System;
using BenchmarkDotNet.Attributes;
using RazorInvoke;
using RazorInvoke.Libraries;
using RazorSharp;

#endregion

namespace Test.Testing.Benchmarking
{

	public class DllBenchmarking
	{
		private int    i;
		private IntPtr m_addr;

		[GlobalSetup]
		public void Setup()
		{
			m_addr = Unsafe.AddressOf(ref i);
		}

		[Benchmark]
		public void DllImport()
		{
			Kernel32.ZeroMemory(m_addr, (IntPtr) sizeof(int));
		}

		private delegate void ZeroMemory(IntPtr addr, int i);

		[Benchmark]
		public void GetFunction()
		{
			Functions.GetFunction<ZeroMemory>("Kernel32.dll", "RtlZeroMemory")(m_addr, sizeof(int));
		}
	}

}