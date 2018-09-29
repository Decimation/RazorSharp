#region

using System.Diagnostics;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.Native;

#endregion

namespace Test.Testing.Tests
{

	[TestFixture]
	public class NativeTests
	{
		[Test]
		public void ReadWriteCurrentProcessMemoryTest()
		{
			string str = "foo";

			string value = Kernel32.ReadCurrentProcessMemory<string>(Unsafe.AddressOf(ref str).Address);
			Debug.Assert(value == "foo");

			Kernel32.WriteCurrentProcessMemory(Unsafe.AddressOf(ref str).Address, "bar");
			Debug.Assert(str == "bar");
		}


	}

}