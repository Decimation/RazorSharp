using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using NUnit.Framework;
using RazorSharp.CLR.Structures;
using RazorSharp.Utilities.Exceptions;

namespace Test.Testing.Tests
{

	[TestFixture]
	public class ClrFunctionTests
	{
		[HandleProcessCorruptedStateExceptions]
		private bool CorruptsState(Action fn)
		{
			try {
				fn();
			}
			catch (SigcallException) {
				return true;
			}
			catch (InvalidProgramException) {
				return true;
			}
			catch (AccessViolationException) {
				return true;
			}

			return false;
		}

		[Test]
		public void GCFunctions()
		{
			object o = new object();

			Debug.Assert(!CorruptsState(() => GCHeap.GlobalHeap.Reference.IsHeapPointer(o)));

			//Debug.Assert(!CorruptsState(() => GCHeap.GlobalHeap.Reference.IsEphemeral(o)));
			//Debug.Assert(!CorruptsState(() => GCHeap.GlobalHeap.Reference.IsGCInProgress()));
			int i = GCHeap.GlobalHeap.Reference.GCCount;
		}
	}

}