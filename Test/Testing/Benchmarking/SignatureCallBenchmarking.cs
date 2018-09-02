#region

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using RazorSharp;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using Test.Testing.Types;

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
			[CLRSigcall(
				"4C 8B 01 49 83 E0 FC 41 F7 00 00 00 00 80 41 8B 40 04 74 0E 8B 51 08 41 0F B7 08 48 0F AF D1 48 03 C2")]
			void doSomething() { }

			[CLRSigcall]
			void doSomething2() { }
		}


		[GlobalSetup]
		public void Setup()
		{


			SignatureCall.DynamicBind<MethodDesc>();
			SignatureCall.DynamicBind<FieldDesc>();
			SignatureCall.DynamicBind<GCHeap>();

			SignatureCall.CacheFunction<CClass>("doSomething2",
				new byte[]
				{
					0x4C, 0x8B, 0x01, 0x49, 0x83, 0xE0, 0xFC, 0x41, 0xF7, 0x00, 0x00, 0x00, 0x00, 0x80, 0x41, 0x8B,
					0x40, 0x04, 0x74, 0x0E, 0x8B, 0x51, 0x08, 0x41, 0x0F, 0xB7, 0x08, 0x48, 0x0F, 0xAF, 0xD1, 0x48,
					0x03, 0xC2
				});
		}


		[Benchmark]
		public void BindSingleInline()
		{
			SignatureCall.DynamicBind<CClass>("doSomething");
		}

		[Benchmark]
		public void BindSingleCached()
		{
			SignatureCall.DynamicBind<CClass>("doSomething2");
		}

	}

}