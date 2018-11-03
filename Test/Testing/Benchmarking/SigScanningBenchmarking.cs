#region

using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using RazorSharp.CLR;
using Test.Testing.Types;

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
			IntPtr p = typeof(Dummy).GetMethodDesc("Increment").Reference.Function;
		}

		[Benchmark]
		public void SigScanningMethodName()
		{
			string n = typeof(Dummy).GetMethodDesc("DoSomething").Reference.Name;
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
			string n = typeof(Dummy).GetFieldDesc("_int").Reference.Name;
		}


	}

}