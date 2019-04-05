using System;
using System.Collections.Generic;
using RazorCommon.Diagnostics;
using RazorSharp.Memory;
using RazorSharp.Native.Enums.Images;
using RazorSharp.Native.Structures.Symbols;
using RazorSharp.Pointers;

namespace RazorSharp.Native.Types.Symbols
{
	public unsafe class SymbolEnvironment : IDisposable
	{
		private IntPtr       m_proc;
		private List<Symbol> m_symbols;

		public SymbolEnvironment(string imgName, string mask)
		{
			m_symbols = new List<Symbol>();

			var proc = Kernel32.GetCurrentProcess();

			Conditions.Require(DbgHelp.SymInitialize(proc, null, false));

			ulong dllBase = 0x400;


			dllBase = DbgHelp.SymLoadModuleEx(proc,
			                                  IntPtr.Zero,
			                                  imgName,
			                                  null,
			                                  dllBase,
			                                  0,
			                                  IntPtr.Zero,
			                                  0);


			Conditions.Require(dllBase != 0, nameof(dllBase));


			bool status = DbgHelp.SymEnumSymbols(proc,
			                                     dllBase,
			                                     mask,
			                                     EnumSymProc,
			                                     IntPtr.Zero);

			Conditions.Require(status);
		}

		public Pointer<T> GetSymbolValue<T>(Symbol sym)
		{
			var symPtr = sym.GetSymbolInfo();
			var alloc  = Mem.AllocUnmanaged<byte>(256);
			bool status = DbgHelp.SymGetTypeInfo(m_proc,
			                                     0x400,
			                                     symPtr.Reference.TypeIndex,
			                                     ImageHelpSymbolTypeInfo.TI_GET_VALUE,
			                                     alloc.ToPointer());
			if (status) {
				return alloc.Cast<T>();
			}
			
			Mem.Free(alloc);
			Mem.Free(symPtr);
			return null;
		}

		private bool EnumSymProc(IntPtr symInfoPtr, uint symSize, IntPtr userCtx)
		{
			var pSymInfo = (SymbolInfo*) symInfoPtr;

			m_symbols.Add(new Symbol(pSymInfo));

			return true;
		}


		public void Dispose()
		{
			DbgHelp.SymCleanup(m_proc);
			Kernel32.CloseHandle(m_proc);
			
			m_proc = IntPtr.Zero;
			m_symbols.Clear();
		}
	}
}