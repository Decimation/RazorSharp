#region

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

#endregion

namespace Test.Testing.Tests
{
	[TestFixture]
	public class FunctionOverrideTests
	{
		private static class Switch
		{
			public static bool? Flag { get; set; }

			private static void Reset()
			{
				Flag = null;
			}
		}


		private class Target
		{
			~Target()
			{
				Switch.Flag = false;
			}

			public static Target operator +(Target t, Target x)
			{
				Switch.Flag = false;
				return t;
			}
		}

		private void override_op_Addition(Target t, Target x)
		{
			Switch.Flag = true;
		}

		private void override_Finalize(Target t)
		{
			Switch.Flag = true;
		}

		private static object ManualInvokeTarget(string name, object instance = null, params object[] args)
		{
			return ManualInvoke(typeof(Target), name, instance, args);
		}

		private static object ManualInvoke(Type target, string targetName, object targetInstance = null,
			params object[]                     args)
		{
			var method = target.GetAnyMethod(targetName);
			return method.Invoke(targetInstance, args);
		}

		private static void Override(Type target, string targetName, Type src, string srcName)
		{
			Pointer<MethodDesc> mdTarget = target.GetMethodDesc(targetName);
			Pointer<byte>       pSrc     = Unsafe.AddressOfFunction(src, srcName);

			mdTarget.Reference.SetStableEntryPoint(pSrc);
		}

		[Test]
		public void OverrideAdditionOperator()
		{
			var target = new Target();
			target += target;

//			Debug.Assert(!Switch.Flag.Value);
			Contract.Requires(!Switch.Flag.Value);

			Override(typeof(Target), "op_Addition", typeof(FunctionOverrideTests), "override_op_Addition");

			target += target;
			Debug.Assert(Switch.Flag.Value);
		}

		[Test]
		public void OverrideFinalizer()
		{
			var target = new Target();
			ManualInvokeTarget("Finalize", target);

//			Debug.Assert(!Switch.Flag.Value);
			Contract.Requires(!Switch.Flag.Value);

			Override(typeof(Target), "Finalize", typeof(FunctionOverrideTests), "override_Finalize");

			ManualInvokeTarget("Finalize", target);
			Debug.Assert(Switch.Flag.Value);
		}
	}
}