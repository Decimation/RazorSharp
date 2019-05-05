using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SimpleSharp.Diagnostics;
using SimpleSharp.Utilities;
using RazorSharp.CoreClr;
using RazorSharp.Native.Win32;
using SimpleSharp.Extensions;

// ReSharper disable UnusedMember.Local

namespace RazorSharp.Native.Symbols
{
	internal static class SymbolUtil
	{
		private const string MASK_STR_DEFAULT = "*!*";
		
		internal static FileInfo DownloadSymbolFile(DirectoryInfo dest, FileInfo dll)
		{
			return DownloadSymbolFile(dest, dll, out _);
		}

		internal static Task<FileInfo> DownloadSymbolFileAsync(DirectoryInfo dest, FileInfo dll)
		{
			return new Task<FileInfo>(() => DownloadSymbolFile(dest, dll));
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

					
				}
			}

			Global.Log.Debug("Done downloading symbols");

			string   pdbStr = Path.Combine(dest.FullName, Clr.CLR_PDB_SHORT);
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

		internal static unsafe bool GetFileSize(string pFileName, ref ulong fileSize)
		{
			var hFile = Kernel32.CreateFile(pFileName,
			                                FileAccess.Read,
			                                FileShare.Read,
			                                IntPtr.Zero,
			                                FileMode.Open,
			                                0,
			                                IntPtr.Zero);


			if (hFile == Kernel32.INVALID_HANDLE_VALUE) {
				return false;
			}

			fileSize = Kernel32.GetFileSize(hFile, null);

			Conditions.Ensure(Kernel32.CloseHandle(hFile));

			if (fileSize == Kernel32.INVALID_FILE_SIZE) {
				return false;
			}


			return fileSize != Kernel32.INVALID_FILE_SIZE;
		}

		internal static bool GetFileParams(string pFileName, ref ulong baseAddr, ref ulong fileSize)
		{
			// Is it .PDB file ?

			if (pFileName.Contains("pdb")) {
				// Yes, it is a .PDB file 

				// Determine its size, and use a dummy base address 

				baseAddr = 0x10000000; // it can be any non-zero value, but if we load symbols 
				// from more than one file, memory regions specified
				// for different files should not overlap
				// (region is "base address + file size")

				if (!SymbolUtil.GetFileSize(pFileName, ref fileSize)) {
					return false;
				}
			}
			else {
				// It is not a .PDB file 

				// Base address and file size can be 0 

				baseAddr = 0;
				fileSize = 0;

				throw new NotImplementedException();
			}

			return true;
		}

		/// <summary>
		///     Deletes an automatically downloaded pdb file
		/// </summary>
		internal static void DeleteSymbolFile(FileInfo pdb)
		{
			// Delete temporarily downloaded symbol files
			Conditions.NotNull(pdb.Directory, nameof(pdb.Directory));

			// This (should) equal IsPdbTemporary
			if (pdb.Directory.FullName.Contains(Environment.CurrentDirectory)) {
				Global.Log.Debug("Deleting temporary PDB file");

				var files = new FileSystemInfo[] {pdb, pdb.Directory, pdb.Directory.Parent};

				foreach (var file in files) {
					file.Delete();
					file.Refresh();
					Conditions.Assert(!file.Exists);
				}
			}
		}

		internal static void CheckSymbolIntegrity(FileInfo pdb, FileInfo dll)
		{
			// Good lord this problem took 3 hours to solve
			// Turns out the pdb file was just out of date lmao

			const string ERR = "The PDB specified by \"" + nameof(pdb) +
			                   "\" does not match the one returned by Microsoft's servers";

			Conditions.NotNull(pdb, nameof(pdb));
			string cd     = Environment.CurrentDirectory;
			var    tmpSym = SymbolUtil.DownloadSymbolFile(new DirectoryInfo(cd), dll);
			Conditions.Ensure(pdb.ContentEquals(tmpSym), ERR, nameof(pdb));
			SymbolUtil.DeleteSymbolFile(tmpSym);
		}
	}
}