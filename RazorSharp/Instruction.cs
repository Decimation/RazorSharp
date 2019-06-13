using System.Reflection;
using System.Reflection.Emit;

namespace RazorSharp
{
	public struct Instruction
	{
		public int    Offset { get; internal set; }
		public OpCode OpCode { get; internal set; }
		public object Data   { get; internal set; }

		public bool IsMethodCall {
			get { return this.Data is MethodInfo; }
		}


		// todo
		public bool IsConstructorCall {
			get { return this.Data is MethodInfo; }
		}
		
		public override string ToString()
		{
			return string.Format("IL_{0:X}: {1} {2}", this.Offset, this.OpCode, Data);
		}
	}
}