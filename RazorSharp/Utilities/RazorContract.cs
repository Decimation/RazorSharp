#region

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using RazorSharp.Utilities.Exceptions;

#endregion

namespace RazorSharp.Utilities
{

	#region

	using AsrtCnd = AssertionConditionAttribute;
	using AsrtCndType = AssertionConditionType;

	#endregion

	// todo: verify all new changes work as of Oct 12 2018

	/// <summary>
	///     Poor man's <see cref="System.Diagnostics.Contracts.Contract" /> because
	///     <see cref="System.Diagnostics.Contracts.Contract" /> is dead and doesn't work with JetBrains Rider
	/// </summary>
	internal static class RazorContract
	{
		private const string COND_FALSE_HALT     = "cond:false => halt";
		private const string VALUE_NULL_HALT     = "value:null => halt";
		private const string STRING_FORMAT_PARAM = "msg";
		private const string NULLREF_EXCEPTION   = "value == null";


		/// <summary>
		/// </summary>
		/// <param name="notArray">
		///     <see cref="Action" /> to perform if <typeparamref name="TExpected" /> is <c>typeof(Array)</c>
		///     and <typeparamref name="TActual" /> is not <c>typeof(Array)</c>
		/// </param>
		/// <param name="notActual">
		///     <see cref="Action" /> to perform if <typeparamref name="TExpected" /> is not
		///     <typeparamref name="TActual" />
		/// </param>
		/// <typeparam name="TExpected">Expected <see cref="Type" /></typeparam>
		/// <typeparam name="TActual">Actual <see cref="Type" /></typeparam>
		private static void ResolveTypeAction<TExpected, TActual>(Action notArray, Action notActual)
		{
			if (typeof(TExpected) == typeof(Array)) {
				if (!typeof(TActual).IsArray) {
					notArray();
				}
			}
			else if (typeof(TExpected) != typeof(TActual)) {
				notActual();
			}
		}

		[DebuggerHidden]
		internal static bool TypeEqual<TExpected, TActual>()
		{
			bool val = true;
			ResolveTypeAction<TExpected, TActual>(() => val = false, () => val = false);
			return val;
		}

		/// <summary>
		/// </summary>
		/// <param name="values"></param>
		/// <typeparam name="T"></typeparam>
		[DebuggerHidden]
		internal static void AssertEqual<T>(params T[] values)
		{
			if (values == null || values.Length == 0) {
				return;
			}

			Assert(values.All(v => v.Equals(values[0])));
		}

		#region Assert

		/// <summary>
		///     Checks for a condition; if <paramref name="cond" /> is <c>false</c>, displays the stack trace.
		/// </summary>
		/// <param name="cond">Condition to assert</param>
		/// <param name="msg">Message</param>
		/// <param name="args">Formatting elements</param>
		[DebuggerHidden]
		[AssertionMethod]
		[ContractAnnotation(COND_FALSE_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[StringFormatMethod(STRING_FORMAT_PARAM)]
		internal static void Assert([AsrtCnd(AsrtCndType.IS_TRUE)] bool cond, string msg, params object[] args)
		{
			Contract.Assert(cond, string.Format(msg, args));
		}

		/// <summary>
		///     Checks for a condition; if <paramref name="cond" /> is <c>false</c>, displays the stack trace.
		/// </summary>
		/// <param name="cond">Condition to assert</param>
		[DebuggerHidden]
		[AssertionMethod]
		[ContractAnnotation(COND_FALSE_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void Assert([AsrtCnd(AsrtCndType.IS_TRUE)] bool cond)
		{
			Contract.Assert(cond);
		}

		#endregion

		#region Requires (precondition)

		#region Not null

		/// <summary>
		///     Specifies a precondition: checks to see if <paramref name="value" /> is <c>null</c>
		/// </summary>
		/// <param name="value">Pointer to check</param>
		[DebuggerHidden]
		[AssertionMethod]
		[ContractAnnotation(VALUE_NULL_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static unsafe void RequiresNotNull([AsrtCnd(AsrtCndType.IS_NOT_NULL)] void* value)
		{
			Requires<NullReferenceException>(value != null);
		}

		/// <summary>
		///     Specifies a precondition: checks to see if <paramref name="value" /> is <see cref="IntPtr.Zero" />
		/// </summary>
		/// <param name="value"><see cref="IntPtr" /> to check</param>
		[DebuggerHidden]
		[AssertionMethod]
		[ContractAnnotation(VALUE_NULL_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RequiresNotNull(IntPtr value)
		{
			Requires<NullReferenceException>(value != IntPtr.Zero);
		}

		/// <summary>
		///     Specifies a precondition: checks to see if <paramref name="value" /> is <c>null</c>
		///     <remarks>
		///         May cause a boxing operation.
		///     </remarks>
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <typeparam name="T"></typeparam>
		[DebuggerHidden]
		[AssertionMethod]
		[ContractAnnotation(VALUE_NULL_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RequiresNotNull<T>([AsrtCnd(AsrtCndType.IS_NOT_NULL)] T value)
		{
			Requires<NullReferenceException>(value != null);
		}

		/// <summary>
		///     Specifies a precondition: checks to see if <paramref name="value" /> is <c>null</c>
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <typeparam name="T"></typeparam>
		[DebuggerHidden]
		[AssertionMethod]
		[ContractAnnotation(VALUE_NULL_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RequiresNotNull<T>([AsrtCnd(AsrtCndType.IS_NOT_NULL)] in T value) where T : class
		{
			Requires<NullReferenceException>(value != null);
		}

		#endregion

		/// <summary>
		///     Specifies a precondition: checks to see if <paramref name="cond" /> is <c>true</c>
		/// </summary>
		/// <param name="cond">Condition to check</param>
		/// <param name="msg">Optional message</param>
		/// <param name="args">Optional formatting elements</param>
		[DebuggerHidden]
		[ContractAnnotation(COND_FALSE_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[StringFormatMethod(STRING_FORMAT_PARAM)]
		internal static void Requires([AsrtCnd(AsrtCndType.IS_TRUE)] bool cond, string msg = null, params object[] args)
		{
			Requires<PreconditionException>(cond, msg);
		}


		private static void FailPrecondition<TException>(string msg = null, params object[] args)
			where TException : Exception, new()
		{
			if (msg == null) {
				msg = "Precondition failed";
				throw Create();
			}

			msg = string.Format(msg, args);
			msg = string.Format("Precondition failed: {0}", msg);

			if (typeof(TException) == typeof(NullReferenceException)) {
				throw new NullReferenceException(NULLREF_EXCEPTION);
			}

			// Special support for PreconditionException
			if (typeof(TException) == typeof(PreconditionException)) {
				throw new PreconditionException(msg);
			}

			TException Create()
			{
				// TException better have a constructor with a string parameter
				ConstructorInfo ctor = typeof(TException).GetConstructor(
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
					CallingConventions.HasThis, new[] {typeof(string)}, null);

				return (TException) ctor.Invoke(new object[] {msg});
			}
		}

		/// <summary>
		///     Specifies a precondition: checks to see if <paramref name="cond" /> is <c>true</c>.
		///     If <paramref name="cond" /> is <c>false</c>, throws <typeparamref name="TException" />
		/// </summary>
		/// <param name="cond">Condition</param>
		/// <param name="msg">Optional message for <typeparamref name="TException"></typeparamref> </param>
		/// <param name="args">Optional formatting elements</param>
		/// <typeparam name="TException">Exception type to throw</typeparam>
		[DebuggerHidden]
		[AssertionMethod]
		[ContractAnnotation(COND_FALSE_HALT)]
		[StringFormatMethod(STRING_FORMAT_PARAM)]
		internal static void Requires<TException>([AsrtCnd(AsrtCndType.IS_TRUE)] bool cond, string msg = null,
			params object[] args) where TException : Exception, new()
		{
			if (!cond) {
				FailPrecondition<TException>(msg, args);
			}
		}


		/// <summary>
		///     <para>
		///         Specifies a precondition: checks to see if <typeparamref name="TExpected" />  is
		///         <typeparamref name="TActual" />.
		///     </para>
		/// </summary>
		/// <typeparam name="TExpected">Expected type</typeparam>
		/// <typeparam name="TActual">Supplied type</typeparam>
		/// <exception cref="TypeException">
		///     If <typeparamref name="TExpected" /> is not <typeparamref name="TActual" />
		/// </exception>
		[DebuggerHidden]
		internal static void RequiresType<TExpected, TActual>()
		{
			ResolveTypeAction<TExpected, TActual>(TypeException.Throw<Array, TActual>,
				TypeException.Throw<TExpected, TActual>);
		}

		/// <summary>
		///     Specifies a precondition: checks to see if <typeparamref name="T" /> is a reference type.
		/// </summary>
		/// <typeparam name="T">Type to verify</typeparam>
		[DebuggerHidden]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RequiresClassType<T>()
		{
			Requires(!typeof(T).IsValueType, "Type parameter <{0}> must be a reference type", typeof(T).Name);
		}

		#endregion

	}

}