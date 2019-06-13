using System;
using System.Reflection;
using System.Reflection.Emit;
using SimpleSharp.Strings;

namespace RazorSharp.CoreClr
{
	public struct Instruction
	{
		public int    Offset { get; internal set; }
		public OpCode OpCode { get; internal set; }
		public object Data   { get; internal set; }

		public bool IsMethodCall => Data is MethodInfo;
		
		// todo
		public bool IsConstructorCall => Data is MethodInfo m && m.IsConstructor;

		public override string ToString()
		{
			string dataString;

			if (Data != null) {
				if (!Hex.TryCreateHex(Data, out dataString)) {
					dataString = Data.ToString();
				}
			}
			else {
				dataString = String.Empty;
			}


			return String.Format("IL_{0:X}: {1} {2}", Offset, OpCode, dataString);
		}
	}
}