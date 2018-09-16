using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NUnit.Framework;
using Microsoft.Diagnostics.Runtime;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using Unsafe = RazorSharp.Unsafe;

namespace Test.Testing.Tests
{

	/// <summary>
	/// Compares ClrMD with RazorSharp
	/// </summary>
	[TestFixture]
	public unsafe class CLRRuntimeTests
	{
		private       ClrRuntime m_runtime;
		private const string     REFERENCE_TYPE_CATEGORY = "Reference types";
		private const string     VALUE_TYPE_CATEGORY     = "Value types";

		private ClrRuntime get()
		{
			var dataTarget =
				DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, UInt32.MaxValue, AttachFlag.Passive);
			return dataTarget.ClrVersions.Single().CreateRuntime();
		}

		[SetUp]
		public void Setup()
		{
			m_runtime = get();
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

		public void Compare<T>(ref T t)
		{
			WriteLine("Beginning comparison of typeof({0})\n", typeof(T).Name);

			WriteLine("-> Comparing type info");
			CompareType(ref t);
			WriteLine("-> Type info comparison passed\n");

			WriteLine("-> Comparing fields");
			CompareFields(ref t);
			WriteLine("-> Field comparison passed\n");


//			CompareMethods(ref t);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ulong GetDataAddr<T>(ref T t)
		{
			var addr = Unsafe.AddressOf(ref t);
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


		// todo
		public void CompareMethods<T>(ref T t)
		{
			var methodDescs = Runtime.GetMethodDescs<T>();

			if (!GetClrType(ref t, out ClrType clrType)) {
				ClrTypeWarn<T>();
				return;
			}

			var clrMethods = clrType.Methods;
			clrMethods  = clrMethods.OrderBy(x => x.MetadataToken).ToList();
			methodDescs = methodDescs.OrderBy(x => x.Reference.Token).ToArray();

//			Assert.That(methodDescs.Length == clrMethods.Count);

			for (int i = 0; i < methodDescs.Length; i++) {
				CompareMethod(methodDescs[i], clrMethods[i]);
			}
		}

		private void CompareMethod(in Pointer<MethodDesc> pMd, in ClrMethod clrMethod)
		{
			Assert.That(pMd.Reference.Token == clrMethod.MetadataToken, "Tokens do not match");
			Assert.That(pMd.Reference.IsConstructor == clrMethod.IsConstructor);
			Assert.That((ulong) pMd.Reference.Function == clrMethod.NativeCode);
		}


		public void CompareFields<T>(ref T t)
		{
			var fieldDescs = Runtime.GetFieldDescs(ref t);
			if (!GetClrType(ref t, out ClrType clrType)) {
				ClrTypeWarn<T>();
				return;
			}


			var clrFields = clrType.Fields;

			Collections.RemoveAll(ref fieldDescs, x => x.Reference.IsStatic);
			clrFields  = clrFields.OrderBy(x => x.Offset).ToList();
			fieldDescs = fieldDescs.OrderBy(x => x.Reference.Offset).ToArray();


			Assert.AreEqual(fieldDescs.Length, clrFields.Count);

			for (int i = 0; i < fieldDescs.Length; i++) {
				CompareField(ref t, fieldDescs[i], clrFields[i]);
			}
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
			foreach (var v in msgs) {
				WriteLine("\t\t-> {0} {1}", v, StringUtils.Check);
			}
		}

		private void CompareField<T>(ref T t, Pointer<FieldDesc> pFd, ClrInstanceField clrField)
		{
			WriteLine("\t-> Field: {0} {1}", typeof(T).Name, clrField);


//			var rsVal  = pFd.Reference.GetValue(t);
//			var clrVal = clrField.GetValue(GetDataAddr(ref t));
//			WriteLine("\t\tValue: {0}, {1}", rsVal, clrVal);

			Assert.Multiple(() =>
			{
				Assert.AreEqual(pFd.Reference.Size, clrField.Size);
				Assert.AreEqual(pFd.Reference.Name, clrField.Name);
				Assert.AreEqual(pFd.Reference.Offset, clrField.Offset);
				Assert.AreEqual(pFd.Reference.Token, clrField.Token);
			});
			WritePass("Size", "Name", "Offset", "Token");

			var rsAddr  = (ulong) pFd.Reference.GetAddress(ref t);
			var clrAddr = clrField.GetAddress(GetDataAddr(ref t));

			// ClrMD may miscalculate the address if its ClrType is null

			Assert.AreEqual(rsAddr, clrAddr, "Addresses do not match: {0}, {1}", Hex.ToHex(rsAddr), Hex.ToHex(clrAddr));

			// -- Access modifiers -- //

			Assert.Multiple(() =>
			{
				Assert.AreEqual(pFd.Reference.IsInternal, clrField.IsInternal);
				Assert.AreEqual(pFd.Reference.IsPrivate, clrField.IsPrivate);
				Assert.AreEqual(pFd.Reference.IsPublic, clrField.IsPublic);
			});


//			Console.WriteLine("[{0} {1}] [{2} {3}]", pFd.Reference.CorType, (int) pFd.Reference.CorType,
//				clrField.ElementType, (int) clrField.ElementType);

			// todo
//			Assert.That((int) pFd.Reference.CorType == (int) clrField.ElementType);
//			if (!pFd.Reference.Info.FieldType.ContainsGenericParameters)
//			Assert.That(pFd.Reference.GetValue(t) == clrField.GetValue(GetDataAddr(ref t)));
		}


		/// <summary>
		/// Compares <see cref="ClrType"/> with <see cref="MethodTable"/>
		/// </summary>
		public void CompareType<T>(ref T t)
		{
			ulong heapAddr = GetDataAddr(ref t);
			if (!GetClrType(ref t, out ClrType clrType)) {
				ClrTypeWarn<T>();
				return;
			}

			Pointer<MethodTable> rsMt  = Runtime.ReadMethodTable(ref t);
			ulong                clrMt = m_runtime.Heap.GetMethodTable(heapAddr);

			if (clrMt == 0) {
				Assert.Warn("MethodTable* could not be retrieved for typeof({0})", typeof(T).Name);
			}

			if (clrMt != 0) {
				/**
				 * Does RazorSharp MT address equal ClrMD MT address?
				 */

				Assert.Multiple(() =>
				{
					Assert.AreEqual(rsMt.ToUInt64(), clrMt, "MethodTable pointers do not match : {0}, {1}",
						Hex.ToHex(rsMt.ToUInt64()), Hex.ToHex(clrMt));
					Assert.AreEqual(rsMt.ToUInt64(), clrType.MethodTable);
				});


				/**
			     * EEClass
			     */

				ulong clrEeClass = m_runtime.Heap.GetEEClassByMethodTable(clrMt);
				Assert.Multiple(() =>
				{
					Assert.AreEqual(clrEeClass, rsMt.Reference.EEClass.ToUInt64());
					Assert.AreEqual(m_runtime.Heap.GetMethodTableByEEClass(clrEeClass), rsMt.ToUInt64());
				});

			}


			Assert.Multiple(() =>
			{
				Assert.AreEqual(clrType.MetadataToken, rsMt.Reference.MDToken);
				Assert.AreEqual(clrType.ContainsPointers, rsMt.Reference.ContainsPointers);
				Assert.AreEqual(clrType.IsAbstract, rsMt.Reference.RuntimeType.IsAbstract);
				Assert.AreEqual(clrType.IsEnum, rsMt.Reference.RuntimeType.IsEnum);
				Assert.AreEqual(clrType.IsPointer, rsMt.Reference.RuntimeType.IsPointer);
				Assert.AreEqual(clrType.IsPrimitive, rsMt.Reference.RuntimeType.IsPrimitive);
				Assert.AreEqual(clrType.IsPublic, rsMt.Reference.RuntimeType.IsPublic);
				Assert.AreEqual(clrType.IsSealed, rsMt.Reference.RuntimeType.IsSealed);
				Assert.AreEqual(clrType.IsString, rsMt.Reference.IsString);
			});


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

			#region Sizes

			/**
			 * Note: we ignore this because both ClrMD and WinDbg seem to be incorrect about the base
			 * size of a string.
			 */
			if (typeof(T) != typeof(string)) {
				Assert.AreEqual(clrType.BaseSize, rsMt.Reference.BaseSize);
			}
			Assert.AreEqual(clrType.ElementSize, rsMt.Reference.ComponentSize);

			#endregion
		}
	}

}