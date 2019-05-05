#region

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using SimpleSharp.Diagnostics;
using SimpleSharp.Extensions;
using SimpleSharp.Utilities;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Symbols;

#endregion

namespace RazorSharp.CoreClr
{
	/// <summary>
	///     Contains the resources for working with the CLR.
	/// </summary>
	internal static class Clr
	{
		/// <summary>
		///     Retrieves resources
		/// </summary>
		static Clr()
		{
			ClrDll     = GetClrDll();
			ClrModule  = Modules.GetModule(CLR_DLL_SHORT);
			ClrVersion = new Version(4, 0, 30319, 42000);


			if (ClrPdb == null) {
				ClrPdb         = GetClrSymbolFile();
				IsPdbTemporary = true;
			}
			else {
				Conditions.Require(ClrPdb.Exists);
				IsPdbTemporary = false;
			}

			ClrSymbols = new ModuleInfo(ClrPdb, ClrModule);
		}


		private static FileInfo GetClrSymbolFile()
		{
			FileInfo clrSym = null;
			string   cd     = Environment.CurrentDirectory;
			string[] dirs   = {cd, Environment.SystemDirectory};

			foreach (string dir in dirs) {
				var fi = FileUtil.FindFile(dir, CLR_PDB_SHORT);
				if (fi != null) {
					clrSym = fi;
					break;
				}
			}

			if (clrSym == null) {
				if (File.Exists(CLR_PDB_FILE_SEARCH)) {
					clrSym = new FileInfo(CLR_PDB_FILE_SEARCH);
				}
				else {
					clrSym = SymbolUtil.DownloadSymbolFile(new DirectoryInfo(cd), ClrDll);
				}
			}

			Global.Log.Debug("Clr symbol file: {File}", clrSym.FullName);

			return clrSym;
		}

		private static FileInfo GetClrDll()
		{
			string clrPath = RuntimeEnvironment.GetRuntimeDirectory() + CLR_DLL_SHORT;
			var    clr     = new FileInfo(clrPath);
			Conditions.Require(clr.Exists);
			return clr;
		}


		internal static void Setup()
		{
			IsSetup = true;
			
		}


		internal static void Close()
		{
			Conditions.Require(IsSetup);

			// This won't delete the symbol file if it wasn't manually downloaded
			// but we'll make sure anyway
			if (IsPdbTemporary)
				SymbolUtil.DeleteSymbolFile(ClrPdb);

			IsSetup = false;
		}


		#region Constants and accessors

		/// <summary>
		///     <c>clr.pdb</c>
		/// </summary>
		internal const string CLR_PDB_SHORT = "clr.pdb";

		/// <summary>
		///     <c>clr.dll</c>
		/// </summary>
		internal const string CLR_DLL_SHORT = "clr.dll";

		/// <summary>
		///     CLR dll file
		/// </summary>
		internal static readonly FileInfo ClrDll;

		/// <summary>
		///     The <see cref="ProcessModule" /> of the CLR
		/// </summary>
		internal static readonly ProcessModule ClrModule;

		/// <summary>
		///     The <see cref="Version" /> of the CLR
		/// </summary>
		internal static readonly Version ClrVersion;

		internal static bool IsSetup { get; private set; }

		/// <summary>
		///     Whether or not <see cref="ClrPdb" /> was automatically downloaded.
		///     If <c>true</c>, <see cref="ClrPdb" /> will be deleted upon calling <see cref="Close" />.
		/// </summary>
		private static bool IsPdbTemporary { get; set; }

		/// <summary>
		///     <para>CLR symbol file</para>
		///     <para>A PDB file will be searched for in <see cref="CLR_PDB_FILE_SEARCH" />;</para>
		///     <para>otherwise the symbol file will be automatically downloaded</para>
		/// </summary>
		internal static readonly FileInfo ClrPdb;

		internal static readonly ModuleInfo ClrSymbols;

		private const string CLR_PDB_FILE_SEARCH = @"C:\Symbols\clr.pdb";

		#endregion
	}
}