#region

using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;

#endregion

namespace Test.Testing.Benchmarking
{

	//                   Method |     Mean |     Error |    StdDev |
	// ------------------------ |---------:|----------:|----------:|
	//     GetFields_ByMT_Check | 128.0 ns | 0.3832 ns | 0.3584 ns | <<
	//  GetFields_ByPtr_NoCheck | 121.8 ns | 0.8253 ns | 0.7316 ns |
	//   GetFields_ByMT_NoCheck | 122.5 ns | 0.3999 ns | 0.3741 ns |
	//   GetFields_ByReflection | 374.4 ns | 3.0981 ns | 2.8979 ns |
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
			int                                    fields = pppMT.Reference.Reference.Reference.FieldDescListLength;
			Pointer<FieldDesc>[]                   cpy    = new Pointer<FieldDesc>[fields];
			for (int i = 0; i < cpy.Length; i++) {
				cpy[i] = &pppMT.Reference.Reference.Reference.FieldDescList[i];
			}
		}

		[Benchmark]
		public unsafe void GetFields_ByMT_NoCheck()
		{
			Pointer<MethodTable> pppMT  = typeof(string).TypeHandle.Value;
			int                  fields = pppMT.Reference.FieldDescListLength;
			Pointer<FieldDesc>[] cpy    = new Pointer<FieldDesc>[fields];
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