#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using JetBrains.Annotations;
using RazorCommon;
using RazorSharp.CLR;
using RazorSharp.Utilities.Exceptions;

#endregion

namespace RazorSharp.Utilities
{

	using AsrtCnd = AssertionConditionAttribute;
	using AsrtCndType = AssertionConditionType;

	/// <summary>
	/// Poor man's <see cref="System.Diagnostics.Contracts.Contract"/> because
	/// <see cref="System.Diagnostics.Contracts.Contract"/> doesn't work with JetBrains Rider
	/// </summary>
	internal static class RazorContract
	{
		/// <summary>
		/// Call to <see cref="Trace.Assert(bool, string)"/>
		/// </summary>
		/// <param name="cond">Condition to assert</param>
		/// <param name="msg"></param>
		[AssertionMethod]
		[ContractAnnotation("cond:false => halt")]
		internal static void Assert([AsrtCnd(AsrtCndType.IS_TRUE)] bool cond, string msg)
		{
			Trace.Assert(cond, msg);
		}

		/// <summary>
		/// Call to <see cref="Trace.Assert(bool)"/>
		/// </summary>
		/// <param name="cond">Condition to assert</param>
		[AssertionMethod]
		[ContractAnnotation("cond:false => halt")]
		internal static void Assert([AsrtCnd(AsrtCndType.IS_TRUE)] bool cond)
		{
			Trace.Assert(cond);
		}

		[AssertionMethod]
		[ContractAnnotation("v:null => halt")]
		internal static unsafe void RequiresNotNull([AsrtCnd(AsrtCndType.IS_NOT_NULL)] void* v)
		{
			if (v == null) {
				throw new NullReferenceException($"Pointer {Hex.ToHex(null)} == null");
			}
		}

		[AssertionMethod]
		[ContractAnnotation("t:null => halt")]
		internal static void RequiresNotNull<T>([AsrtCnd(AsrtCndType.IS_NOT_NULL)] in T t) where T : class
		{
			if (t == null) {
				throw new NullReferenceException($"{nameof(t)} == null");
			}
		}

		internal static void Requires([AsrtCnd(AsrtCndType.IS_TRUE)] bool cond, string msg = null)
		{
			Requires<RuntimeException>(cond, msg);
		}

		/// <summary>
		/// Checks to see if <paramref name="cond"/> is <c>true</c>. If <paramref name="cond"/> is <c>false</c>, throws
		/// <typeparamref name="TException"/>
		/// </summary>
		/// <param name="cond">Condition</param>
		/// <param name="msg">Optional message for <typeparamref name="TException"></typeparamref> </param>
		/// <typeparam name="TException">Exception type to throw</typeparam>
		[AssertionMethod]
		[ContractAnnotation("cond:false => halt")]
		internal static void Requires<TException>([AsrtCnd(AsrtCndType.IS_TRUE)] bool cond, string msg = null)
			where TException : Exception, new()
		{
			if (!cond) {
				if (msg == null) {
					throw new TException();
				}
				else {
					// Special support for RuntimeException
					if (typeof(TException) == typeof(RuntimeException)) {
						throw new RuntimeException(msg);
					}

					// TException better have a constructor with a string parameter
					var ctor = typeof(TException).GetConstructor(
						bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
						CallingConventions.HasThis, types: new[] {typeof(string)}, null);
					var exception = ctor.Invoke(new object[] {msg});
					var exc       = (TException) exception;
					throw exc;
				}
			}
		}


		/// <summary>
		/// <para>Asserts that <typeparamref name="TExpected"/>  is <typeparamref name="TActual"/>.</para>
		///
		///
		/// </summary>
		/// <typeparam name="TExpected">Expected type</typeparam>
		/// <typeparam name="TActual">Supplied type</typeparam>
		/// <exception cref="TypeException">If <typeparamref name="TExpected"/> is not <typeparamref name="TActual"/> </exception>
		internal static void RequiresType<TExpected, TActual>()
		{
			if (typeof(TExpected) == typeof(Array)) {
				if (!typeof(TActual).IsArray) {
					TypeException.Throw<Array, TActual>();
				}
			}
			else if (typeof(TExpected) != typeof(TActual)) {
				TypeException.Throw<TExpected, TActual>();
			}
		}

		internal static void AssertNoThrows<TException>(Action action) where TException : Exception
		{
			Trace.Assert(!Throws<TException>(action));
		}

		internal static void AssertThrows<TException>(Action action) where TException : Exception
		{
			Trace.Assert(Throws<TException>(action));
		}

		[HandleProcessCorruptedStateExceptions]
		internal static bool Throws<TException>(Action action) where TException : Exception
		{
			try {
				action();
			}
			catch (TException) {
				return true;
			}

			return false;
		}

		/// <summary>
		/// Call to <see cref="Trace.Assert(bool)"/>
		/// </summary>
		/// <param name="values"></param>
		/// <typeparam name="T"></typeparam>
		internal static void AssertEqual<T>(params T[] values)
		{
			if (values == null || values.Length == 0) {
				return;
			}

			Trace.Assert(values.All(v => v.Equals(values[0])));
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="__this"> <see cref="RazorSharp.CLR.Structures.FieldDesc"/> address</param>
		/// <exception cref="FieldDescException">If <see cref="Runtime.FieldAddrMap"/> does not contain <paramref name="__this"/> </exception>
		internal static void RequiresFieldDescAddress(IntPtr __this)
		{
			const string fieldDescException = "FieldDesc* has incorrect address. Is the FieldDesc* dereferenced?";
			Requires<FieldDescException>(Runtime.FieldAddrMap.ContainsKey(__this), fieldDescException);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="__this"> <see cref="RazorSharp.CLR.Structures.MethodDesc"/> address</param>
		/// <exception cref="MethodDescException">If <see cref="Runtime.MethodAddrMap"/> does not contain <paramref name="__this"/> </exception>
		internal static void RequiresMethodDescAddress(IntPtr __this)
		{
			const string methodDescException = "MethodDesc* has incorrect address. Is the MethodDesc* dereferenced?";
			Requires<MethodDescException>(Runtime.MethodAddrMap.ContainsKey(__this), methodDescException);
		}
	}

}