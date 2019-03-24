using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Utilities;
using RazorSharp.CoreClr;
using RazorSharp.Memory;
using RazorSharp.Native.Enums;
using RazorSharp.Native.Structures;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

namespace RazorSharp.Native
{
	public unsafe class Symbols : IDisposable
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

		public Symbols(string image, string mask, ulong @base, uint size)
		{
			m_process = Kernel32.GetCurrentProcess();

			m_base = @base;

			m_imgStrNative  = Marshal.StringToHGlobalAnsi(image);
			m_maskStrNative = Marshal.StringToHGlobalAnsi(mask);

			Conditions.NativeRequire(DbgHelp.SymInitialize(m_process, null, false));

			m_dllBase = DbgHelp.SymLoadModuleEx(m_process, IntPtr.Zero, m_imgStrNative,
			                                    IntPtr.Zero, m_base, size, IntPtr.Zero, 0);
		}

		public Symbols(string image) : this(image, MASK_STR_DEFAULT, BASE_DEFAULT, SIZE_DEFAULT) { }

		public Pointer<byte> GetSymAddress(string userContext, string module)
		{
			var offset = GetSymOffset(userContext);
			return Modules.GetAddress(module, offset);
		}

		private static void CheckOffset(long offset)
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
					//var childs = new FindChildrenParams();

					// Don't ensure this method returns true
					//DbgHelp.SymGetTypeInfo(m_process, pSymInfo->ModBase, pSymInfo->TypeIndex,
					//                       ImageHelpSymbolTypeInfo.TI_GET_CHILDRENCOUNT, &childs.Count);

					m_addrBuffer = (IntPtr) pSymInfo->Address;
					return false;
				}
			}

			return true;
		}

		private void SymEnumSymbols(IntPtr ctxStrNative)
		{
			bool value = DbgHelp.SymEnumSymbols(m_process, m_dllBase, 
			                                    m_maskStrNative, EnumSymProc, ctxStrNative);
			
//			Conditions.NativeRequire(value);
		}

		private void SymEnumTypes(IntPtr ctxStrNative)
		{
			bool value = DbgHelp.SymEnumTypes(m_process, m_dllBase, EnumSymProc, ctxStrNative);
//			Conditions.NativeRequire(value);
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
			Conditions.NativeRequire(DbgHelp.SymCleanup(m_process));
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
			using (var sym = new Symbols(image)) {
				var addr = sym.GetSymAddress(name, module);
				return addr;
			}
		}


		internal static FileInfo DownloadSymbolFile(DirectoryInfo dest, FileInfo dll)
		{
			return DownloadSymbolFile(dest, dll, out _);
		}

		internal static FileInfo DownloadSymbolFile(DirectoryInfo dest, FileInfo dll, out DirectoryInfo super)
		{
			// symchk
			string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
			var    symChk    = new FileInfo(String.Format(@"{0}\Windows Kits\10\Debuggers\x64\symchk.exe", progFiles));
			Conditions.RequiresFileExists(symChk);

			string cmd = String.Format("\"{0}\" \"{1}\" /s SRV*{2}*http://msdl.microsoft.com/download/symbols",
			                           symChk.FullName, dll.FullName, dest.FullName);


			using (var cmdProc = Common.Shell("\"" + cmd + "\"")) {
				var startTime = DateTimeOffset.Now;

				cmdProc.ErrorDataReceived += (sender, args) =>
				{
					Global.Log.Error("Process error: {Error}", args.Data);
				};

				cmdProc.Start();

				var stdOut = cmdProc.StandardOutput;
				while (!stdOut.EndOfStream) {
					var ln = stdOut.ReadLine();
					Conditions.RequiresNotNull(ln, nameof(ln));
					if (ln.Contains("SYMCHK: PASSED + IGNORED files = 1")) {
						break;
					}

					if (DateTimeOffset.Now.Subtract(startTime).TotalMinutes > 1.5) {
						throw new TimeoutException("Could not download CLR symbols");
					}
				}
			}

			Global.Log.Debug("Done downloading symbols");

			string   pdbStr = dest.FullName + @"\" + Clr.CLR_PDB_SHORT;
			FileInfo pdb;

			if (Directory.Exists(pdbStr)) {
				// directory will be named <symbol>.pdb
				super = new DirectoryInfo(pdbStr);

				// sole child directory will be something like 9FF14BF5D36043909E88FF823F35EE3B2
				var children = super.GetDirectories();
				Conditions.Assert(children.Length == 1);
				var child = children[0];

				// (possibly sole) file will be the symbol file
				var files = child.GetFiles();
				pdb = files.First(f => f.Name.Contains(Clr.CLR_PDB_SHORT));
			}
			else if (File.Exists(pdbStr)) {
				super = null;
				pdb   = new FileInfo(pdbStr);
			}
			else {
				throw new Exception(String.Format("Error downloading symbols. File: {0}", pdbStr));
			}

			Conditions.RequiresFileExists(pdb);
			return pdb;
		}
	}
}