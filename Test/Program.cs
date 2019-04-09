#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using RazorCommon;
using RazorCommon.Diagnostics;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.EE;
using RazorSharp.Memory;
using RazorSharp.Memory.Calling.Symbols;
using RazorSharp.Memory.Calling.Symbols.Attributes;
using RazorSharp.Memory.Fixed;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native;
using RazorSharp.Native.Symbols;
using RazorSharp.Native.Win32;
using RazorSharp.Utilities;
using Test.PEFile;
using CSUnsafe = System.Runtime.CompilerServices.Unsafe;
using Unsafe = RazorSharp.Memory.Unsafe;

#endregion


namespace Test
{
	#region

	using DWORD = UInt32;

	#endregion

	using Ptr = Pointer<byte>;

	public static unsafe class Program
	{
		// todo: replace native pointers* with Pointer<T> for consistency
		// todo: RazorSharp, ClrMD, Reflection, Cecil comparison

		// Common library: RazorCommon
		// Testing library: RazorSandbox

		[ClrSymcall(Symbol = "Object::GetSize", FullyQualified = true)]
		private static int Size(this object obj)
		{
			return Constants.INVALID_VALUE;
		}


		private class MyStruct
		{
			// #define IDS_CLASSLOAD_MISSINGMETHODRVA          0x1797

			// MethodTableBuilder::ValidateMethods()
			// https://github.com/dotnet/coreclr/blob/master/src/vm/methodtablebuilder.cpp
			// 4880: BuildMethodTableThrowException(IDS_CLASSLOAD_MISSINGMETHODRVA, it.Token());
//			[MethodImpl(MethodImplOptions.InternalCall)]

			[ClrSymcall(Symbol = "Object::GetSize", FullyQualified = true)]
			public int Run()
			{
				throw null;
			}
		}


		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			//Core.Setup();

			

			
			string img = @"C:\Users\Deci\RiderProjects\RazorSharp\RazorSharp\bin\x64\Debug\RazorSharp.dll";
			using (var stream = File.OpenRead(img)) {
				var imageReader = new JbImageReader(stream);
				if (imageReader.ReadImage()) {
					Console.WriteLine(imageReader.ReadDataDirectory());
					Image i = imageReader.image;
					Console.WriteLine("rva: {0:X}",imageReader.metadata.VirtualAddress);
					Console.WriteLine(i.Sections[0].Name);

					foreach (var s in i.Sections) {
						Console.WriteLine(s.Name);
					}

					

					Console.WriteLine("raw data {0:X}",i.MetadataSection.PointerToRawData);


					

				}
			}

			//Core.Close();
		}

		static void ReadToken(uint u)
		{
			
		}

		static string ReadCString(byte[] buf)
		{
			return Encoding.ASCII.GetString(buf);
		}


		private static void Arglist(__arglist)
		{
			var rg = new ArgIterator(__arglist);

			while (rg.GetRemainingCount() > 0) {
				var v = rg.GetNextArg();
				Console.WriteLine(TypedReference.ToObject(v));
			}
		}
	}
}