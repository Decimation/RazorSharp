#region

using System.Collections.Generic;
using NUnit.Framework;
using RazorSharp;

#endregion

namespace Test.Testing.Tests
{

	[TestFixture]
	internal class SizeTests
	{
		[Test]
		public void Test()
		{
			/**
			 * Heap sizes
			 *
			 * Type			x86 size				x64 size
			 * object		12						24
			 * object[]		16 + length * 4			32 + length * 8
			 * int[]		12 + length * 4			28 + length * 4
			 * byte[]		12 + length				24 + length
			 * string		14 + length * 2			26 + length * 2
			 */

			/**
			 * Size:		26 bytes
			 */
			string blank = "";
			AssertHeapSize(ref blank, 26);

			/**
			 * Size:        32(0x20) bytes
			 * sizeof(00000183c0a72f18) = 32 (0x20) bytes (System.String)
			 */
			string foo = "foo";
			AssertHeapSize(ref foo, 32);


			/**
			 * Size:        46(0x2e) bytes
			 * sizeof(00000183c0a8f078) = 48 (0x30) bytes (System.String)
			 */
			string randStr = new string(' ', 10);
			AssertHeapSize(ref randStr, 46);


			/**
			 * Size:        24(0x18) bytes
			 * sizeof(000001ef8003da50) = 24 (0x18) bytes (System.Int64)
			 */
			object obj = 0xFFL;
			AssertHeapSize(ref obj, 24);


			/**
			 * Size:        40(0x28) bytes
			 * sizeof(0000026d80047588) = 96 (0x60) bytes (System.Collections.Generic.List`1[[System.Int32, mscorlib]])
			 */
			List<int> list = new List<int> {1, 2, 3, 4, 5};
			AssertHeapSize(ref list, 40);

			/**
			 * Size:        44(0x2c) bytes
			 * sizeof(000001c88d89f9b8) = 48 (0x30) bytes (System.Int32[])
			 */
			int[] arr = new[] {1, 2, 3, 4, 5};
			AssertHeapSize(ref arr, 44);

			/**
			 * Size:        64(0x40) bytes
			 * sizeof(0000019a05bc4180) = 64 (0x40) bytes (System.Int64[])
			 */
			long[] longArr = new[] {1L, 2L, 3L, 4L, 5L};
			AssertHeapSize(ref longArr, 64);


			/**
			 * Size:        64(0x40) bytes
			 * sizeof(1FA80067F28) = 264 (0x108) bytes (System.String[])
			 */
			string[] strArr = new[] {"foo", "bar", "abcdef", new string(' ', 5), new string(' ', 15)};
			AssertHeapSize(ref strArr, 64);
		}

		private static void AssertHeapSize<T>(ref T t, int heapSize) where T : class
		{
			Assert.That(heapSize, Is.EqualTo(Unsafe.HeapSize(in t)));
		}
	}

}