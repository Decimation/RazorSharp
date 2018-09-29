#region

using System;
using System.Reflection;
using NUnit.Framework;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.CLR.Structures.EE;
using RazorSharp.Pointers;

#endregion

namespace Test.Testing.Tests
{

	[TestFixture]
	public class Reflection_RazorSharp_Tests
	{
		[Test]
		public void Test_String()
		{
			string s = "foo";
			Compare<string>();
		}

		private void Compare<T>()
		{
			CompareType<T>();
			CompareFields<T>();
			CompareMethods<T>();
		}

		public void CompareType<T>()
		{
			Type                 reType        = typeof(T);
			Pointer<MethodTable> rsMethodTable = Runtime.MethodTableOf<T>();
			Pointer<EEClass>     rsEEClass     = rsMethodTable.Reference.EEClass;

			Assert.True(reType == rsMethodTable.Reference.RuntimeType);
		}

		public void CompareMethods<T>()
		{
			Pointer<MethodDesc>[] mbs     = Runtime.GetMethodDescs<T>();
			MethodInfo[]          methods = Runtime.GetMethods(typeof(T));

			Assert.AreEqual(mbs.Length, methods.Length);

			for (int i = 0; i < mbs.Length; i++) {
				CompareMethod(mbs[i], methods[i]);
			}
		}

		public void CompareFields<T>()
		{
			Pointer<FieldDesc>[] fds    = Runtime.GetFieldDescs<T>();
			FieldInfo[]          fields = Runtime.GetFields<T>();

			Assert.AreEqual(fds.Length, fields.Length);

			for (int i = 0; i < fds.Length; i++) {
				CompareField(fds[i], fields[i]);
			}
		}

		public void CompareField(Pointer<FieldDesc> rsFieldDesc, FieldInfo reField)
		{
			Assert.True(reField.Equals(rsFieldDesc.Reference.Info));
		}

		public void CompareMethod(Pointer<MethodDesc> rsMethodDesc, MethodInfo reMethod)
		{
			Assert.True(reMethod.Equals(rsMethodDesc.Reference.Info), "Fail: {0}", reMethod.Name);
		}
	}

}