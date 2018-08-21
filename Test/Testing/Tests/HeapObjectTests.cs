#region

using NUnit.Framework;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures.HeapObjects;

#endregion

namespace Test.Testing.Tests
{

	[TestFixture]
	internal unsafe class HeapObjectTests
	{
		[Test]
		public void Test()
		{
			object       x = "foo";
			HeapObject** h = Runtime.GetHeapObject(ref x);
			TestingUtil.HeapObject(ref x, h);

			string         s      = "foo";
			StringObject** strObj = Runtime.GetStringObject(ref s);
			TestingUtil.StringObject(ref s, strObj);

			int[]         arr    = {1, 2, 3};
			ArrayObject** arrObj = Runtime.GetArrayObject(ref arr);
			TestingUtil.ArrayObject(ref arr, arrObj);

			s += " bar";
			Assert.That(s.Length, Is.EqualTo((**strObj).Length));
		}
	}

}