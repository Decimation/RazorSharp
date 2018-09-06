using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using RazorSharp;
using RazorSharp.CLR;
using Test.Testing.Types;

namespace Test.Testing.Tests
{

	[TestFixture]
	internal class StructureTests
	{
		private static void AssertFieldInfo<TType, TField>(string fieldName, ProtectionLevel prot = ProtectionLevel.Private)
		{
			var fieldActual =
				typeof(TType).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

			var fieldDesc = Runtime.GetFieldDesc<TType>(fieldName);

			/**
			 * Intrinsic functions
			 */
			Debug.Assert(!fieldDesc.Reference.IsStatic);
			Debug.Assert(fieldDesc.Reference.Protection == prot);
			Debug.Assert(!fieldDesc.Reference.IsThreadLocal);

			/**
			 * Sigcall functions
			 */
			Debug.Assert(fieldDesc.Reference.Info == fieldActual);
			Debug.Assert(fieldDesc.Reference.Name == fieldName);
			Debug.Assert(fieldDesc.Reference.Size == Unsafe.SizeOf<TField>());
			Debug.Assert(fieldDesc.Reference.EnclosingType == typeof(TType));
			Debug.Assert(fieldDesc.Reference.MemberDef == fieldActual.MetadataToken);
			Debug.Assert(fieldDesc.Reference.EnclosingMethodTable == Runtime.MethodTableOf<TType>());
		}

		[Test]
		public void TestFieldDesc()
		{
			AssertFieldInfo<string, char>("m_firstChar");
			AssertFieldInfo<Dummy, byte>("_byte");
			AssertFieldInfo<Dummy, DateTime>("_dateTime");
			AssertFieldInfo<Dummy, string>("_string");
		}

		[Test]
		public void TestMethodDesc()
		{
			AssertMethodInfo<Dummy>("doSomething");
		}


		private static void AssertMethodInfo<TType>(string fnName)
		{
			var methodInfoActual = typeof(TType).GetMethod(fnName,
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
			var methodDesc = Runtime.GetMethodDesc<TType>(fnName);


			Debug.Assert(methodDesc.Reference.Info == methodInfoActual);
			Debug.Assert(methodDesc.Reference.EnclosingMethodTable == Runtime.MethodTableOf<TType>());
			Debug.Assert(methodDesc.Reference.MemberDef == methodInfoActual.MetadataToken);
			Debug.Assert(!methodDesc.Reference.IsCtor);

		}
	}

}