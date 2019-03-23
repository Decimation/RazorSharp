using System;
using System.Text;

namespace RazorSharp.Native
{
	// todo: WIP
	public struct NativeInteger
	{
		private int m_value;

		public static OutputAs DefaultOutput { get; set; }

		static NativeInteger()
		{
			DefaultOutput = OutputAs.Integer | OutputAs.Bool;
		}

		public int Value {
			get => m_value;
			set => m_value = value;
		}


		public bool Bool {
			get => AsBool(m_value);
			set => m_value = AsInt(value);
		}

		private static int  AsInt(bool b) => b ? 1 : 0;
		private static bool AsBool(int i) => i != 0;


		public NativeInteger(int i)
		{
			m_value = i;
		}

		public NativeInteger(bool b)
		{
			m_value = AsInt(b);
		}

		public static implicit operator NativeInteger(int i) => new NativeInteger(i);

		public static implicit operator NativeInteger(bool b) => new NativeInteger(b);

		public static NativeInteger operator +(NativeInteger x, NativeInteger y) => x.m_value + y.m_value;

		public static NativeInteger operator *(NativeInteger x, NativeInteger y) => x.m_value * y.m_value;

		public static NativeInteger operator -(NativeInteger x, NativeInteger y) => x.m_value - y.m_value;

		public static NativeInteger operator /(NativeInteger x, NativeInteger y) => x.m_value / y.m_value;

		public static NativeInteger operator |(NativeInteger x, NativeInteger y) => x.m_value | y.m_value;

		public static NativeInteger operator &(NativeInteger x, NativeInteger y) => x.m_value & y.m_value;

		public static NativeInteger operator ^(NativeInteger x, NativeInteger y) => x.m_value ^ y.m_value;
		
		public static NativeInteger operator %(NativeInteger x, NativeInteger y) => x.m_value % y.m_value;

		public static NativeInteger operator >>(NativeInteger x, int y) => x.m_value >> y;

		public static NativeInteger operator <<(NativeInteger x, int y) => x.m_value << y;

		public static NativeInteger operator ~(NativeInteger x) => ~x.m_value;


		public static bool operator true(NativeInteger x) => x.Bool; //todo

		public static bool operator false(NativeInteger x) => x.Bool; //todo

		public static bool operator !(NativeInteger x) => !x.Bool;

		public static explicit operator bool(NativeInteger x) => x.Bool;
		public static explicit operator int(NativeInteger  x) => x.m_value;

		public override string ToString()
		{
			if (DefaultOutput == (OutputAs.Integer | OutputAs.Bool)) {
				return string.Format("{0} ({1})", m_value, Bool);
			}


			switch (DefaultOutput) {
				case OutputAs.Integer:
					return m_value.ToString();
					break;
				case OutputAs.Bool:
					return Bool.ToString();
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}