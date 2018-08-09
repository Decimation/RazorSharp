#region

using RazorSharp.Runtime;

#endregion

namespace RazorSharp
{

	public unsafe struct LPCUTF8
	{
		private byte* m_value;

		public int Length {
			get { return Value.Length; }
		}

		public string Value {
			get { return CLRFunctions.StringFunctions.NewString(m_value); }
		}

		public override string ToString()
		{
			return Value;
		}
	}

}