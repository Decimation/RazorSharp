#region

using System;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable FieldCanBeMadeReadOnly.Local

#endregion

// ReSharper disable UnusedMember.Global
#pragma warning disable 649
#pragma warning disable 169

namespace Test.Testing.Types
{
	public unsafe class Dummy
	{
		private bool _bool;
		private byte _byte;

		private char _char;

		private DateTime _dateTime;
		private decimal  _decimal;
		private double   _double;

		private float    _float;
		private int      _int;
		private long     _long;
		private object   _object;
		private string[] _ptrTypeRg;
		private sbyte    _sbyte;
		private short    _short;

		private string _string;
		private uint   _uint;
		private ulong  _ulong;
		private ushort _ushort;

		private int[] _valTypeRg;

		private void* _voidptr;
		public Dummy() : this(new Random().Next(0, 100), "foo") { }

		private Dummy(int i, string s)
		{
			_int    = i;
			_string = s;
			_object = 0;

			// Value escapes the local scope but whatever jaja
			_voidptr  = &i;
			_dateTime = DateTime.Now;
		}

		public string String  => _string;
		public int    Integer => _int;

		public object Object {
			get => _object;
			set => _object = value;
		}

		public decimal Decimal {
			get => _decimal;
			set => _decimal = value;
		}

		public DateTime DateTime => _dateTime;

		public void Increment()
		{
			_int++;
		}

		public void echo()
		{
			Console.WriteLine("echo");
		}

		public void doSomething() { }

		public override string ToString()
		{
			return string.Format("int: {0} | string: {1}", _int, _string);
		}
	}
}