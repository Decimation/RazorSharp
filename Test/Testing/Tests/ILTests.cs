#region

using System.Diagnostics;
using NUnit.Framework;
using RazorSharp.Clr;
using RazorSharp.Clr.Structures;
using RazorSharp.Pointers;

#endregion

// ReSharper disable InconsistentNaming

namespace Test.Testing.Tests
{
	[TestFixture]
	public unsafe class ILTests
	{
		private static class Operations
		{
			public static int AddOp(int a, int b)
			{
				return a + b;
			}

			public static int SubOp(int a, int b)
			{
				return a - b;
			}
		}

		private static int get_ItemOp(void* __this, int index)
		{
//			Debug.Assert(GCHeap.GlobalHeap.Reference.IsHeapPointer(__this));
			return -0xFF;
		}

		private class Substrate
		{
			public int Value { get; private set; }

			public int this[int index] => Value;

			public void incr()
			{
				Value++;
			}
		}


		[Test]
		public void TestIndexHook()
		{
			Pointer<MethodDesc> mdItemOp         = typeof(Substrate).GetMethodDesc("get_Item");
			Pointer<MethodDesc> mdItemOpOverride = typeof(ILTests).GetMethodDesc("get_ItemOp");
			mdItemOp.Reference.SetStableEntryPoint(mdItemOpOverride.Reference.Function);
			var a = new Substrate();
			Debug.Assert(a[0] == -0xFF);
		}

		[Test]
		public void TestInstructionReplace()
		{
			Pointer<MethodDesc> mdAdd = typeof(Operations).GetMethodDesc("AddOp");
			Pointer<MethodDesc> mdSub = typeof(Operations).GetMethodDesc("SubOp");
			mdAdd.Reference.Function = mdSub.Reference.Function;
			Debug.Assert(Operations.AddOp(1, 1) == 0);
		}
	}
}