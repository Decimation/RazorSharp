#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using RazorCommon.Diagnostics;
using RazorCommon.Utilities;
using RazorSharp.CoreClr;
using RazorSharp.Memory;
using RazorSharp.Native.Structures.Symbols;
using RazorSharp.Pointers;

#endregion

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression

namespace RazorSharp.Native
{
	// todo: add support for cases where there are multiple symbols of the same name
	
	/// <summary>
	/// Provides access to symbols in PDB files and matching them with the corresponding memory
	/// location in an image
	/// <remarks>
	/// "name" parameters are the user context parameters.
	/// </remarks>
	/// </summary>
	public unsafe class Symbols : IDisposable
	{
		private const ulong  BASE_DEFAULT     = 0x400000;
		private const uint   SIZE_DEFAULT     = 0x20000;
		private const string MASK_STR_DEFAULT = "*!*";

		private const long INVALID_OFFSET = -4194304;

		private const string OffsetsFileName = "RazorSharpSymbolOffsets.txt";

		/// <summary>
		///     The file which caches symbols and their offsets
		/// </summary>
		private static readonly FileInfo CacheFile;

		/// <summary>
		///     <para>Key: Symbol name</para>
		///     <para>Value: Symbol offset</para>
		/// </summary>
		private static readonly Dictionary<string, long> Cache;

		private readonly ulong m_base,
		                       m_dllBase;

		private IntPtr m_process,
		               m_addrBuffer,
		               m_imgStrNative,
		               m_maskStrNative;

		private SymbolInfo* m_symbolBuffer;

		static Symbols()
		{
			if (FileUtil.GetOrCreateTempFile(OffsetsFileName, out CacheFile)) {
				Global.Log.Verbose("Symbol cache file detected");
			}
			else {
				Global.Log.Verbose("Symbol cache file created");
			}

			Cache = FileUtil.ReadDictionary(CacheFile, s => s, Int64.Parse);

			Global.Log.Debug("Read {Count} cached offsets", Cache.Count);
		}

		public SymbolInfo* this[string name] => GetSymbol(name);

		public void Dispose()
		{
			ReleaseUnmanagedResources();
			GC.SuppressFinalize(this);
//			Global.Log.Debug("Completed disposing");
		}

		/// <summary>
		///     Caches the offsets in <seealso cref="Cache" />
		/// </summary>
		public static void Close()
		{
			File.WriteAllText(CacheFile.FullName, String.Empty);
			Global.Log.Verbose("Writing {Count} offsets to cache", Cache.Count);
			FileUtil.WriteDictionary(Cache, CacheFile);
		}


		public Pointer<byte> GetSymAddress(string name, string module)
		{
			long offset = GetSymOffset(name);
			return Modules.GetAddress(module, offset);
		}

		private static void CheckOffset(long offset)
		{
			Conditions.Ensure(offset != INVALID_OFFSET, nameof(offset));
		}


		public long[] GetSymOffsets(string[] names)
		{
			int len     = names.Length;
			var offsets = new List<long>();

			for (int i = 0; i < len; i++) {
				if (Cache.ContainsKey(names[i])) {
					offsets.Add(Cache[names[i]]);
				}
				else {
					var ctxStrNative = Marshal.StringToHGlobalAnsi(names[i]);
					SymEnumSymbols(ctxStrNative);
					SymEnumTypes(ctxStrNative);
					long ofs = (m_addrBuffer - (int) m_base).ToInt64();
					m_symbolBuffer = null;
					CheckOffset(ofs);
					offsets.Add(ofs);
					Marshal.FreeHGlobal(ctxStrNative);
					Cache.Add(names[i], ofs);
				}
			}

			return offsets.ToArray();
		}


		private bool EnumSymProc(IntPtr lpSymInfo, uint reserved, IntPtr userContext)
		{
			Conditions.NotNull(lpSymInfo, nameof(lpSymInfo));
			var    pSymInfo = (SymbolInfo*) lpSymInfo;
			string str      = Marshal.PtrToStringAnsi(userContext);
			Conditions.NotNull(str, nameof(str));
			int maxCmpLen = str.Length;

			if (maxCmpLen == pSymInfo->NameLen) {
				string s = NativeHelp.GetString(&pSymInfo->Name, pSymInfo->NameLen);

				if (String.CompareOrdinal(s, str) == 0) {
					//var childs = new FindChildrenParams();

					// Don't ensure this method returns true
					//DbgHelp.SymGetTypeInfo(m_process, pSymInfo->ModBase, pSymInfo->TypeIndex,
					//                       ImageHelpSymbolTypeInfo.TI_GET_CHILDRENCOUNT, &childs.Count);

					m_addrBuffer   = (IntPtr) pSymInfo->Address;
					m_symbolBuffer = pSymInfo;
					return false;
				}
			}

			return true;
		}

		private void SymEnumSymbols(IntPtr nameStrNative)
		{
			bool value = DbgHelp.SymEnumSymbols(m_process, m_dllBase,
			                                    m_maskStrNative, EnumSymProc, nameStrNative);

//			Conditions.NativeRequire(value);
		}


		private void SymEnumTypes(IntPtr nameStrNative)
		{
			bool value = DbgHelp.SymEnumTypes(m_process, m_dllBase, EnumSymProc, nameStrNative);
//			Conditions.NativeRequire(value);
		}

		public SymbolInfo* GetSymbol(string name)
		{
			var ctxStrNative = Marshal.StringToHGlobalAnsi(name);

			SymEnumSymbols(ctxStrNative);
			SymEnumTypes(ctxStrNative);

			Marshal.FreeHGlobal(ctxStrNative);

			SymbolInfo* sym = m_symbolBuffer;
			m_symbolBuffer = null;
			return sym;
		}

		public TDelegate GetFunction<TDelegate>(string name, string module) where TDelegate : Delegate
		{
			Pointer<byte> addr = GetSymAddress(name, module);
			return Functions.GetDelegateForFunctionPointer<TDelegate>(addr.Address);
		}

		public long GetSymOffset(string name)
		{
			if (Cache.ContainsKey(name)) {
				return Cache[name];
			}

			var ctxStrNative = Marshal.StringToHGlobalAnsi(name);

			SymEnumSymbols(ctxStrNative);
			SymEnumTypes(ctxStrNative);

			Marshal.FreeHGlobal(ctxStrNative);

			long offset = (m_addrBuffer - (int) m_base).ToInt64();
			m_symbolBuffer = default;
			CheckOffset(offset);

			Cache.Add(name, offset);
			return offset;
		}

		private void ReleaseUnmanagedResources()
		{
			Conditions.Require(DbgHelp.SymCleanup(m_process));
			Marshal.FreeHGlobal(m_imgStrNative);
			Marshal.FreeHGlobal(m_maskStrNative);
			m_process       = IntPtr.Zero;
			m_addrBuffer    = IntPtr.Zero;
			m_imgStrNative  = IntPtr.Zero;
			m_maskStrNative = IntPtr.Zero;
			m_symbolBuffer  = null;
		}

		public Pointer<byte> GetClrSymAddress(string name)
		{
			return GetSymAddress(name, Clr.CLR_DLL_SHORT);
		}


		public static Pointer<byte> GetSymAddress(string image, string module, string name)
		{
			using (var sym = new Symbols(image)) {
				Pointer<byte> addr = sym.GetSymAddress(name, module);
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
			Conditions.Require(symChk.Exists);

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
					string ln = stdOut.ReadLine();
					Conditions.NotNull(ln, nameof(ln));
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
				DirectoryInfo[] children = super.GetDirectories();
				Conditions.Assert(children.Length == 1);
				var child = children[0];

				// (possibly sole) file will be the symbol file
				FileInfo[] files = child.GetFiles();
				pdb = files.First(f => f.Name.Contains(Clr.CLR_PDB_SHORT));
			}
			else if (File.Exists(pdbStr)) {
				super = null;
				pdb   = new FileInfo(pdbStr);
			}
			else {
				throw new Exception(String.Format("Error downloading symbols. File: {0}", pdbStr));
			}

			Conditions.Ensure(pdb.Exists);
			return pdb;
		}


		#region Constructors

		public Symbols(string image, string mask, ulong @base, uint size)
		{
			m_process = Kernel32.GetCurrentProcess();

			m_base = @base;

			m_imgStrNative  = Marshal.StringToHGlobalAnsi(image);
			m_maskStrNative = Marshal.StringToHGlobalAnsi(mask);

			Conditions.Require(DbgHelp.SymInitialize(m_process, null, false));

			m_dllBase = DbgHelp.SymLoadModuleEx(m_process, IntPtr.Zero, m_imgStrNative,
			                                    IntPtr.Zero, m_base, size, IntPtr.Zero, 0);
			m_symbolBuffer = null;
			m_addrBuffer   = IntPtr.Zero;
		}

		public Symbols(string image) : this(image, MASK_STR_DEFAULT, BASE_DEFAULT, SIZE_DEFAULT) { }

		#endregion
	}
}