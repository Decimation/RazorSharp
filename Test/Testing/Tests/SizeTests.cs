#region

using System;
using System.Collections.Generic;
using NUnit.Framework;
using RazorSharp;

#endregion

namespace Test.Testing.Tests
{
	/*public struct SizeInfo
	{
		/// <summary>
		///     <see cref="Unsafe.AutoSizeOf{T}" />
		/// </summary>
		internal int AutoSizeOf { get; private set; }

		/// <summary>
		///     <see cref="Unsafe.ManagedSizeOf{T}" />
		/// </summary>
		internal int ManagedSizeOf { get; private set; }

		/// <summary>
		///     <see cref="Unsafe.NativeSizeOf{T}" />
		/// </summary>
		internal int NativeSizeOf { get; private set; }

		/// <summary>
		///     <see cref="Unsafe.SizeOf{T}" />
		/// </summary>
		internal int SizeOf { get; private set; }

		/// <summary>
		///     <see cref="Unsafe.HeapSize{T}" />
		/// </summary>
		internal int HeapSize { get; private set; }

		/// <summary>
		///     <see cref="Unsafe.BaseFieldsSize{T}()" />
		/// </summary>
		internal int BaseFieldsSize { get; private set; }

		/// <summary>
		///     <see cref="Unsafe.BaseFieldsSize{T}(ref T)" />
		/// </summary>
		internal int BaseFieldsSizeRef { get; private set; }

		/// <summary>
		///     <see cref="Unsafe.BaseInstanceSize{T}" />
		/// </summary>
		internal int BaseInstanceSize { get; private set; }


		public static SizeInfo Get<T>(T t)
		{
			SizeInfo si = new SizeInfo
			{
				HeapSize          = Unsafe.SizeOf__(t, Unsafe.SizeType.Heap),
				SizeOf            = Unsafe.SizeOf__(t, Unsafe.SizeType.Default),
				AutoSizeOf        = Unsafe.SizeOf__(t),
				BaseInstanceSize  = Unsafe.SizeOf__(t, Unsafe.SizeType.BaseInstance),
				NativeSizeOf      = Unsafe.SizeOf__(t, Unsafe.SizeType.Native),
				ManagedSizeOf     = Unsafe.SizeOf__(t, Unsafe.SizeType.Managed),
				BaseFieldsSizeRef = Unsafe.SizeOf__(t, Unsafe.SizeType.BaseFields),
				BaseFieldsSize    = Unsafe.BaseFieldsSize<T>()
			};


			return si;
		}

		public override string ToString()
		{
			ConsoleTable table = new ConsoleTable("Size type", "Value");
			table.AddRow("SizeOf", SizeOf);
			table.AddRow("HeapSize", HeapSize);
			table.AddRow("Auto", AutoSizeOf);
			table.AddRow("BaseInstance", BaseInstanceSize);
			table.AddRow("Native", NativeSizeOf);
			table.AddRow("Managed", ManagedSizeOf);
			table.AddRow("BaseFieldsSize", BaseFieldsSize);
			table.AddRow("BaseFieldsSizeRef", BaseFieldsSizeRef);
			return table.ToMarkDownString();
		}
	}*/

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
	[TestFixture]
	internal class SizeTests
	{
		private static void AssertHeapSize<T>(ref T t, int heapSize) where T : class
		{
			Assert.That(heapSize, Is.EqualTo(Unsafe.HeapSize(ref t)));
		}

		/// <summary>
		///     Size:        44(0x2c) bytes
		///     sizeof(000001c88d89f9b8) = 48 (0x30) bytes (System.Int32[])
		/// </summary>
		[Test]
		public void Array_Int()
		{
			int[] arr = {1, 2, 3, 4, 5};
			AssertHeapSize(ref arr, 44);
		}

		/// <summary>
		///     Size:        64(0x40) bytes
		///     sizeof(0000019a05bc4180) = 64 (0x40) bytes (System.Int64[])
		/// </summary>
		[Test]
		public void Array_Long()
		{
			long[] longArr = {1L, 2L, 3L, 4L, 5L};
			AssertHeapSize(ref longArr, 64);
		}

		/// <summary>
		///     Size:        64(0x40) bytes
		///     sizeof(1FA80067F28) = 264 (0x108) bytes (System.String[])
		/// </summary>
		[Test]
		public void Array_String()
		{
			string[] strArr = {"foo", "bar", "abcdef", new string(' ', 5), new string(' ', 15)};
			AssertHeapSize(ref strArr, 64);
		}

		/// <summary>
		///     Size: 24 bytes
		/// </summary>
		[Test]
		public void IComparable_Boxed_Int()
		{
			IComparable c = 1;
			AssertHeapSize(ref c, 24);
		}

		/// <summary>
		///     Size: 32 bytes
		/// </summary>
		[Test]
		public void IComparable_String()
		{
			IComparable c = "foo";
			AssertHeapSize(ref c, 32);
		}

		/// <summary>
		///     Size:        40(0x28) bytes
		///     sizeof(0000026d80047588) = 96 (0x60) bytes (System.Collections.Generic.List`1[[System.Int32, mscorlib]])
		/// </summary>
		[Test]
		public void List_Int()
		{
			var list = new List<int> {1, 2, 3, 4, 5};
			AssertHeapSize(ref list, 40);
		}

		/// <summary>
		///     Size:        24(0x18) bytes
		///     sizeof(000001ef8003da50) = 24 (0x18) bytes (System.Int64)
		/// </summary>
		[Test]
		public void Object_Boxed()
		{
			object obj = 0xFFL;
			AssertHeapSize(ref obj, 24);
		}

		/// <summary>
		///     Size:        46(0x2e) bytes
		///     sizeof(00000183c0a8f078) = 48 (0x30) bytes (System.String)
		/// </summary>
		[Test]
		public void String_10()
		{
			string randStr = new string(' ', 10);
			AssertHeapSize(ref randStr, 46);
		}

		/// <summary>
		///     Size:		26 bytes
		/// </summary>
		[Test]
		public void String_Empty()
		{
			string blank = "";
			AssertHeapSize(ref blank, 26);
		}

		/// <summary>
		///     Size:        32(0x20) bytes
		///     sizeof(00000183c0a72f18) = 32 (0x20) bytes (System.String)
		/// </summary>
		[Test]
		public void String_Foo()
		{
			string foo = "foo";
			AssertHeapSize(ref foo, 32);
		}
	}
}