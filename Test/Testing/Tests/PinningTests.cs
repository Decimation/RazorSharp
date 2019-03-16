#region

using System.Diagnostics;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.CoreClr.Fixed;

#endregion

namespace Test.Testing.Tests
{
	[TestFixture]
	internal unsafe class PinningTests
	{
		private const int PASSES = 10;

		private static void ApplyPressure_PinHandle<T>(ref T t) where T : class
		{
			PinHandle ph       = new ObjectPinHandle(t);
			var       origHeap = Unsafe.AddressOfHeap(ref t).Address;

//			Console.WriteLine("Original: {0}", Hex.ToHex(origHeap));

			for (int i = 0; i < PASSES; i++) {
				TestingUtil.CreateGCPressure();
				Debug.Assert(origHeap == Unsafe.AddressOfHeap(ref t));
			}

			Debug.Assert(origHeap == Unsafe.AddressOfHeap(ref t));
			ph.Dispose();
		}

		private static void ApplyPressure_PinHelper<T>(ref T t) where T : class
		{
			var origHeap = Unsafe.AddressOfHeap(ref t).Address;

//			Console.WriteLine("Original: {0}", Hex.ToHex(origHeap));

			for (int i = 0; i < PASSES; i++)
				fixed (byte* pData = &PinHelper.GetPinningHelper(t).Data) {
					TestingUtil.CreateGCPressure();
					Debug.Assert(origHeap == Unsafe.AddressOfHeap(ref t));
					Debug.Assert(pData == Unsafe.AddressOfHeap(ref t, OffsetType.Fields).ToPointer());
				}

			Debug.Assert(origHeap == Unsafe.AddressOfHeap(ref t));

//			Console.WriteLine("After: {0}", Hex.ToHex(Unsafe.AddressOfHeap(ref t)));
//			Console.WriteLine("GCs: {0}", GCHeap.GlobalHeap->GCCount);
		}

		[Test]
		public void TestPinHandle()
		{
			string s = "foo";
			ApplyPressure_PinHandle(ref s);
		}

		[Test]
		public void TestPinHelper()
		{
			string s = "foo";
			ApplyPressure_PinHelper(ref s);
		}
	}
}