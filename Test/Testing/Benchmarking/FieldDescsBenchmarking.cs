using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using RazorCommon;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

namespace Test.Testing.Benchmarking
{

	public class FieldDescsBenchmarking
	{
		struct VectorOf3
		{
			private int a,
			            b,
			            c;
		}


		public unsafe static Pointer<FieldDesc>[] GetFieldDescs(Type t)
		{
			RazorContract.Requires(!t.IsArray, "Arrays do not have fields");

			MethodTable*         mt   = Runtime.MethodTableOf(t);
			int                  len  = mt->FieldDescListLength;
			Pointer<FieldDesc>[] lpFd = new Pointer<FieldDesc>[len];

			for (int i = 0; i < len; i++)
				lpFd[i] = &mt->FieldDescList[i];

			for (int i = 0; i < len; i++) {
				var fi = lpFd[i].Reference.Info;
			}


			return lpFd;
		}

		public unsafe static Pointer<FieldDesc>[] GetFieldDescsNoGetFI(Type t)
		{
			RazorContract.Requires(!t.IsArray, "Arrays do not have fields");

			MethodTable*         mt   = Runtime.MethodTableOf(t);
			int                  len  = mt->FieldDescListLength;
			Pointer<FieldDesc>[] lpFd = new Pointer<FieldDesc>[len];

			for (int i = 0; i < len; i++)
				lpFd[i] = &mt->FieldDescList[i];


			return lpFd;
		}

		private Pointer<FieldDesc> FD_getFieldInfo;

		[GlobalSetup]
		public void Setup()
		{
			FD_getFieldInfo = Runtime.GetFieldDesc<VectorOf3>("a");
		}

		[Benchmark]
		public void getFieldInfo()
		{
			var x = FD_getFieldInfo.Reference.Info;
		}

		[Benchmark]
		public static unsafe void Single2_NoGet()
		{
			var fds = GetFieldDescsNoGetFI(typeof(VectorOf3));
			for (int i = 0; i < fds.Length; i++) {
				if (fds[i].Reference.Info.Name == "a") {
					return;
				}
			}
		}

//		[Benchmark]
		public void All2()
		{
			GetFieldDescs(typeof(VectorOf3));
		}

		[Benchmark]
		public void All()
		{
			Runtime.GetFieldDescs<VectorOf3>();
		}

		[Benchmark]
		public void Single()
		{
			Runtime.GetFieldDesc<VectorOf3>("a");
		}
	}

}