#region

using System.Diagnostics;
using NUnit.Framework;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Pointers;
using Test.Testing.Types;
// ReSharper disable InconsistentNaming
#pragma warning disable 219

#endregion

namespace Test.Testing.Tests
{
	[TestFixture]
	public class SignatureCallTests
	{
		[Test]
		public void FieldDesc()
		{
			Pointer<FieldDesc> fd = typeof(string).GetFieldDesc("m_firstChar");

//			void*                module    = fd.Reference.GetModule();
			int size = fd.Reference.Size;

			Pointer<MethodTable> mt        = fd.Reference.EnclosingMethodTable;
			int                  memberDef = fd.Reference.Token;
		}

		[Test]
		public void GCHeap()
		{
			Pointer<GCHeap> gc = RazorSharp.CoreClr.Structures.GCHeap.GlobalHeap;
			string          o  = "foo";

			// todo: weird DivideByZeroException here; passes fine without unit testing
//			bool isHeapPtr = gc->IsHeapPointer(o);


			bool isGcInProg = gc.Reference.IsGCInProgress();
			int  gcCount    = gc.Reference.GCCount;
		}

		[Test]
		public void JITFunctions()
		{
			Pointer<MethodTable> mt = typeof(string).GetMethodTable();
			Debug.Assert(Runtime.MethodTableToType(mt) == typeof(string));
		}

		[Test]
		public void MethodDesc()
		{
			Pointer<MethodDesc> md = typeof(Dummy).GetMethodDesc("doSomething");

			bool                 isCtor                 = md.Reference.IsConstructor;
			int                  memberDef              = md.Reference.Token;
			bool                 isPointingToNativeCode = md.Reference.IsPointingToNativeCode;
			int                  size                   = md.Reference.SizeOf;
			Pointer<MethodTable> mt                     = md.Reference.EnclosingMethodTable;
			md.Reference.Reset();
		}
	}
}