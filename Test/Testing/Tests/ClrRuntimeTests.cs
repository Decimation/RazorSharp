#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Diagnostics.Runtime;
using NUnit.Framework;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using Test.Testing.Types;
using Unsafe = RazorSharp.Unsafe;

#endregion

namespace Test.Testing.Tests
{



	/// <summary>
	///     Compares ClrMD with RazorSharp
	/// </summary>
	[TestFixture]
	public unsafe class ClrRuntimeTests
	{
		private       ClrRuntime m_runtime;
		private const string     REFERENCE_TYPE_CATEGORY = "Reference types";
		private const string     VALUE_TYPE_CATEGORY     = "Value types";

		private ClrRuntime get()
		{
			DataTarget dataTarget =
				DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, UInt32.MaxValue, AttachFlag.Passive);
			return dataTarget.ClrVersions.Single().CreateRuntime();
		}

		#region Tests

		[SetUp]
		public void Setup()
		{
			m_runtime = get();
		}

		[Test]
		[Category(VALUE_TYPE_CATEGORY)]
		public void Test_Point()
		{
			Point p = new Point();
			Compare(ref p);
		}

		[Test]
		[Category(REFERENCE_TYPE_CATEGORY)]
		public void Test_Object()
		{
			object o = new object();
			Compare(ref o);
		}

		[Test]
		[Category(VALUE_TYPE_CATEGORY)]
		public void Test_Int32()
		{
			int i = 0;
			Compare(ref i);
		}

		[Test]
		[Category(REFERENCE_TYPE_CATEGORY)]
		public void Test_IList_Int32()
		{
			IList<int> ls = new List<int>();
			Compare(ref ls);
		}

		[Test]
		[Category(REFERENCE_TYPE_CATEGORY)]
		public void Test_String()
		{
			string s = "foo";
			Compare(ref s);
		}

		#endregion

		#region Util

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ulong GetDataAddr<T>(ref T t)
		{
			IntPtr addr = Unsafe.AddressOf(ref t);
			if (!typeof(T).IsValueType) {
				addr = *(IntPtr*) addr;
			}

			return (ulong) addr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetClrType<T>(ref T t, out ClrType clrType)
		{
			clrType = m_runtime.Heap.GetObjectType(GetDataAddr(ref t)) ??
			          m_runtime.Modules[0].GetTypeByName(typeof(T).FullName);
			return clrType != null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ClrTypeWarn<T>()
		{
			Assert.Warn("ClrType could not be retrieved for typeof({0})", typeof(T).Name);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[StringFormatMethod("str")]
		private void WriteLine(string str, params object[] args)
		{
			TestContext.WriteLine(str, args);
		}

		private void WritePass(params string[] msgs)
		{
			foreach (string v in msgs) {
				WriteLine("\t\t-> {0} {1}", v, StringUtils.Check);
			}
		}

		#endregion

		#region Compare

		public void Compare<T>(ref T t)
		{
			WriteLine("Beginning comparison of typeof({0})\n", typeof(T).Name);

			WriteLine("-> Comparing type info");
			CompareType(ref t);
			WriteLine("-> Type info comparison passed\n");

			WriteLine("-> Comparing fields");
			CompareFields(ref t);
			WriteLine("-> Fields comparison passed\n");

			WriteLine("-> Comparing methods");
			CompareMethods(ref t);
			WriteLine("-> Methods comparison passed");
		}


		// todo
		public void CompareMethods<T>(ref T t)
		{
			Pointer<MethodDesc>[] methodDescs = Runtime.GetMethodDescs<T>();

			List<ClrMethod> clrMethods = methodDescs.Select(c => m_runtime.GetMethodByHandle(c.ToUInt64())).ToList();

			for (int i = 0; i < methodDescs.Length; i++) {
				CompareMethod(methodDescs[i], clrMethods[i]);
			}
		}

		/// <summary>
		///     Compares a <see cref="MethodDesc" /> with a <see cref="ClrMethod" />
		/// </summary>
		private void CompareMethod(Pointer<MethodDesc> pMd, ClrMethod clrMethod)
		{
			WriteLine("\t-> Method: {0} {1}", pMd.Reference.Name, clrMethod);
			Assert.Multiple(() =>
			{
				try {
					if (clrMethod.MethodDesc == 0) {
						WriteLine("\t\t-> MethodDesc* could not be retrieved for {0}", clrMethod);
					}
					else {
						Assert.AreEqual(pMd.ToUInt64(), clrMethod.MethodDesc, "MethodDesc");
					}
				}
				catch (ArgumentOutOfRangeException) { }

				Assert.AreEqual(pMd.Reference.Token, clrMethod.MetadataToken, "Token");
			});
		}

		public void CompareFields<T>(ref T t)
		{
			Pointer<FieldDesc>[] fieldDescs = Runtime.GetFieldDescs(ref t);
			if (!GetClrType(ref t, out ClrType clrType)) {
				ClrTypeWarn<T>();
				Assert.Warn("Field comparison not ran");
				return;
			}



			IList<ClrInstanceField> clrFields = clrType.Fields;

			Collections.RemoveAll(ref fieldDescs, x => x.Reference.IsStatic);
			clrFields  = clrFields.OrderBy(x => x.Offset).ToList();
			fieldDescs = fieldDescs.OrderBy(x => x.Reference.Offset).ToArray();


			Assert.AreEqual(fieldDescs.Length, clrFields.Count);

			for (int i = 0; i < fieldDescs.Length; i++) {
				CompareField(ref t, fieldDescs[i], clrFields[i]);
			}
		}

		/// <summary>
		///     Compares a <see cref="FieldDesc" /> with a <see cref="ClrInstanceField" />
		/// </summary>
		private void CompareField<T>(ref T t, Pointer<FieldDesc> pFd, ClrInstanceField clrField)
		{
			WriteLine("\t-> Field: {0} {1}", typeof(T).Name, clrField);

			#region Size, name, offset, token

// @formatter:off — disable formatter after this line

			Assert.Multiple(() =>
			{
				Assert.AreEqual(pFd.Reference.Size, 	clrField.Size);
				Assert.AreEqual(pFd.Reference.Name, 	clrField.Name);
				Assert.AreEqual(pFd.Reference.Offset, 	clrField.Offset);
				Assert.AreEqual(pFd.Reference.Token, 	clrField.Token);
			});

// @formatter:on — enable formatter after this line

			#endregion

			#region Address

			// ClrMD may miscalculate the address if its ClrType is null
			ulong rsAddr  = (ulong) pFd.Reference.GetAddress(ref t);
			ulong clrAddr = clrField.GetAddress(GetDataAddr(ref t));
			Assert.AreEqual(rsAddr, clrAddr, "Addresses do not match");

			#endregion

			#region Access modifiers

// @formatter:off — disable formatter after this line

			Assert.Multiple(() =>
			{
				Assert.AreEqual(pFd.Reference.IsInternal, 	clrField.IsInternal);
				Assert.AreEqual(pFd.Reference.IsPrivate, 	clrField.IsPrivate);
				Assert.AreEqual(pFd.Reference.IsPublic, 	clrField.IsPublic);
			});

// @formatter:on — enable formatter after this line

			#endregion
		}


		/// <summary>
		///     Compares a <see cref="ClrType" /> with a <see cref="MethodTable" />
		/// </summary>
		public void CompareType<T>(ref T t)
		{
			ulong heapAddr = GetDataAddr(ref t);
			if (!GetClrType(ref t, out ClrType clrType)) {
				ClrTypeWarn<T>();
				Assert.Warn("Type comparison not ran");
				return;
			}

			Pointer<MethodTable> rsMt  = Runtime.ReadMethodTable(ref t);
			ulong                clrMt = m_runtime.Heap.GetMethodTable(heapAddr);

			if (clrMt == 0) {
				WriteLine("\t-> MethodTable* could not be retrieved for typeof({0})", typeof(T).Name);
			}
			else {
				#region MethodTable

				Assert.Multiple(() =>
				{
					Assert.AreEqual(rsMt.ToUInt64(), clrMt, "MethodTable pointers do not match");
					Assert.AreEqual(rsMt.ToUInt64(), clrType.MethodTable);
				});

				#endregion

				#region EEClass

				ulong clrEeClass = m_runtime.Heap.GetEEClassByMethodTable(clrMt);
				Assert.Multiple(() =>
				{
					Assert.AreEqual(clrEeClass, rsMt.Reference.EEClass.ToUInt64());
					Assert.AreEqual(m_runtime.Heap.GetMethodTableByEEClass(clrEeClass), rsMt.ToUInt64());
				});

				#endregion
			}

			#region Token, properties

// @formatter:off — disable formatter after this line

			Assert.Multiple(() =>
			{
				Assert.AreEqual(clrType.MetadataToken, 		rsMt.Reference.MDToken);
				Assert.AreEqual(clrType.ContainsPointers,	rsMt.Reference.ContainsPointers);
				Assert.AreEqual(clrType.IsAbstract, 		rsMt.Reference.RuntimeType.IsAbstract);
				Assert.AreEqual(clrType.IsEnum, 			rsMt.Reference.RuntimeType.IsEnum);
				Assert.AreEqual(clrType.IsPointer, 			rsMt.Reference.RuntimeType.IsPointer);
				Assert.AreEqual(clrType.IsPrimitive, 		rsMt.Reference.RuntimeType.IsPrimitive);
				Assert.AreEqual(clrType.IsPublic, 			rsMt.Reference.RuntimeType.IsPublic);
				Assert.AreEqual(clrType.IsSealed, 			rsMt.Reference.RuntimeType.IsSealed);
				Assert.AreEqual(clrType.IsString, 			rsMt.Reference.IsString);
			});

// @formatter:on — enable formatter after this line

			#endregion


			#region Untestable

//			Assert.That(clrType.IsException == rsM);
//			Assert.That(clrType.IsFinalizable == ((rsMt.Reference.Flags & MethodTableFlags.HasFinalizer) != 0));
//			Assert.That(clrType.IsFree);
//			Assert.That(clrType.IsInterface == ((rsMt.Reference.Flags & MethodTableFlags.Interface) != MethodTableFlags.Interface));
//			Assert.That(clrType.IsPrivate);
//			Assert.That(clrType.IsProtected == rsMt.Reference.RuntimeType.p);
//			Assert.That(clrType.HasSimpleValue);
//			Assert.That(clrType.IsObjectReference);
//			Assert.That(clrType.IsRuntimeType);
//			Assert.That(clrType.IsValueClass);

			#endregion


			#region Sizes

			/**
			 * Note: we ignore this because both ClrMD and WinDbg seem to be incorrect about the base
			 * size of a string.
			 */
			if (RazorContract.TypeEqual<string, T>()) {
				WriteLine("-> Ignoring BaseSize comparison: typeof(T) is string");
			}
			else {
				Assert.AreEqual(clrType.BaseSize, rsMt.Reference.BaseSize);
			}

			Assert.AreEqual(clrType.ElementSize, rsMt.Reference.ComponentSize);

			#endregion
		}

		#endregion

	}

}