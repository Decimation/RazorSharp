using System;
using NUnit.Framework;

namespace Test.Testing.Tests
{
	[TestFixture]
	public unsafe class MemoryTests
	{
		[Test]
		public void TestSpan()
		{
			Assert.Throws<IndexOutOfRangeException>(Span);
		}
		
		public void Span()
		{
			Span<int> sp    = stackalloc int[2];
			int       value = sp[2];
		}
	}
}