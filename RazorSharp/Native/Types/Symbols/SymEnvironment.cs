using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RazorCommon.Diagnostics;
using RazorSharp.CoreClr;
using RazorSharp.Memory;
using RazorSharp.Native.Enums.Images;
using RazorSharp.Native.Structures.Symbols;
using RazorSharp.Pointers;

namespace RazorSharp.Native.Types.Symbols
{
	// todo: WIP
	public unsafe class SymEnvironment : IDisposable
	{
		private          IntPtr       m_proc;
		private readonly List<Symbol> m_symbols;
		private const    string       MASK_WILDCARD = "*";
		private          ulong        m_dllbase;

		public SymEnvironment(string imgName, string mask = MASK_WILDCARD)
		{
			m_symbols = new List<Symbol>();

			var proc = Kernel32.GetCurrentProcess();

			Conditions.Require(DbgHelp.SymInitialize(proc, null, false));

			m_dllbase = (ulong) Clr.ClrModule.BaseAddress;

			Global.Log.Debug("dll base {Base}", m_dllbase.ToString("X"));

			m_dllbase = DbgHelp.SymLoadModuleEx(proc,
			                                    IntPtr.Zero,
			                                    imgName,
			                                    null,
			                                    m_dllbase,
			                                    (uint) Clr.ClrModule.ModuleMemorySize,
			                                    IntPtr.Zero,
			                                    0);


			Global.Log.Debug("Code {Code}", Marshal.GetLastWin32Error());
			
			Conditions.Require(m_dllbase != 0, nameof(m_dllbase));

			Global.Log.Debug("dll base {Base}", m_dllbase.ToString("X"));

			bool status = DbgHelp.SymEnumSymbols(proc,
			                                     m_dllbase,
			                                     mask,
			                                     EnumSymProc,
			                                     IntPtr.Zero);

			Conditions.Require(status);
		}

		public T GetSymbolValue<T>(Symbol sym)
		{
			var ptr = AllocSymbolValue<T>(sym);
			var buf = ptr.Reference;
			Mem.Free(ptr);
			return buf;
		}

		public Pointer<T> AllocSymbolValue<T>(Symbol sym)
		{
			var symPtr = sym.GetSymbolInfo();

			void* nil = null;

			bool status = DbgHelp.SymGetTypeInfo(m_proc,
			                                     m_dllbase,
			                                     symPtr.Reference.TypeIndex,
			                                     ImageHelpSymbolTypeInfo.TI_GET_ADDRESS,
			                                     &nil);


			Pointer<T> allocCpy = null;
			if (status) {
				Global.Log.Debug("gud");
			}
			else {
				Global.Log.Error("Fail {code}", Marshal.GetLastWin32Error());
			}

			Mem.Free(symPtr);
			return allocCpy;
		}

		private bool EnumSymProc(IntPtr symInfoPtr, uint symSize, IntPtr userCtx)
		{
			var pSymInfo = (SymbolInfo*) symInfoPtr;

			m_symbols.Add(new Symbol(pSymInfo));

			return true;
		}

		public Symbol[] Search(string userCtx)
		{
			return m_symbols.Where(sym => sym.Name.Contains(userCtx)).ToArray();
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