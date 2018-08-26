#region

using System;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable FieldCanBeMadeReadOnly.Local

#endregion

// ReSharper disable UnusedMember.Global
#pragma warning disable 649
#pragma warning disable 169

namespace Test.Testing
{

	public unsafe class Dummy
	{
		private byte   _byte;
		private sbyte  _sbyte;
		private ushort _ushort;
		private short  _short;
		private uint   _uint;
		private int    _int;
		private ulong  _ulong;
		private long   _long;

		private char _char;
		private bool _bool;

		private float   _float;
		private double  _double;
		private decimal _decimal;

		private string _string;
		private object _object;

		private void* _voidptr;

		private DateTime _dateTime;

		public String String  => _string;
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
			return String.Format("int: {0} | string: {1}", _int, _string);
		}
	}


}