using System;

namespace RazorSharp.CLR.Structures
{

	public unsafe class MethodDescription
	{
		private readonly MethodDesc* m_methodDesc;


		public IntPtr Function => m_methodDesc->Function;
		public string Name => m_methodDesc->Name;
	}

}