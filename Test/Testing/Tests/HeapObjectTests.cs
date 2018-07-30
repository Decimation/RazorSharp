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
			RAssert.HeapObject(ref x, h);

			string         s      = "foo";
			StringObject** strObj = Runtime.GetStringObject(ref s);
			RAssert.StringObject(ref s, strObj);

			int[]         arr    = {1, 2, 3};
			ArrayObject** arrObj = Runtime.GetArrayObject(ref arr);
			RAssert.ArrayObject(ref arr, arrObj);

			s += " bar";
			Assert.That(s.Length, Is.EqualTo((**strObj).Length));
		}
	}

}