#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using RazorSharp.Components;
using RazorSharp.Memory;
using RazorSharp.Native.Symbols;
using SimpleSharp.Diagnostics;
using SimpleSharp.Utilities;

#endregion

namespace RazorSharp.CoreClr
{
	/// <summary>
	///     Contains resources for working with the CLR.
	/// </summary>
	public sealed class Clr : Releasable
	{
		/// <summary>
		///     Whether or not <see cref="ClrPdb" /> was automatically downloaded.
		///     If <c>true</c>, <see cref="ClrPdb" /> will be deleted upon calling <see cref="Close" />.
		/// </summary>
		private readonly bool m_isPdbTemporary;

		/// <summary>
		///     CLR dll file
		/// </summary>
		public FileInfo ClrDll { get; }

		/// <summary>
		///     The <see cref="ProcessModule" /> of the CLR
		/// </summary>
		public ProcessModule ClrModule { get; }

		/// <summary>
		///     The <see cref="Version" /> of the CLR
		/// </summary>
		public Version ClrVersion { get; }

		public ModuleInfo ClrSymbols { get; }

		/// <summary>
		///     <para>CLR symbol file</para>
		///     <para>A PDB file will be searched for in <see cref="CLR_PDB_FILE_SEARCH" />;</para>
		///     <para>otherwise the symbol file will be automatically downloaded</para>
		/// </summary>
		public FileInfo ClrPdb { get; }

		private static FileInfo GetClrDll()
		{
			string clrPath = RuntimeEnvironment.GetRuntimeDirectory() + CLR_DLL_SHORT;
			var    clr     = new FileInfo(clrPath);
			Conditions.Require(clr.Exists);
			return clr;
		}

		private FileInfo GetClrSymbolFile()
		{
			string   cd   = Environment.CurrentDirectory;
			string[] dirs = {cd, Environment.SystemDirectory};

			var clrSym = dirs.Select(dir => FileUtil.FindFile(dir, CLR_PDB_SHORT))
			                 .FirstOrDefault(fi => fi != null) ?? (File.Exists(CLR_PDB_FILE_SEARCH)
				             ? new FileInfo(CLR_PDB_FILE_SEARCH)
				             : SymbolUtil.DownloadSymbolFile(new DirectoryInfo(cd), ClrDll));

			Global.Log.Debug("Clr symbol file: {File}", clrSym.FullName);

			return clrSym;
		}

		public override void Close()
		{
			// This won't delete the symbol file if it wasn't manually downloaded
			// but we'll make sure anyway
			if (m_isPdbTemporary)
				SymbolUtil.DeleteSymbolFile(ClrPdb);

			// Delete instance
			Value = null;

			base.Close();
		}

		#region Constants

		/// <summary>
		///     <c>clr.pdb</c>
		/// </summary>
		internal const string CLR_PDB_SHORT = "clr.pdb";

		/// <summary>
		///     <c>clr.dll</c>
		/// </summary>
		internal const string CLR_DLL_SHORT = "clr.dll";

		private const string CLR_PDB_FILE_SEARCH = @"C:\Symbols\clr.pdb";

		#endregion

		#region Singleton

		/// <summary>
		///     Gets an instance of <see cref="Clr" />
		/// </summary>
		public static Clr Value { get; private set; } = new Clr();

		private Clr()
		{
			ClrDll     = GetClrDll();
			ClrModule  = Modules.GetModule(CLR_DLL_SHORT);
			ClrVersion = new Version(4, 0, 30319, 42000);

			if (ClrPdb == null) {
				ClrPdb           = GetClrSymbolFile();
				m_isPdbTemporary = true;
			}
			else {
				Conditions.Require(ClrPdb.Exists);
				m_isPdbTemporary = false;
			}

			ClrSymbols = new ModuleInfo(ClrPdb, ClrModule);
		}

		#endregion
	}
}