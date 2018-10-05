using System;
using RazorSharp.Common;
using RazorSharp.Native.Structures;

namespace RazorSharp.Native
{

	public class StackCall
	{
		public int    ThreadId   { get; private set; }
		public ulong  AddrPC     { get; private set; }
		public ulong  AddrReturn { get; private set; }
		public string Symbol     { get; private set; }
		public string MappedFile { get; private set; }

		public StackCall(IntPtr hProcess, ulong addrPc, ulong addrReturn, int threadId)
		{
			this.ThreadId   = threadId;
			this.AddrPC     = addrPc;
			this.AddrReturn = addrReturn;

			System.Text.StringBuilder returnedString = new System.Text.StringBuilder(256);

			IntPtr pcOffset = (IntPtr)Functions.UlongToLong(addrPc);
			Psapi.GetMappedFileNameW(hProcess, pcOffset, returnedString, (uint)returnedString.Capacity);
			this.MappedFile = returnedString.ToString();

			IMAGEHLP_SYMBOL64 pcSymbol = Functions.GetSymbolFromAddress(hProcess, addrPc);
			this.Symbol = new string(pcSymbol.Name);
		}

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Id", ThreadId);
			table.AddRow("AddrPC", Hex.ToHex(AddrPC));
			table.AddRow("AddrReturn", Hex.ToHex(AddrReturn));
			table.AddRow("Symbol", Symbol);
			table.AddRow("MappedFile", MappedFile);
			return table.ToMarkDownString();
		}
	}

}