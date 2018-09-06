using System;
using System.Diagnostics;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.CLR;
using RazorSharp.Memory;
using RazorSharp.Pointers;

namespace Test.Testing.Tests
{

	[TestFixture]
	public class StackAllocateTests
	{
		private class Klass
		{
			private string m_s;

			public ref string Str => ref m_s;

			public override string ToString()
			{
				return String.Format("m_s = [{0}]", m_s);
			}
		}

		[Test]
		public unsafe void StackAllocate()
		{
			/**
			 * Stack allocate a reference type
			 */
			byte* b = stackalloc byte[Unsafe.BaseInstanceSize<Klass>()];
			Mem.StackInit<Klass>(ref b);
			Pointer<Klass> k = &b;


			// Make sure it's on the stack
			Debug.Assert(Mem.IsOnStack(ref k.Reference));
			Debug.Assert(Mem.IsOnStack(k.Address));

			// Make sure it's not in the GC heap
			Debug.Assert(!GCHeap.IsInGCHeap(ref k.Reference));
			Debug.Assert(!GCHeap.IsInGCHeap(k.Address));

			// Make sure it's not a GC heap pointer
			Debug.Assert(!GCHeap.GlobalHeap->IsHeapPointer(k.Reference));
			Debug.Assert(!GCHeap.GlobalHeap->IsHeapPointer(k.ToPointer()));
			/* << - >> */
		}
	}

}