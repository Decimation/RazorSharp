#region

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using RazorCommon.Diagnostics;

#endregion

namespace RazorSharp.Memory
{
	/// <summary>
	///     <para>The Nasm assembler for x86/64.</para>
	///     <para>This code is from Squalr</para>
	/// </summary>
	public static class Assembler
	{
		private const string NASM_EXEC = @"C:\Lib\nasm.exe";


		/// <summary>
		///     Assemble the specified assembly code at a base address.
		/// </summary>
		/// <param name="assembly">The assembly code.</param>
		/// <param name="isProcess32Bit">Whether or not the assembly is in the context of a 32 bit program.</param>
		/// <param name="baseAddress">The address where the code is rebased.</param>
		/// <returns>An array of bytes containing the assembly code.</returns>
		public static byte[] Assemble(string assembly, bool isProcess32Bit, ulong baseAddress = 0)
		{
			Conditions.Require(File.Exists(NASM_EXEC),
			                   "Could not find nasm.exe", nameof(NASM_EXEC));

			// Note: Can't have PTR keyword

			string msg, innerMsg;
			byte[] bytes = null;

			string preamble = "org 0x" + baseAddress.ToString("X") + Environment.NewLine;

			if (isProcess32Bit) {
				preamble += "[BITS 32]" + Environment.NewLine;
			}
			else {
				preamble += "[BITS 64]" + Environment.NewLine;
			}

			assembly = preamble + assembly;

			try {
				string assemblyFilePath = Path.Combine(Path.GetTempPath(), "Assembly" + Guid.NewGuid() + ".asm");
				string outputFilePath   = Path.Combine(Path.GetTempPath(), "Assembly" + Guid.NewGuid() + ".bin");


				File.WriteAllText(assemblyFilePath, assembly);


				var startInfo = new ProcessStartInfo(NASM_EXEC)
				{
					Arguments              = "-f bin -o " + Escape(outputFilePath) + " " + Escape(assemblyFilePath),
					RedirectStandardError  = true,
					RedirectStandardOutput = true,
					UseShellExecute        = false,
					CreateNoWindow         = true
				};

				var process = Process.Start(startInfo);
				msg      = process.StandardOutput.ReadToEnd();
				innerMsg = process.StandardError.ReadToEnd();


				if (String.IsNullOrEmpty(msg) && !String.IsNullOrEmpty(innerMsg)) {
					msg = "NASM Compile error";
				}

				process.WaitForExit();


				if (File.Exists(outputFilePath)) {
					bytes = File.ReadAllBytes(outputFilePath);
				}

				File.Delete(assemblyFilePath);
				File.Delete(outputFilePath);
			}
			catch (Exception ex) {
				msg      = "Error compiling with NASM";
				innerMsg = ex.ToString();
			}


			innerMsg = Encoding.ASCII.GetString(Encoding.Unicode.GetBytes(innerMsg));
			return bytes;
		}


		private static string Escape(string str)
		{
			return String.Format("\"{0}\"", str);
		}
	}
}