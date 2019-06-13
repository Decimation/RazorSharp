using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RazorSharp
{
	public class OpCodeTranslator
	{
		private static Dictionary<short, OpCode> _opCodes = new Dictionary<short, OpCode>();

		static OpCodeTranslator()
		{
			Initialize();
		}


		public static OpCode GetOpCode(short value)
		{
			return _opCodes[value];
		}
		
		public static int GetInt32(byte[] bytes, int index)
		{
			return
				bytes[index + 0] |
				bytes[index + 1] << 8 |
				bytes[index + 2] << 16 |
				bytes[index + 3] << 24;
		}


		private static void Initialize()
		{
			foreach (FieldInfo fieldInfo in typeof(OpCodes).GetFields())
			{
				OpCode opCode = (OpCode)fieldInfo.GetValue(null);

				_opCodes.Add(opCode.Value, opCode);
			}
		}
	}
}