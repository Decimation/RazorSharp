using System.Reflection;
using BenchmarkDotNet.Attributes;
using RazorSharp.Runtime;

namespace Test.Testing.Benchmarking
{

	public unsafe class SigScanningBenchmarking
	{
		[Benchmark]
		public void ReflectionFunctionPointer()
		{
			var p = typeof(Dummy).GetMethod("Increment").MethodHandle.GetFunctionPointer();
		}

		[Benchmark]
		public void SigScanningFunctionPointer()
		{
			var p = Runtime.GetMethodDesc<Dummy>("Increment")->Function;
		}

		[Benchmark]
		public void SigScanningMethodName()
		{
			var n = Runtime.GetMethodDesc<Dummy>("DoSomething")->Name;
		}

		[Benchmark]
		public void ReflectionMethodName()
		{
			var n = typeof(Dummy).GetMethod("DoSomething").Name;
		}

		[Benchmark]
		public void ReflectionFieldName()
		{
			var n = typeof(Dummy).GetField("_int", BindingFlags.Instance | BindingFlags.NonPublic).Name;
		}

		[Benchmark]
		public void SigScanningFieldName()
		{
			var n = Runtime.GetFieldDesc<Dummy>("_int")->Name;
		}


	}

}