#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using RazorCommon.Utilities;
using RazorSharp.CoreClr;
using RazorSharp.Native;
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
	///     <see cref="System.Diagnostics.Contracts.Contract" /> is dead and doesn't work with JetBrains Rider
	/// </summary>
	internal static class Conditions
	{
		private const string COND_FALSE_HALT   = "cond:false => halt";
		private const string VALUE_NULL_HALT   = "value:null => halt";
		private const string NULLREF_EXCEPTION = "value == null";

		internal const string STRING_FORMAT_PARAM = "msg";

		internal static void CheckCompatibility()
		{
			/**
			 * RazorSharp
			 *
			 * History:
			 * 	- RazorSharp (deci-common-c)
			 * 	- RazorSharpNeue
			 * 	- RazorCLR
			 * 	- RazorSharp
			 *
			 * Notes:
			 *  - 32-bit is not fully supported
			 *  - Most types are probably not thread-safe
			 *
			 * Goals:
			 *  - Provide identical and better functionality of ClrMD, SOS, and Reflection
			 * 	  but in a faster and more efficient way
			 */

			/**
			 * RazorSharp is tested on and targets:
			 *
			 * - x64
			 * - Windows
			 * - .NET CLR 4.7.2
			 * - Workstation Concurrent GC
			 *
			 */
			Requires64Bit();
			RequiresOS(OSPlatform.Windows);

			/**
			 * 4.0.30319.42000
			 * The version we've been testing and targeting.
			 * Other versions will probably work but we're just making sure
			 * todo - determine compatibility
			 */
			Requires(Environment.Version == Clr.ClrVersion);

			RequiresWorkstationGC();
			RequiresClr();

			if (Debugger.IsAttached) {
				Global.Log.Warning("Debugging is enabled: some features may not work correctly");
			}
		}

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
				if (!typeof(TActual).IsArray)
					notArray();
			}
			else if (typeof(TExpected) != typeof(TActual)) {
				notActual();
			}
		}

		internal static bool AssertTypeEqual<TExpected, TActual>()
		{
			bool val = true;
			ResolveTypeAction<TExpected, TActual>(() => val = false, () => val = false);
			return val;
		}


		/// <summary>
		/// </summary>
		/// <param name="values"></param>
		/// <typeparam name="T"></typeparam>
		internal static void AssertEqual<T>(params T[] values)
		{
			if (values == null || values.Length == 0)
				return;

			Assert(values.All(v => v.Equals(values[0])));
		}

		#region Assert

		/// <summary>
		///     Checks for a condition; if <paramref name="cond" /> is <c>false</c>, displays the stack trace.
		/// </summary>
		/// <param name="cond">Condition to assert</param>
		/// <param name="msg">Message</param>
		/// <param name="args">Formatting elements</param>
		[AssertionMethod]
		[ContractAnnotation(COND_FALSE_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[StringFormatMethod(STRING_FORMAT_PARAM)]
		internal static void Assert([AsrtCnd(AsrtCndType.IS_TRUE)] bool cond, string msg, params object[] args)
		{
			Contract.Assert(cond, String.Format(msg, args));
		}

		/// <summary>
		///     Checks for a condition; if <paramref name="cond" /> is <c>false</c>, displays the stack trace.
		/// </summary>
		/// <param name="cond">Condition to assert</param>
		[AssertionMethod]
		[ContractAnnotation(COND_FALSE_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void Assert([AsrtCnd(AsrtCndType.IS_TRUE)] bool cond)
		{
			Contract.Assert(cond);
		}


		internal static void AssertAllEqual<TSource, TResult>(Func<TSource, TResult> selector,
		                                                      IEnumerable<TSource>   values)
		{
			AssertAllEqual(values.Select(selector).ToArray());
		}


		internal static void AssertAllEqual<T>(T def, T[] values)
		{
			Assert(values.Any(o => o.Equals(def)));
		}

		internal static void AssertAllEqual<T>(T[] values)
		{
			AssertAllEqual(values[0], values);
		}

		internal static void AssertAllEqualAlt<T>(params T[] values)
		{
			AssertAllEqual(values);
		}

		internal static void AssertAll([AsrtCnd(AsrtCndType.IS_TRUE)] params bool[] conds)
		{
			foreach (bool b in conds) {
				Assert(b);
			}
		}

		#endregion

		private static MethodBase PreviousMethod()
		{
			return new StackFrame(2).GetMethod();
		}

		/// <summary>
		/// Commonly used for native methods or Win32 API methods which return a <see cref="bool"/> indicating
		/// whether or not the function succeeded.
		/// </summary>
		internal static void NativeRequire(bool cond, string msg = null, params object[] args)
		{
			NativeRequire(cond, PreviousMethod().Name, msg, args);
		}
		
		/// <summary>
		/// Commonly used for native methods or Win32 API methods which return a <see cref="bool"/> indicating
		/// whether or not the function succeeded.
		/// </summary>
		internal static void NativeRequire(bool cond, string name, string msg = null, params object[] args)
		{
			if (!cond) {
				string error   = String.Format("P/Invoke or native method \"{0}\" failed", name);
				var    msgFail = CreateFailString(error, msg, args);
				ThrowHelper.FailRequire<NativeException>(msgFail);
			}
		}

		#region Requires (exception)

		/// <summary>
		/// Requires <paramref name="value"/> be more than <c>0</c>
		/// </summary>
		internal static void RequiresUnsigned(int value, string name, string msg = null, params object[] args)
		{
			if (!(value > 0)) {
				string error   = String.Format("Value \"{0}\" must be > 0", name);
				var    msgFail = CreateFailString(error, msg, args);
				ThrowHelper.FailRequire(msgFail);
			}
		}

		// ReSharper disable once InconsistentNaming
		internal static void RequiresWorkstationGC()
		{
			Requires(!GCSettings.IsServerGC);
		}

		// todo
		internal static void RequiresSameLength<T>(params T[][] rg)
		{
			var len = rg[0].Length;
			foreach (var x in rg) {
				Requires(x.Length == len);
			}
		}

		[StringFormatMethod(STRING_FORMAT_PARAM)]
		private static string CreateFailString(string error, string msg = null, params object[] args)
		{
			string msgFmt;
			if (!String.IsNullOrWhiteSpace(msg)) {
				msgFmt = String.Format(msg, args);
			}
			else {
				msgFmt = null;
			}


			return String.Format("{0}: {1}", error, msgFmt);
		}

		// 3-16-19: Use string interpolation expression disabled cause it's fucking annoying

		private static string CreateErrorString<T>(T value, string name, string errorStub)
		{
			//string error = String.Format("File \"{0}\" does not exist in directory \"{1}\"", file.Name,
			//                             file.Directory);

			string typeName = value.GetType().Name;
			string error    = String.Format("{0} \"{1}\" {2}", typeName, name, errorStub);

			return error;
		}

		[AssertionMethod]
		[StringFormatMethod(STRING_FORMAT_PARAM)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RequiresFileExists(FileInfo file, string msg = null, params object[] args)
		{
			if (!file.Exists) {
				string error = String.Format("File \"{0}\" does not exist in directory \"{1}\"",
				                             file.Name, file.Directory);

				var msgFail = CreateFailString(error, msg, args);
				ThrowHelper.FailRequire<FileNotFoundException>(msgFail);
			}
		}


		[AssertionMethod]
		[StringFormatMethod(STRING_FORMAT_PARAM)]
		[ContractAnnotation(COND_FALSE_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void Requires(bool cond, string msg = null, params object[] args)
		{
			if (!cond) {
				ThrowHelper.FailRequire(msg, args);
			}
		}


		internal static void RequiresClr()
		{
			bool isRunningOnMono = Type.GetType("Mono.Runtime") != null;
			Requires(!isRunningOnMono);
		}

		// ReSharper disable once InconsistentNaming
		internal static void RequiresOS(OSPlatform os)
		{
			Requires(RuntimeInformation.IsOSPlatform(os), "OS type");
		}

		internal static unsafe void Requires64Bit()
		{
			Requires(IntPtr.Size == 8 && Environment.Is64BitProcess, "Only 64-bit is supported at the moment.");
//			AssertAllEqualAlt(Offsets.PTR_SIZE, IntPtr.Size, sizeof(void*), 8);
		}

		#region Not null

		internal static void RequiresNotNullOrWhiteSpace(string s, string name)
		{
			Requires(!String.IsNullOrWhiteSpace(s), $"String \"{name}\" cannot be null or whitespace");
		}

		private static void CheckNull(bool b, string name)
		{
			if (!b) {
				ThrowHelper.FailRequire<ArgumentNullException>(name);
			}
		}

		/// <summary>
		///     Specifies a precondition: checks to see if <paramref name="value" /> is <c>null</c>
		/// </summary>
		/// <param name="value">Pointer to check</param>
		/// <param name="name"></param>
		[AssertionMethod]
		[ContractAnnotation(VALUE_NULL_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static unsafe void RequiresNotNull([AsrtCnd(AsrtCndType.IS_NOT_NULL)] void* value, string name)
		{
			CheckNull(value != null, name);
		}

		/// <summary>
		///     Specifies a precondition: checks to see if <paramref name="value" /> is <see cref="IntPtr.Zero" />
		/// </summary>
		/// <param name="value"><see cref="IntPtr" /> to check</param>
		/// <param name="name"></param>
		[AssertionMethod]
		[ContractAnnotation(VALUE_NULL_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RequiresNotNull(IntPtr value, string name)
		{
			CheckNull(value != IntPtr.Zero, name);
		}
		

		/// <summary>
		///     Specifies a precondition: checks to see if <paramref name="value" /> is <c>null</c>
		///     <remarks>
		///         May cause a boxing operation.
		///     </remarks>
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <param name="name"></param>
		/// <typeparam name="T"></typeparam>
		[AssertionMethod]
		[ContractAnnotation(VALUE_NULL_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RequiresNotNull<T>([AsrtCnd(AsrtCndType.IS_NOT_NULL)] T value, string name)
		{
			if (!typeof(T).IsValueType)
				CheckNull(value != null, name);
		}

		/// <summary>
		///     Specifies a precondition: checks to see if <paramref name="value" /> is <c>null</c>
		/// </summary>
		/// <param name="value">Value to check</param>
		/// <param name="name"></param>
		/// <typeparam name="T"></typeparam>
		[AssertionMethod]
		[ContractAnnotation(VALUE_NULL_HALT)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RequiresNotNull<T>([AsrtCnd(AsrtCndType.IS_NOT_NULL)] in T value, string name)
			where T : class
		{
			CheckNull(value != null, name);
		}

		#endregion


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
		internal static void RequiresType<TExpected, TActual>()
		{
			ResolveTypeAction<TExpected, TActual>(TypeException.ThrowTypesNotEqual<Array, TActual>,
			                                      TypeException.ThrowTypesNotEqual<TExpected, TActual>);
		}

		internal static void RequiresTypeNot<TNot, TActual>(string msg = null)
		{
			string baseMsg = $"Type arguments cannot be equal: \"{typeof(TNot).Name}\" and \"{typeof(TActual).Name}\"";

			if (msg != null) {
				baseMsg += $": {msg}";
			}

			Requires(!AssertTypeEqual<TNot, TActual>(), baseMsg);
		}

		/// <summary>
		///     Specifies a precondition: checks to see if <typeparamref name="T" /> is a reference type.
		/// </summary>
		/// <typeparam name="T">Type to verify</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RequiresClassType<T>()
		{
			Requires(!typeof(T).IsValueType, "Type parameter \"<{0}>\" must be a reference type", typeof(T).Name);
		}

		/// <summary>
		///     Specifies a precondition: checks to see if <typeparamref name="T" /> is a value type.
		/// </summary>
		/// <typeparam name="T">Type to verify</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void RequiresValueType<T>()
		{
			Requires(typeof(T).IsValueType, "Type parameter \"<{0}>\" must be a value type", typeof(T).Name);
		}

		#endregion
	}
}