using System;
using JetBrains.Annotations;
using RazorSharp.Import.Attributes;

// ReSharper disable InconsistentNaming
#pragma warning disable 0649

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

		[SymField(SymImportOptions.FullyQualified)]
		public short* g_szWStr;
		
		[SymField(SymImportOptions.FullyQualified)]
		public short* g_sz16Str;
		
		[SymField(SymImportOptions.FullyQualified)]
		public int* g_sz32Str;

//		[SymField(nameof(g_sz32Str),SymImportOptions.FullyQualified)]
//		public string g_sz32Str2;

		[SymCall(SymImportOptions.IgnoreEnclosingNamespace)]
		public void hello()
		{
			Console.WriteLine("Orig");
		}
	}
}