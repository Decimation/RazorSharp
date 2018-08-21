#region

using System;
using System.Diagnostics;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.Pointers;

#endregion

namespace Test.Testing.Tests
{

	[TestFixture]
	public class PointerTests2
	{
		private struct Target
		{
			private readonly string _str;
			private readonly int    _int;

			public string Str => _str;
			public int    I   => _int;

			public bool Equals(Target other)
			{
				return string.Equals(_str, other._str) && _int == other._int;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) {
					return false;
				}

				return obj is Target other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked {
					return ((_str != null ? _str.GetHashCode() : 0) * 397) ^ _int;
				}
			}

			public static bool operator ==(Target left, Target right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(Target left, Target right)
			{
				return !left.Equals(right);
			}

			public Target(string str, int i)
			{
				_str = str;
				_int = i;
			}
		}

		[Test]
		public void Test()
		{
			Target        t   = new Target("foo", 123);
			Pointer<byte> ptr = Unsafe.AddressOf(ref t);

			Debug.Assert(ptr.Read<string>() == t.Str);
			Debug.Assert(ptr.Read<int>(2) == t.I);

			ptr += IntPtr.Size;
			Debug.Assert(ptr.Read<int>() == t.I);

			ptr -= IntPtr.Size;

			Pointer<string> lpStr = ptr.Reinterpret<string>();
			Debug.Assert(lpStr.Reference == t.Str);
			Debug.Assert(lpStr.Value == t.Str);
			Debug.Assert(lpStr[0] == t.Str);

			lpStr++;
			Debug.Assert(lpStr.Read<int>() == t.I);
			lpStr--;
			lpStr.Write("bar");
			Debug.Assert(lpStr.Reference == t.Str);

			Debug.Assert(lpStr == Unsafe.AddressOfField(ref t, "_str"));

			Pointer<int> lpInt32 = lpStr.Reinterpret<int>();
			lpInt32 += 2;
			Debug.Assert(lpInt32 == Unsafe.AddressOfField(ref t, "_int"));
			lpInt32[0] = 321;
			Debug.Assert(lpInt32.Reference == t.I);
			Debug.Assert(lpInt32.Value == t.I);
			Debug.Assert(lpInt32[0] == t.I);

			Pointer<Target> lpTarget = ptr.Reinterpret<Target>();
			lpTarget.Increment();
			lpTarget.Decrement();
			lpTarget.Add(lpTarget.ElementSize);
			lpTarget.Subtract(lpTarget.ElementSize);
			Debug.Assert(lpTarget[0] == t);
			Debug.Assert(lpTarget.Reference == t);
			Debug.Assert(lpTarget.Value == t);
		}
	}

}