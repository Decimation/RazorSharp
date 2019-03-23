using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using RazorCommon.Extensions;
using RazorCommon.Utilities;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.EE;
using RazorSharp.CoreClr.Structures.HeapObjects;
using RazorSharp.Memory;
using RazorSharp.Native;
using RazorSharp.Utilities;

namespace RazorSharp.CoreClr
{
	public static class Clr
	{
		public static bool IsSetup { get; private set; }

		private static readonly Type[] ClrTypes =
		{
			typeof(FieldDesc), typeof(MethodDesc), typeof(MethodDescChunk), typeof(MethodTable),
			typeof(ArrayObject), typeof(HeapObject), typeof(StringObject), typeof(EEClass)
		};

		private static readonly Type[] ClrTypes2 =
		{
			typeof(MethodDesc), typeof(FieldDesc), typeof(ClrFunctions), typeof(GCHeap)
		};

		public static FileInfo ClrPdb { get; set; }
		
		/// <summary>
		/// Whether or not <see cref="ClrPdb"/> was automatically downloaded.
		/// If <c>true</c>, <see cref="ClrPdb"/> will be deleted upon calling <see cref="Close"/>.
		/// </summary>
		public static bool IsPdbTemporary { get; private set; }

		internal static readonly FileInfo      ClrDll;
		internal static readonly ProcessModule ClrModule;
		internal static readonly Version       ClrVersion;

		/// <summary>
		/// <c>clr.pdb</c>
		/// </summary>
		internal const string CLR_PDB_SHORT = "clr.pdb";

		/// <summary>
		///     <c>clr.dll</c>
		/// </summary>
		internal const string CLR_DLL_SHORT = "clr.dll";


		static Clr()
		{
			ClrDll     = GetClrDll();
			ClrModule  = Modules.GetModule(Clr.CLR_DLL_SHORT);
			ClrVersion = new Version(4, 0, 30319, 42000);
		}


		/// <summary>
		/// Matches <see cref="ClrPdb"/> with the symbol file returned by Microsoft's servers
		/// </summary>
		public static void CheckSymbolIntegrity()
		{
			// Good lord this problem took 3 hours to solve
			// Turns out the pdb file was just out of date lmao

			const string ERR = "The PDB specified by \"ClrPdb\" does not match the one returned by Microsoft's servers";
			Conditions.RequiresNotNull(ClrPdb, nameof(ClrPdb));
			string cd = Environment.CurrentDirectory;
			var tmpSym = Symbols.DownloadSymbolFile(new DirectoryInfo(cd), ClrDll);
			Conditions.Requires(ClrPdb.ContentEquals(tmpSym), ERR);
			DeleteSymbolFile(tmpSym);
		}
		
		private static FileInfo GetClrSymbolFile()
		{
			FileInfo clrSym = null;
			string   cd     = Environment.CurrentDirectory;

			string[] dirs = {cd, Environment.SystemDirectory};
			foreach (string dir in dirs) {
				var fi = FileUtil.FindFile(dir, CLR_PDB_SHORT);
				if (fi != null) {
					clrSym = fi;
					break;
				}
			}

			if (clrSym == null) {
				clrSym = Symbols.DownloadSymbolFile(new DirectoryInfo(cd), ClrDll);
			}

			Global.Log.Debug("Clr symbol file: {File}", clrSym.FullName);

			return clrSym;
		}

		private static FileInfo GetClrDll()
		{
			string clrPath = RuntimeEnvironment.GetRuntimeDirectory() + CLR_DLL_SHORT;
			var    clr     = new FileInfo(clrPath);
			Conditions.RequiresFileExists(clr);
			return clr;
		}

		internal static void Setup()
		{
			if (ClrPdb == null) {
				ClrPdb = GetClrSymbolFile();
				IsPdbTemporary = true;
			}
			else {
				Conditions.RequiresFileExists(ClrPdb);
				IsPdbTemporary = false;
			}

			
			IsSetup = true;
		}

		

		/// <summary>
		/// Deletes an automatically downloaded pdb file
		/// </summary>
		private static void DeleteSymbolFile(FileInfo pdb)
		{
			// Delete temporarily downloaded symbol files
			Conditions.RequiresNotNull(pdb.Directory, nameof(pdb.Directory));

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
		
		public static void Close()
		{
			if (!IsSetup) {
				return;
			}

			// This won't delete the symbol file if it wasn't manually downloaded
			// but we'll make sure anyway
			if (IsPdbTemporary)
				DeleteSymbolFile(ClrPdb);

			IsSetup = false;
			
		}
	}
}