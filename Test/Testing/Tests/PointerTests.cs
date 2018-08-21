#region

using System.Diagnostics;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.Pointers.Ex;

#endregion

namespace Test.Testing.Tests
{

	[TestFixture]
	internal unsafe class PointerTests
	{
		[Test]
		public void Test()
		{
			string            s      = "foo";
			ExPointer<string> strPtr = new ExPointer<string>(ref s);
			Assert.That(strPtr.Value, Is.EqualTo(s));

			ExPointer<string> strPtr2 = new ExPointer<string>(ref s);
			Debug.Assert(strPtr == strPtr2);

			string[]          arr     = {"", "foo", "anime"};
			ExPointer<string> strPtr3 = Unsafe.AddressOfHeap(ref arr, OffsetType.ArrayData);
			TestingUtil.Elements(strPtr3, arr);
			strPtr3 -= 3;
			Debug.Assert(strPtr3.Value == arr[0]);
			strPtr3++;
			Debug.Assert(strPtr3.Value == arr[1]);

			TestingUtil.Pressure(strPtr, ref s);
			TestingUtil.Pressure(strPtr2, ref s);
		}
	}

}