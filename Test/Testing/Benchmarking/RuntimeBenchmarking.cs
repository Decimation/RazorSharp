using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using RazorSharp.Runtime.CLRTypes;

namespace Test.Testing.Benchmarking
{

	public unsafe class RuntimeBenchmarking
	{
		[Benchmark]
		[Arguments(typeof(Dummy), "DoSomething")]
		public MethodDesc GetMethodDesc(Type t, string name)
		{
			var methodHandle = t.GetMethod(name, BindingFlags.Instance | BindingFlags.Public).MethodHandle;
			return *(MethodDesc*) methodHandle.Value;
		}

		[Benchmark]
		[Arguments(typeof(Dummy), "_integer")]
		public FieldDesc GetFieldDesc(Type t, string name)
		{
			var fieldHandle = t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).FieldHandle;
			return *(FieldDesc*) fieldHandle.Value;
		}
	}

}