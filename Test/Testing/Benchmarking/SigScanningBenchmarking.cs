#region

using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using RazorSharp.CLR;

#endregion

namespace Test.Testing.Benchmarking
{

	public unsafe class SigScanningBenchmarking
	{
		[Benchmark]
		public void ReflectionFunctionPointer()
		{
			IntPtr p = typeof(Dummy).GetMethod("Increment").MethodHandle.GetFunctionPointer();
		}

		[Benchmark]
		public void SigScanningFunctionPointer()
		{
			IntPtr p = Runtime.GetMethodDesc<Dummy>("Increment")->Function;
		}

		[Benchmark]
		public void SigScanningMethodName()
		{
			string n = Runtime.GetMethodDesc<Dummy>("DoSomething")->Name;
		}

		[Benchmark]
		public void ReflectionMethodName()
		{
			string n = typeof(Dummy).GetMethod("DoSomething").Name;
		}

		[Benchmark]
		public void ReflectionFieldName()
		{
			string n = typeof(Dummy).GetField("_int", BindingFlags.Instance | BindingFlags.NonPublic).Name;
		}

		[Benchmark]
		public void SigScanningFieldName()
		{
			string n = Runtime.GetFieldDesc<Dummy>("_int")->Name;
		}


	}

}