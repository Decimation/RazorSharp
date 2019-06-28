#region

using System;
using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
using System.Runtime.ExceptionServices;
using RazorSharp.Analysis;
using RazorSharp.Utilities;

#endregion


namespace Test
{
	#region

	using DWORD = UInt32;

	#endregion


	public static unsafe class Program
	{
		// Common library: SimpleSharp
		// Testing library: Sandbox


		private static void Test<T>(T value)
		{
			var options = InspectOptions.Values | InspectOptions.FieldOffsets
			                                    | InspectOptions.Addresses
			                                    | InspectOptions.InternalStructures
			                                    | InspectOptions.MemoryOffsets
			                                    | InspectOptions.AuxiliaryInfo
			                                    | InspectOptions.ArrayOrString;

			var layout = Inspect.Layout<T>(InspectOptions.Types);
			layout.Options |= options;
			layout.Populate(ref value);
			Console.WriteLine(layout);
		}


		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			var field = typeof(string).GetAnyField("m_firstChar").GetMetaField();
			
			Test("foo");
		}
	}
}