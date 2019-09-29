#region

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using RazorSharp.Import;
using RazorSharp.Memory;
using RazorSharp.Model;
using RazorSharp.Utilities;
using SimpleSharp.Diagnostics;

#endregion

namespace RazorSharp
{
	/// <summary>
	///     Contains resources for working with the CLR.
	/// </summary>
	internal sealed class Clr : Releasable
	{
		private const string CONTEXT = nameof(Clr);

		protected override string Id => CONTEXT;

		/// <summary>
		///     CLR DLL file
		/// </summary>
		internal FileInfo ClrDll { get; }

		/// <summary>
		///     The <see cref="ProcessModule" /> of the CLR
		/// </summary>
		internal ProcessModule ClrModule { get; }

		/// <summary>
		///     The <see cref="Version" /> of the CLR
		/// </summary>
		internal Version ClrVersion { get; }

		/// <summary>
		/// CLR symbol access.
		/// </summary>
		internal ModuleImport ClrSymbols { get; }

		/// <summary>
		///     <para>CLR symbol file</para>
		///     <para>A PDB file will be searched for in <see cref="CLR_PDB_FILE_SEARCH" />;</para>
		///     <para>otherwise the symbol file will be automatically downloaded</para>
		/// </summary>
		internal FileInfo ClrPdb { get; }
		
		// symchk "C:\Windows\Microsoft.NET\Framework\v4.0.30319\clr.dll" /s SRV*C:\Users\Deci\Desktop\clr.pdb*http://msdl.microsoft.com/download/symbols
		// symchk "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\clr.dll" /s SRV*C:\Users\Deci\Desktop\clr.pdb*http://msdl.microsoft.com/download/symbols

		/// <summary>
		///     CLR symbol file name
		/// </summary>
		internal static string ClrPdbShort { get; } = Mem.Is64Bit ? CLR_PDB_SHORT : CLR32_PDB_SHORT;

		/// <summary>
		///     CLR symbol file path
		/// </summary>
		internal static string ClrPdbFileSearch { get; } = Mem.Is64Bit ? CLR_PDB_FILE_SEARCH : CLR32_PDB_FILE_SEARCH;

		private static FileInfo GetClrDll()
		{
			string clrPath = RuntimeEnvironment.GetRuntimeDirectory() + CLR_DLL_SHORT;
			var    clr     = new FileInfo(clrPath);

			Conditions.Require(clr.Exists);

			return clr;
		}

		private static FileInfo GetClrSymbolFile()
		{
			var clrSym = new FileInfo(ClrPdbFileSearch);

			if (!clrSym.Exists) {
				string msg = String.Format("Clr symbol file not found. Put \"{0}\" in \"{1}\".",
				                           ClrPdbShort, ClrPdbFileSearch);
				throw new FileNotFoundException(msg);
			}

			Global.Value.WriteDebug(CONTEXT, "Clr symbol file: {File}", clrSym.FullName);

			return clrSym;
		}

		public override void Close()
		{
			// Delete instance
			Value = null;

			base.Close();
		}

		#region Constants

		/// <summary>
		///     CLR module file: <c>clr.dll</c>
		/// </summary>
		private const string CLR_DLL_SHORT = "clr.dll";
		
		/// <summary>
		///     CLR symbol file name: <c>clr.pdb</c>
		///     <remarks>x64</remarks>
		/// </summary>
		private const string CLR_PDB_SHORT = "clr.pdb";

		/// <summary>
		///     CLR symbol file name: <c>clr32.pdb</c>
		///     <remarks>x86</remarks>
		/// </summary>
		private const string CLR32_PDB_SHORT = "clr32.pdb";
		
		/// <summary>
		///     CLR symbol path
		///     <remarks>x64</remarks>
		/// </summary>
		private const string CLR_PDB_FILE_SEARCH = @"C:\Symbols\clr.pdb";

		/// <summary>
		///     CLR symbol path
		///     <remarks>x86</remarks>
		/// </summary>
		private const string CLR32_PDB_FILE_SEARCH = @"C:\Symbols\clr32.pdb";

		#endregion

		#region Singleton

		/// <summary>
		///     Gets an instance of <see cref="Clr" />
		/// </summary>
		internal static Clr Value { get; private set; } = new Clr();

		private Clr()
		{
			ClrDll     = GetClrDll();
			ClrModule  = ModuleUtil.GetModule(CLR_DLL_SHORT);
			ClrVersion = new Version(4, 0, 30319, 42000);

			if (ClrPdb == null) {
				ClrPdb = GetClrSymbolFile();
			}
			else {
				Conditions.Require(ClrPdb.Exists);
			}

			ClrSymbols = new ModuleImport(ClrPdb, ClrModule);
			
			Setup();
		}

		#endregion
	}
}