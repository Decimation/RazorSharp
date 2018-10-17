#region

using BenchmarkDotNet.Attributes;
using RazorSharp.CLR.Structures;
using RazorSharp.Memory;

#endregion

namespace Test.Testing.Benchmarking
{

	public unsafe class SignatureCallBenchmarking
	{
		//                 Method |     Mean |    Error |   StdDev |
		// ---------------------- |---------:|---------:|---------:|
		//  TranspileSingleInline | 885.2 us | 2.511 us | 2.349 us |
		//  TranspileSingleCached | 874.9 us | 1.432 us | 1.270 us |
		//

		public class CClass
		{
			[ClrSigcall(
				"4C 8B 01 49 83 E0 FC 41 F7 00 00 00 00 80 41 8B 40 04 74 0E 8B 51 08 41 0F B7 08 48 0F AF D1 48 03 C2")]
			private void doSomething() { }

			[ClrSigcall]
			private void doSomething2() { }

			[ClrSigcall]
			private void doSomething3() { }
		}


		[GlobalSetup]
		public void Setup()
		{
			SignatureCall.DynamicBind<MethodDesc>();
			SignatureCall.DynamicBind<FieldDesc>();
			SignatureCall.DynamicBind<GCHeap>();
		}

		private readonly byte[] m_sig =
		{
			0x4C, 0x8B, 0x01, 0x49, 0x83, 0xE0, 0xFC, 0x41, 0xF7, 0x00, 0x00, 0x00, 0x00, 0x80, 0x41, 0x8B,
			0x40, 0x04, 0x74, 0x0E, 0x8B, 0x51, 0x08, 0x41, 0x0F, 0xB7, 0x08, 0x48, 0x0F, 0xAF, 0xD1, 0x48,
			0x03, 0xC2
		};

		[IterationCleanup]
		public void Cleanup()
		{
			SignatureCall.Clear();
		}

		[Benchmark]
		public void CacheNoToken()
		{
			SignatureCall.CacheFunction<CClass>("doSomething2", m_sig);
		}

		[Benchmark]
		public void CacheToken()
		{
			SignatureCall.CacheFunction<CClass>(100663534, m_sig);
		}


//		[Benchmark]
		public void BindSingleInline()
		{
			SignatureCall.DynamicBind<CClass>("doSomething");
		}

//		[Benchmark]
		public void BindSingleCached()
		{
			SignatureCall.DynamicBind<CClass>("doSomething2");
		}

	}

}