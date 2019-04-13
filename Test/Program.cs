#region

using System;
using RazorSharp.CoreClr;
using RazorSharp.Memory.Calling.Symbols.Attributes;
using RazorSharp.Memory.Pointers;
using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

#endregion


namespace Test
{
	#region

	using DWORD = UInt32;
	using Ptr = Pointer<byte>;

	#endregion


	public static class Program
	{
		// todo: replace native pointers* with Pointer<T> for consistency
		// todo: RazorSharp, ClrMD, Reflection, Cecil, dnlib, MetadataTools comparison

		// Common library: RazorCommon
		// Testing library: RazorSandbox

		[ClrSymcall(Symbol = "Object::GetSize", FullyQualified = true)]
		private static int Size(this object obj)
		{
			return Constants.INVALID_VALUE;
		}

		// https://github.com/dotnet/coreclr/blob/master/src/vm/jitinterface.cpp#L6961
		// https://github.com/dotnet/coreclr/blob/master/src/vm/jitinterface.cpp#L7090


		public static void Main(string[] args)
		{
			
		}
	}
}