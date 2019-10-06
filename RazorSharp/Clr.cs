#region

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using RazorSharp.CoreClr.Metadata;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Import;
using RazorSharp.Interop.Utilities;
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
		protected override string Id => nameof(Clr);

		/// <summary>
		///     CLR DLL file
		/// </summary>
		internal FileInfo LibraryFile { get; }

		/// <summary>
		///     <para>CLR symbol file</para>
		///     <para>A PDB file will be searched for in <see cref="RuntimeEnvironment.GetRuntimeDirectory"/></para>
		/// </summary>
		internal FileInfo SymbolsFile { get; }

		/// <summary>
		///     The <see cref="ProcessModule" /> of the CLR
		/// </summary>
		internal ProcessModule Module { get; }

		/// <summary>
		///     The <see cref="System.Version" /> of the CLR
		/// </summary>
		internal Version Version { get; }

		/// <summary>
		/// CLR symbol access.
		/// </summary>
		internal IImportProvider Imports { get; }
		
		
		private static FileInfo GetRuntimeFile(string fileName)
		{
			string path = RuntimeEnvironment.GetRuntimeDirectory() + fileName;
			var    file = new FileInfo(path);

			Conditions.Require(file.Exists);

			return file;
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
		/// </summary>
		private const string CLR_PDB_SHORT = "clr.pdb";

		#endregion

		#region Singleton

		/// <summary>
		///     Gets an instance of <see cref="Clr" />
		/// </summary>
		internal static Clr Value { get; private set; } = new Clr();

		private Clr()
		{
			// Version: 4.0.30319.42000
			// symchk "C:\Windows\Microsoft.NET\Framework\v4.0.30319\clr.dll" /s SRV*C:\Users\Deci\Desktop\clr.pdb*http://msdl.microsoft.com/download/symbols
			// symchk "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\clr.dll" /s SRV*C:\Users\Deci\Desktop\clr.pdb*http://msdl.microsoft.com/download/symbols
			
			LibraryFile = GetRuntimeFile(CLR_DLL_SHORT);
			SymbolsFile = GetRuntimeFile(CLR_PDB_SHORT);
			Module      = ModuleHelper.FindModule(CLR_DLL_SHORT);
			Version     = new Version(4, 0, 30319, 42000);
			Imports     = new ModuleImport(SymbolsFile, Module);

			Setup();
		}

		#endregion
	}
}