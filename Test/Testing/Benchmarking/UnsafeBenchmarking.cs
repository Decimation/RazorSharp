#region

using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Test.Testing.Types;

#endregion

namespace Test.Testing.Benchmarking
{

	#region

	using CSUnsafe = Unsafe;

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
			RazorSharp.Unsafe.OffsetOf(ref _dummy, _dummy.Decimal);
		}

		[Benchmark]
		public void OffsetOfByName()
		{
			RazorSharp.Unsafe.OffsetOf<Dummy>("_decimal");
		}

		[Benchmark]
		public void AddressOfByName()
		{
			RazorSharp.Unsafe.AddressOfField(ref _dummy, "_int");
		}


		//[Benchmark]
		public void SizeOf_Test()
		{
			// Inline Unsafe.SizeOf<T>

			CSUnsafe.SizeOf<int>();
		}
	}

}