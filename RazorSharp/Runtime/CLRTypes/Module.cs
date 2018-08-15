#region

using System;
using System.Runtime.InteropServices;
using RazorCommon;

#endregion

namespace RazorSharp.Runtime.CLRTypes
{

	#region

	using DWORD = UInt32;
	using WORD = UInt16;

	#endregion

	//todo: fix
	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/93955d4b58380068df9d99c58a699de6ad03f532/src/vm/ceeload.h#L1321
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct Module
	{
		[FieldOffset(0)] private readonly byte*       m_pSimpleName;
		[FieldOffset(8)]  private readonly void*       m_file;
		[FieldOffset(16)] private readonly MethodDesc* m_pDllMain;
		[FieldOffset(24)] private readonly DWORD       m_dwTransientFlags;
		[FieldOffset(28)] private readonly DWORD       m_dwPersistedFlags;
		[FieldOffset(32)] private readonly void*       m_pVASigCookieBlock;
		[FieldOffset(40)] private readonly void*       m_pAssembly;

		//todo
		private static void ModuleInfo(IntPtr module)
		{
			long* addrPtr = (long*) module.ToPointer();

			var assembly                = addrPtr + 6;
			var typeDefToMethodTableMap = addrPtr + 48;
			var typeRefToMethodTableMap = typeDefToMethodTableMap + 9;
			var methodDefToDescMap      = typeRefToMethodTableMap + 9;
			var fieldDefToDescMap       = methodDefToDescMap + 9;

			var table = new ConsoleTable("Data", "Address");

			table.AddRow("Assembly", Hex.ToHex(*assembly));
			table.AddRow("TypeDefToMethodTableMap", Hex.ToHex(*typeDefToMethodTableMap));
			table.AddRow("TypeRefToMethodTableMap", Hex.ToHex(*typeRefToMethodTableMap));
			table.AddRow("MethodDefToDescMap", Hex.ToHex(*methodDefToDescMap));
			table.AddRow("FieldDefToDescMap", Hex.ToHex(*fieldDefToDescMap));
			Console.WriteLine(table.ToMarkDownString());
		}

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Simple name", Hex.ToHex(m_pSimpleName));
			table.AddRow("File", Hex.ToHex(m_file));
			table.AddRow("Dll main", Hex.ToHex(m_pDllMain));
			table.AddRow("Transient flags", m_dwTransientFlags);
			table.AddRow("Persisted flags", m_dwPersistedFlags);
			table.AddRow("Cookie block", Hex.ToHex(m_pVASigCookieBlock));
			table.AddRow("Assembly", Hex.ToHex(m_pAssembly));
			return table.ToMarkDownString();
		}

		// Current line: https://github.com/dotnet/coreclr/blob/93955d4b58380068df9d99c58a699de6ad03f532/src/vm/ceeload.h#L1428
	}


}