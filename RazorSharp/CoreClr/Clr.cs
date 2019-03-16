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


		private static FileInfo GetClrSymbolFile()
		{
			FileInfo clrSym = null;
			string   cd     = Environment.CurrentDirectory;

			string[] dirs = {cd, Environment.SystemDirectory};
			foreach (string dir in dirs) {
				var fi = FileUtil.FindFile(dir, Clr.CLR_PDB_SHORT);
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
			if (ClrPdb == null)
				ClrPdb = GetClrSymbolFile();

			Conditions.RequiresFileExists(ClrPdb);
			IsSetup = true;
		}

		internal static void Reorganize()
		{
			foreach (var type in ClrTypes) {
				Memory.Structures.ReorganizeAuto(type);
			}
		}

		public static void Close()
		{
			if (!IsSetup) {
				return;
			}

			// Delete temporarily downloaded symbol files
			Conditions.RequiresNotNull(ClrPdb.Directory, nameof(ClrPdb.Directory));

			if (ClrPdb.Directory.FullName.Contains(Environment.CurrentDirectory)) {
				Global.Log.Debug("Deleting temporary PDB file");

				var files = new FileSystemInfo[] {ClrPdb, ClrPdb.Directory, ClrPdb.Directory.Parent};

				foreach (var file in files) {
					file.Delete();
					file.Refresh();
					Conditions.Assert(!file.Exists);
				}
			}

			IsSetup = false;
		}
	}
}