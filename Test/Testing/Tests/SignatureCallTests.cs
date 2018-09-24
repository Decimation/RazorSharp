using System.Diagnostics;
using NUnit.Framework;
using NUnit.Framework.Internal;
using RazorSharp.CLR;
using Test.Testing.Types;

namespace Test.Testing.Tests
{
	[TestFixture]
	public unsafe class SignatureCallTests
	{
		[Test]
		public void MethodDesc()
		{
			var md = Runtime.GetMethodDesc<Dummy>("doSomething");

			bool isCtor = md.Reference.IsConstructor;
			var memberDef = md.Reference.Token;
			var isPointingToNativeCode = md.Reference.IsPointingToNativeCode;
			var size = md.Reference.SizeOf;
			var mt = md.Reference.EnclosingMethodTable;
			md.Reference.Reset();

		}

		[Test]
		public void FieldDesc()
		{
			var fd = Runtime.GetFieldDesc<string>("m_firstChar");
			var module = fd.Reference.GetModule();
			var size = fd.Reference.Size;
			var mt = fd.Reference.EnclosingMethodTable;
			var memberDef = fd.Reference.Token;
		}

		[Test]
		public void GCHeap()
		{
			var gc = RazorSharp.CLR.Structures.GCHeap.GlobalHeap;
			string o = "foo";

			// todo: weird DivideByZeroException here, passes fine without unit testing
//			bool isHeapPtr = gc->IsHeapPointer(o);

			bool isEphemeral = gc.Reference.IsEphemeral(o);
			bool isGcInProg = gc.Reference.IsGCInProgress();
			int gcCount = gc.Reference.GCCount;
		}

		[Test]
		public void JITFunctions()
		{
			var mt = Runtime.MethodTableOf<string>();
			Debug.Assert(Runtime.MethodTableToType(mt) == typeof(string));
		}
	}

}