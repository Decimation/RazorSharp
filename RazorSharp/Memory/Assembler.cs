using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace RazorSharp.Memory
{
	/// <summary>
	/// The Nasm assembler for x86/64.
	/// </summary>
	internal class Assembler
	{
		/// <summary>
		/// Assemble the specified assembly code.
		/// </summary>
		/// <param name="assembly">The assembly code.</param>
		/// <param name="isProcess32Bit">Whether or not the assembly is in the context of a 32 bit program.</param>
		/// <returns>An array of bytes containing the assembly code.</returns>
		public byte[] Assemble(string assembly, bool isProcess32Bit)
		{
			// Assemble and return the code
			return Assemble(assembly, isProcess32Bit, 0);
		}

		/// <summary>
		/// The path to the nasm binary. This is searched for recursively and cached. This is done since NuGet can move the relative location of the file.
		/// </summary>
		private readonly Lazy<string> m_nasmPath = new Lazy<string>(() => @"C:\Lib\nasm.exe",
		                                                            LazyThreadSafetyMode.ExecutionAndPublication
		);


		/// <summary>
		/// Assemble the specified assembly code at a base address.
		/// </summary>
		/// <param name="assembly">The assembly code.</param>
		/// <param name="isProcess32Bit">Whether or not the assembly is in the context of a 32 bit program.</param>
		/// <param name="baseAddress">The address where the code is rebased.</param>
		/// <returns>An array of bytes containing the assembly code.</returns>
		public byte[] Assemble(string assembly, bool isProcess32Bit, ulong baseAddress)
		{
			// Can't have PTR keyword
			
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
				string assemblyFilePath = Path.Combine(Path.GetTempPath(), "SqualrAssembly" + Guid.NewGuid() + ".asm");
				string outputFilePath   = Path.Combine(Path.GetTempPath(), "SqualrAssembly" + Guid.NewGuid() + ".bin");


				File.WriteAllText(assemblyFilePath, assembly);

				string exePath = m_nasmPath.Value;

				var startInfo = new ProcessStartInfo(exePath)
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


			Console.WriteLine(msg);
			Console.WriteLine(Encoding.ASCII.GetString(Encoding.Unicode.GetBytes(innerMsg)));
			return bytes;
		}


		private static string Escape(string str)
		{
			return String.Format("\"{0}\"", str);
		}
	}
}