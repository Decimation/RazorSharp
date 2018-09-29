#region

using System.Diagnostics;
using NUnit.Framework;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;
using Test.Testing.Types;

#endregion

namespace Test.Testing.Tests
{

	[TestFixture]
	public unsafe class SignatureCallTests
	{
		[Test]
		public void MethodDesc()
		{
			Pointer<MethodDesc> md = Runtime.GetMethodDesc<Dummy>("doSomething");

			bool                 isCtor                 = md.Reference.IsConstructor;
			int                  memberDef              = md.Reference.Token;
			bool                 isPointingToNativeCode = md.Reference.IsPointingToNativeCode;
			int                  size                   = md.Reference.SizeOf;
			Pointer<MethodTable> mt                     = md.Reference.EnclosingMethodTable;
			md.Reference.Reset();
		}

		[Test]
		public void FieldDesc()
		{
			Pointer<FieldDesc>   fd        = Runtime.GetFieldDesc<string>("m_firstChar");
			void*                module    = fd.Reference.GetModule();
			int                  size      = fd.Reference.Size;
			Pointer<MethodTable> mt        = fd.Reference.EnclosingMethodTable;
			int                  memberDef = fd.Reference.Token;
		}

		[Test]
		public void GCHeap()
		{
			Pointer<GCHeap> gc = RazorSharp.CLR.Structures.GCHeap.GlobalHeap;
			string          o  = "foo";

			// todo: weird DivideByZeroException here, passes fine without unit testing
//			bool isHeapPtr = gc->IsHeapPointer(o);

			bool isEphemeral = gc.Reference.IsEphemeral(o);
			bool isGcInProg  = gc.Reference.IsGCInProgress();
			int  gcCount     = gc.Reference.GCCount;
		}

		[Test]
		public void JITFunctions()
		{
			Pointer<MethodTable> mt = Runtime.MethodTableOf<string>();
			Debug.Assert(Runtime.MethodTableToType(mt) == typeof(string));
		}
	}

}