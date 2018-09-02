using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Environments;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;
using Test.Testing.Types;
using Runtime = RazorSharp.CLR.Runtime;

namespace Test.Testing.Benchmarking
{

	//                   Method |     Mean |     Error |    StdDev |
	// ------------------------ |---------:|----------:|----------:|
	//     GetFields_ByMT_Check | 146.0 ns | 0.4860 ns | 0.4308 ns | <<
	//  GetFields_ByPtr_NoCheck | 149.8 ns | 0.4889 ns | 0.4573 ns |
	//   GetFields_ByMT_NoCheck | 135.8 ns | 0.4067 ns | 0.3605 ns |
	//   GetFields_ByReflection | 170.8 ns | 0.3591 ns | 0.3183 ns |
	public class RuntimeBenchmarking
	{


		[Benchmark]
		public void GetFields_ByMT_Check()
		{
			Runtime.GetFieldDescs<string>();
		}

		[Benchmark]
		public unsafe void GetFields_ByPtr_NoCheck()
		{
			string                                 s      = "foo";
			Pointer<Pointer<Pointer<MethodTable>>> pppMT  = Unsafe.AsPointer(ref s);
			var                                    fields = pppMT.Reference.Reference.Reference.FieldDescListLength;
			var                                    cpy    = new Pointer<FieldDesc>[fields];
			for (int i = 0; i < cpy.Length; i++) {
				cpy[i] = &pppMT.Reference.Reference.Reference.FieldDescList[i];
			}
		}

		[Benchmark]
		public unsafe void GetFields_ByMT_NoCheck()
		{
			Pointer<MethodTable> pppMT  = typeof(string).TypeHandle.Value;
			var                  fields = pppMT.Reference.FieldDescListLength;
			var                  cpy    = new Pointer<FieldDesc>[fields];
			for (int i = 0; i < cpy.Length; i++) {
				cpy[i] = &pppMT.Reference.FieldDescList[i];
			}
		}


		[Benchmark]
		public void GetFields_ByReflection()
		{
			Runtime.GetFields<string>();
		}
	}

}