using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using RazorCommon.Extensions;
using RazorCommon.Utilities;

namespace RazorSharp.Native.Symbols
{
	// todo: WIP
	
	/// <summary>
	/// Uses <see cref="CVDUMP_EXE"/> to read symbol information
	/// </summary>
	public class CvPdbParser
	{
		private readonly FileInfo m_pdb;

		private const string CVDUMP_EXE = "cvdump";


		public CvPdbParser(FileInfo pdb)
		{
			m_pdb = pdb;
		}

		private StringBuilder ParseSymbols()
		{
			// (000004) S_GPROC32: [0001:0017D9C0], Cb: 00000043, Type:     T_NOTYPE(0000), SString::SString
			//          Parent: 00000000, End: 0000003C, Next: 00000000
			//          Debug start: 00000013, Debug end: 0000003D
			//          Flags: Optimized Debug Info
			// 
			// (00003C) S_END
			
			
			
			return null;
		}
		
		public StringBuilder ReadOutput(string k)
		{
			var sb = new StringBuilder();
			
			using (var proc = Common.Shell(CVDUMP_EXE + " " + k + " " + m_pdb.FullName)) {
				proc.Start();

				var stdOut = proc.StandardOutput;

				while (!stdOut.EndOfStream) {
					sb.Append(stdOut.ReadToEnd());
				}
			}

			return sb;
		}

		[DebuggerDisplay("Buffer: {" + nameof(m_buffer) + "}")]
		class Tokenizer
		{
			private string m_buffer;

			public Tokenizer(StringBuilder sb)
			{
				m_buffer = sb.ToString();
			}

			public void SeekUntil(string c)
			{
				var index = m_buffer.IndexOf(c);
				m_buffer = m_buffer.Substring(index);
			}

			public (string Name, string Value) ReadField()
			{
				var name  = ReadUntil(":");
				var value = ReadUntil(",");
				return (name, value);
			}

			public string ReadUntil(string c)
			{
				var open  = m_buffer.IndexOf(c);
				var value = m_buffer.JSubstring(0, open);

				m_buffer = m_buffer.SubstringAfter(value + c).TrimStart(' ');

				return value;
			}

			public string ReadEnclosed()
			{
				var open  = m_buffer.IndexOf('(');
				var close = m_buffer.IndexOf(')', open);

				var enclosed = m_buffer.JSubstring(open + 1, close);

				m_buffer = m_buffer.Substring(close + 1 + 1);

				return enclosed;
			}
		}

		public void ReadSymbol()
		{
			
		}
	}
}