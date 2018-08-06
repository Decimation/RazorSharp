using System;
using System.Diagnostics;
using NUnit.Framework;
using RazorSharp;

namespace Test.Testing.Tests
{

	[TestFixture]
	internal class OffsetOfTests
	{
		[Test]
		public void Test()
		{
			Dummy d = new Dummy();

			assertOffset<object>(0, 8);
			assertOffset(0UL, 16);
			assertOffset(0L, 24);
			assertOffset(0D, 32);

			//assertOffset<Void>(null, 40);
			assertOffset(0U, 48);
			assertOffset(d.Integer, 52);
			assertOffset(0F, 56);
			assertOffset<ushort>(0, 60);
			assertOffset<short>(0, 62);
			assertOffset('\0', 64);
			assertOffset<byte>(0, 66);
			assertOffset<sbyte>(0, 67);
			assertOffset<bool>(false, 68);

			//padding
			assertOffset(0M, 72);
			assertOffset<DateTime>(d.DateTime, 88);


			void assertOffset<TMember>(TMember t, int offset)
			{
				Debug.Assert(Unsafe.OffsetOf(ref d, t) == offset);
			}
		}
	}

}