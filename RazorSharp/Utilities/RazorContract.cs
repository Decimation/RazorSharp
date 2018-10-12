#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using RazorSharp.Common;
using RazorSharp.Utilities.Exceptions;

#endregion

namespace RazorSharp.Utilities
{

	#region

	using AsrtCnd = AssertionConditionAttribute;
	using AsrtCndType = AssertionConditionType;

	#endregion

	/// <summary>
	///     Poor man's <see cref="System.Diagnostics.Contracts.Contract" /> because
	///     <see cref="System.Diagnostics.Contracts.Contract" /> doesn't work with JetBrains Rider
	/// </summary>
	internal static class RazorContract
	{
		private const string COND_FALSE_HALT     = "cond:false => halt";
		private const string VALUE_NULL_HALT     = "value:null => halt";
		private const string STRING_FORMAT_PARAM = "msg";
		private const string NULLREF_EXCEPTION   = "Value == null";


		/// <summary>
		///     Call to <see cref="Trace.Assert(bool, string)" />
		/// </summary>
		/// <param name="cond">Condition to assert</param>
		/// <param name="msg"></param>
		/// <param name="args"></param>
		[DebuggerHidden]
		[AssertionMethod]
		[ContractAnnotation(COND_FALSE_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[StringFormatMethod(STRING_FORMAT_PARAM)]
		internal static void Assert([AsrtCnd(AsrtCndType.IS_TRUE)] bool cond, string msg, params object[] args)
		{
			Trace.Assert(cond, String.Format(msg, args));
		}

		/// <summary>
		///     Call to <see cref="Trace.Assert(bool)" />
		/// </summary>
		/// <param name="cond">Condition to assert</param>
		[DebuggerHidden]
		[AssertionMethod]
		[ContractAnnotation(COND_FALSE_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void Assert([AsrtCnd(AsrtCndType.IS_TRUE)] bool cond)
		{
			Trace.Assert(cond);
		}

		[DebuggerHidden]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ThrowNullReferenceException()
		{
			throw new NullReferenceException(NULLREF_EXCEPTION);
		}

		[DebuggerHidden]
		[AssertionMethod]
		[ContractAnnotation(VALUE_NULL_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static unsafe void RequiresNotNull([AsrtCnd(AsrtCndType.IS_NOT_NULL)] void* value)
		{
			if (value == null) {
				ThrowNullReferenceException();
			}
		}

		[DebuggerHidden]
		[AssertionMethod]
		[ContractAnnotation(VALUE_NULL_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RequiresNotNull(IntPtr value)
		{
			if (value == IntPtr.Zero) {
				ThrowNullReferenceException();
			}
		}

		[DebuggerHidden]
		[AssertionMethod]
		[ContractAnnotation(VALUE_NULL_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RequiresNotNull<T>([AsrtCnd(AsrtCndType.IS_NOT_NULL)] in T value) where T : class
		{
			if (value == null) {
				ThrowNullReferenceException();
			}
		}

		[DebuggerHidden]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[StringFormatMethod(STRING_FORMAT_PARAM)]
		internal static void Requires([AsrtCnd(AsrtCndType.IS_TRUE)] bool cond, string msg = null, params object[] args)
		{
			if (cond) {
				return;
			}

			Requires<RuntimeException>(cond, msg);
		}

		/// <summary>
		///     Checks to see if <paramref name="cond" /> is <c>true</c>. If <paramref name="cond" /> is <c>false</c>, throws
		///     <typeparamref name="TException" />
		/// </summary>
		/// <param name="cond">Condition</param>
		/// <param name="msg">Optional message for <typeparamref name="TException"></typeparamref> </param>
		/// <param name="args"></param>
		/// <typeparam name="TException">Exception type to throw</typeparam>
		[DebuggerHidden]
		[AssertionMethod]
		[ContractAnnotation(COND_FALSE_HALT)]
		[StringFormatMethod(STRING_FORMAT_PARAM)]
		internal static void Requires<TException>([AsrtCnd(AsrtCndType.IS_TRUE)] bool cond,
			string msg = null, params object[] args) where TException : Exception, new()
		{
			if (!cond) {
				if (msg == null) {
					throw new TException();
				}
				else {
					msg = String.Format(msg, args);

					// Special support for RuntimeException
					if (typeof(TException) == typeof(RuntimeException)) {
						throw new RuntimeException(msg);
					}

					// TException better have a constructor with a string parameter
					ConstructorInfo ctor = typeof(TException).GetConstructor(
						BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
						CallingConventions.HasThis, new[] {typeof(string)}, null);
					object     exception = ctor.Invoke(new object[] {msg});
					TException exc       = (TException) exception;
					throw exc;
				}
			}
		}


		/// <summary>
		///     <para>Asserts that <typeparamref name="TExpected" />  is <typeparamref name="TActual" />.</para>
		/// </summary>
		/// <typeparam name="TExpected">Expected type</typeparam>
		/// <typeparam name="TActual">Supplied type</typeparam>
		/// <exception cref="TypeException">
		///     If <typeparamref name="TExpected" /> is not <typeparamref name="TActual" />
		/// </exception>
		[DebuggerHidden]
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

		[DebuggerHidden]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RequiresClassType<T>()
		{
			Assert(!typeof(T).IsValueType, "Type parameter <{0}> must be a reference type", typeof(T).Name);
		}

		[DebuggerHidden]
		internal static bool TypeEqual<TExpected, TActual>()
		{
			if (typeof(TExpected) == typeof(Array)) {
				if (!typeof(TActual).IsArray) {
					return false;
				}
			}
			else if (typeof(TExpected) != typeof(TActual)) {
				return false;
			}

			return true;
		}

		/// <summary>
		///     Call to <see cref="Trace.Assert(bool)" />
		/// </summary>
		/// <param name="values"></param>
		/// <typeparam name="T"></typeparam>
		[DebuggerHidden]
		internal static void AssertEqual<T>(params T[] values)
		{
			if (values == null || values.Length == 0) {
				return;
			}

			Trace.Assert(values.All(v => v.Equals(values[0])));
		}


	}

}