#region

using System.Diagnostics;
using NUnit.Framework;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
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
			private int m_value;

			public int Value => m_value;

			public int this[int index] => m_value;

			public void incr()
			{
				m_value++;
			}
		}


		[Test]
		public void TestIndexHook()
		{
			Pointer<MethodDesc> mdItemOp         = Runtime.GetMethodDesc<Substrate>("get_Item");
			Pointer<MethodDesc> mdItemOpOverride = Runtime.GetMethodDesc(typeof(ILTests), "get_ItemOp");
			mdItemOp.Reference.SetStableEntryPoint(mdItemOpOverride.Reference.Function);
			Substrate a = new Substrate();
			Debug.Assert(a[0] == -0xFF);
		}

		[Test]
		public void TestInstructionReplace()
		{
			Pointer<MethodDesc> mdAdd = Runtime.GetMethodDesc(typeof(Operations), "AddOp");
			Pointer<MethodDesc> mdSub = Runtime.GetMethodDesc(typeof(Operations), "SubOp");
			mdAdd.Reference.Function = mdSub.Reference.Function;
			Debug.Assert(Operations.AddOp(1, 1) == 0);
		}
	}

}