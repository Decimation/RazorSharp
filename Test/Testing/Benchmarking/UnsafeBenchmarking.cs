#region

using BenchmarkDotNet.Attributes;
using RazorSharp;

#endregion

namespace Test.Testing.Benchmarking
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion

	public class UnsafeBenchmarking
	{
		private Dummy _dummy;

		[GlobalSetup]
		public void Setup()
		{
			_dummy = new Dummy();
		}

		[Benchmark]
		public void OffsetOf()
		{
			Unsafe.OffsetOf(ref _dummy, _dummy.Decimal);
		}

		[Benchmark]
		public void OffsetOfByName()
		{
			Unsafe.OffsetOf<Dummy>("_decimal");
		}

		[Benchmark]
		public void AddressOfByName()
		{
			Unsafe.AddressOf(ref _dummy, "_int");
		}


		//[Benchmark]
		public void SizeOf_Test()
		{
			// Inline Unsafe.SizeOf<T>

			CSUnsafe.SizeOf<int>();
		}
	}

}