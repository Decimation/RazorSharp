using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RazorSharp.Memory;
using RazorSharp.Native.Enums;
using RazorSharp.Native.Structures;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

namespace RazorSharp.Native
{
	public unsafe class Symbolism : IDisposable
	{
		private IntPtr m_process,
		               m_addrBuffer,
		               m_imgStrNative,
		               m_maskStrNative;

		private const ulong  BASE_DEFAULT     = 0x400000;
		private const uint   SIZE_DEFAULT     = 0x20000;
		private const string MASK_STR_DEFAULT = "*!*";

		private readonly ulong m_base,
		                       m_dllBase;

		private const long INVALID_OFFSET = -4194304;

		public Symbolism(string image, string mask, ulong @base, uint size)
		{
			m_process = Kernel32.GetCurrentProcess();

			m_base = @base;

			m_imgStrNative  = Marshal.StringToHGlobalAnsi(image);
			m_maskStrNative = Marshal.StringToHGlobalAnsi(mask);

			Conditions.Assert(DbgHelp.SymInitialize(m_process, null, false));

			m_dllBase = DbgHelp.SymLoadModuleEx(m_process, IntPtr.Zero,
			                                    m_imgStrNative,
			                                    IntPtr.Zero,
			                                    m_base,
			                                    size,
			                                    IntPtr.Zero,
			                                    0);
		}

		public Symbolism(string image) : this(image, MASK_STR_DEFAULT, BASE_DEFAULT, SIZE_DEFAULT) { }

		public Pointer<byte> GetSymAddress(string userContext, string module)
		{
			var offset = GetSymOffset(userContext);
			return Modules.GetAddress(module, offset);
		}

		private void CheckOffset(long offset)
		{
			Conditions.Requires(offset != INVALID_OFFSET, "Offset is invalid");
		}

		public long[] GetSymOffsets(string[] userContext)
		{
			int len     = userContext.Length;
			var offsets = new List<long>();

			for (int i = 0; i < len; i++) {
				var ctxStrNative = Marshal.StringToHGlobalAnsi(userContext[i]);
				SymEnumSymbols(ctxStrNative);
				SymEnumTypes(ctxStrNative);
				var ofs = (m_addrBuffer - (int) m_base).ToInt64();
				CheckOffset(ofs);
				offsets.Add(ofs);
				Marshal.FreeHGlobal(ctxStrNative);
			}

			return offsets.ToArray();
		}

		private bool EnumSymProc(IntPtr pSymInfoX, uint reserved, IntPtr userContext)
		{
			Conditions.RequiresNotNull(pSymInfoX, nameof(pSymInfoX));
			var pSymInfo = (SymbolInfo*) pSymInfoX;
			var str      = Marshal.PtrToStringAnsi(userContext);
			Conditions.RequiresNotNull(str, nameof(str));
			int maxCmpLen = str.Length;

			if (maxCmpLen == pSymInfo->NameLen) {
				var s = Marshal.PtrToStringAnsi(new IntPtr(&pSymInfo->Name), (int) pSymInfo->NameLen);

				if (String.CompareOrdinal(s, str) == 0) {
					var childs = new TI_FINDCHILDREN_PARAMS();
					DbgHelp.SymGetTypeInfo(m_process, pSymInfo->ModBase, pSymInfo->TypeIndex,
					                       IMAGEHLP_SYMBOL_TYPE_INFO.TI_GET_CHILDRENCOUNT, &childs.Count);

					m_addrBuffer = (IntPtr) pSymInfo->Address;
				}
			}

			return true;
		}

		private void SymEnumSymbols(IntPtr ctxStrNative)
		{
			Conditions.Assert(DbgHelp.SymEnumSymbols(m_process, m_dllBase, m_maskStrNative, EnumSymProc, ctxStrNative));
		}

		private void SymEnumTypes(IntPtr ctxStrNative)
		{
			Conditions.Assert(DbgHelp.SymEnumTypes(m_process, m_dllBase, EnumSymProc, ctxStrNative));
		}

		public long this[string userContext] {
			get { return GetSymOffset(userContext); }
		}

		public TDelegate GetFunction<TDelegate>(string userContext, string module) where TDelegate : Delegate
		{
			var addr = GetSymAddress(userContext, module);
			return Marshal.GetDelegateForFunctionPointer<TDelegate>(addr.Address);
		}

		public long GetSymOffset(string userContext)
		{
			var ctxStrNative = Marshal.StringToHGlobalAnsi(userContext);

			SymEnumSymbols(ctxStrNative);
			SymEnumTypes(ctxStrNative);

			Marshal.FreeHGlobal(ctxStrNative);

			var offset = (m_addrBuffer - (int) m_base).ToInt64();
			CheckOffset(offset);
			return offset;
		}

		private void ReleaseUnmanagedResources()
		{
			Conditions.Assert(DbgHelp.SymCleanup(m_process));
			Marshal.FreeHGlobal(m_imgStrNative);
			Marshal.FreeHGlobal(m_maskStrNative);
			m_process       = IntPtr.Zero;
			m_addrBuffer    = IntPtr.Zero;
			m_imgStrNative  = IntPtr.Zero;
			m_maskStrNative = IntPtr.Zero;
		}

		public void Dispose()
		{
			ReleaseUnmanagedResources();
			GC.SuppressFinalize(this);
//			Global.Log.Debug("Completed disposing");
		}

		public static Pointer<byte> GetSymAddress(string image, string module, string name)
		{
			using (var sym = new Symbolism(image)) {
				return sym.GetSymAddress(name, module);
			}
		}

		// todo: make portable
		internal const string CLR_PDB = @"C:\Users\Deci\Desktop\clrx.pdb";
	}
}