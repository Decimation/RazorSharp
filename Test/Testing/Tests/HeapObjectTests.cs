using NUnit.Framework;
using RazorSharp.Runtime;
using RazorSharp.Runtime.CLRTypes.HeapObjects;

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
			TestingAssertion.AssertHeapObject(ref x, h);

			string         s      = "foo";
			StringObject** strObj = Runtime.GetStringObject(ref s);
			TestingAssertion.AssertStringObject(ref s, strObj);

			int[]         arr    = {1, 2, 3};
			ArrayObject** arrObj = Runtime.GetArrayObject(ref arr);
			TestingAssertion.AssertArrayObject(ref arr, arrObj);

			s += " bar";
			Assert.That(s.Length, Is.EqualTo((**strObj).Length));
		}
	}

}