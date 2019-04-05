using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RazorCommon.Diagnostics;
using RazorSharp.CoreClr;
using RazorSharp.Native.Structures.Symbols;
using RazorSharp.Pointers;

namespace RazorSharp.Native
{
	public unsafe class SymCollection : IDisposable
	{
		private readonly List<Symbol> m_symbols;
		private readonly string m_imgName;
		private readonly string m_mask;

		private const string MASK_WILDCARD = "*";
		
		public SymCollection(string imageName, string mask = MASK_WILDCARD)
		{
			m_symbols = new List<Symbol>();
			m_imgName = imageName;
			m_mask = mask;
		}
		
		

		public void LoadAll()
		{
			var proc = Kernel32.GetCurrentProcess();

			Conditions.Require(DbgHelp.SymInitialize(proc,null,false));
			
			ulong  dllBase = 0x400;
			
			
			dllBase = DbgHelp.SymLoadModuleEx(proc,
			                                  IntPtr.Zero,
			                                  m_imgName,
			                                  null,
			                                  dllBase,
			                                  0,
			                                  IntPtr.Zero,
			                                  0);

			Conditions.Require(dllBase != 0, nameof(dllBase));
			

			bool status = DbgHelp.SymEnumSymbols(proc,
			                                     dllBase,
			                                     m_mask,
			                                     EnumSymProc,
			                                     IntPtr.Zero);

			Conditions.Require(status);

			DbgHelp.SymCleanup(proc);
			Kernel32.CloseHandle(proc);
		}

		public Symbol[] Search(string userCtx)
		{
			return m_symbols.Where(sym => sym.Name.Contains(userCtx)).ToArray();
		}


		private bool EnumSymProc(IntPtr symInfoPtr, uint symSize, IntPtr userCtx)
		{
			var pSymInfo = (SymbolInfo*) symInfoPtr;

			m_symbols.Add(new Symbol(pSymInfo));

			return true;
		}

		public void Dispose() { }
	}
}