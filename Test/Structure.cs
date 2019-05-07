using System;
using RazorSharp.Memory.Extern.Symbols.Attributes;

namespace Test
{
	[SymNamespace(PDB, DLL)]
	unsafe struct Structure
	{
		internal const string DLL = @"C:\Users\Deci\CLionProjects\NativeLib64\cmake-build-debug\NativeLib64.dll";
		internal const string PDB = @"C:\Users\Deci\CLionProjects\NativeLib64\cmake-build-debug\NativeLib64.pdb";
		
		[SymField(SymImportOptions.FullyQualified)]
		public int g_int32;

		[SymField(SymImportOptions.FullyQualified)]
		public byte* g_szStr;

		[SymCall(SymImportOptions.IgnoreEnclosingNamespace)]
		public void hello()
		{
			Console.WriteLine("Orig");
		}
	}
}