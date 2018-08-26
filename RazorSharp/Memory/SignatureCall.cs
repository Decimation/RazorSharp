#region

#region

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;

#endregion

// ReSharper disable InconsistentNaming

#endregion

namespace RazorSharp.Memory
{


	/// <inheritdoc />
	/// <summary>
	///     Indicates that the attributed function is exposed via signature scanning (using
	///     <see cref="T:RazorSharp.Memory.SigScanner" />
	///     internally).
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class SigcallAttribute : Attribute
	{
		/// <summary>
		///     Module (DLL) containing <see cref="Signature" />
		/// </summary>
		internal string Module { get; }

		/// <summary>
		///     Unique byte-sequence-string signature of the function
		/// </summary>
		internal string Signature { get; }

		/// <summary>
		/// </summary>
		internal bool IsInFunctionMap = false;

		/// <summary>
		///     Relative to <see cref="SigScanner.BaseAddress" />
		/// </summary>
		public long OffsetGuess = 0x0;


		public SigcallAttribute(string module, string signature)
		{
			Module    = module;
			Signature = signature;

			IsInFunctionMap = false;
		}


	}

	/// <inheritdoc />
	/// <summary>
	///     <see cref="T:RazorSharp.Memory.SigcallAttribute" /> for module <see cref="F:RazorSharp.CLR.CLRFunctions.ClrDll" />
	/// </summary>
	public class CLRSigcallAttribute : SigcallAttribute
	{


		/// <summary>
		///     <para>Assumes:</para>
		///     <para><see cref="SigcallAttribute.Module" /> is <see cref="CLRFunctions.ClrDll" /></para>
		///     <para>
		///         <see cref="SigcallAttribute.Signature" /> is in <see cref="CLRFunctions.FunctionMap" /> as a
		///         <see cref="T:byte[]" /> and <see cref="SigcallAttribute.FunctionName" /> is the name of the annotated
		///         function
		///     </para>
		/// </summary>
		public CLRSigcallAttribute() : base(CLRFunctions.ClrDll, null)
		{
			base.IsInFunctionMap = true;
		}

		public CLRSigcallAttribute(string signature) : base(CLRFunctions.ClrDll, signature) { }
	}


	// todo: WIP

	/// <summary>
	///     Contains methods for operating with <see cref="SigcallAttribute" />-annotated functions
	/// </summary>
	public static class SignatureCall
	{


		/// <summary>
		///     Fully transpiled types
		///     Not including individual methods
		/// </summary>
		private static readonly ISet<Type> TranspiledTypes = new HashSet<Type>();

		private static readonly SigScanner SigScanner   = new SigScanner();
		private const           string     TEXT_SEGMENT = ".text";
		public static           bool       UseTextSegment { get; set; } = true;

		public static bool IsTranspiled<T>()
		{
			return IsTranspiled(typeof(T));
		}

		public static bool IsTranspiled(Type t)
		{
			return TranspiledTypes.Contains(t);
		}


		#region Independent

		internal static void TranspileIndependent(Type t)
		{
			MethodInfo[] methodInfos = Runtime.GetMethods(t);

			foreach (MethodInfo methodInfo in methodInfos) {
				ApplySigcallIndependent(methodInfo);
			}

			TranspiledTypes.Add(t);
		}

		internal static void TranspileIndependent<T>()
		{
			TranspileIndependent(typeof(T));
		}

		private static void SelectModule(SigcallAttribute attr)
		{
			if (UseTextSegment) {
				SigScanner.SelectModuleBySegment(attr.Module, TEXT_SEGMENT);
			}
			else {
				SigScanner.SelectModule(attr.Module);
			}
		}

		private static IntPtr GetCorrespondingFunctionPointer(SigcallAttribute attr, MethodInfo methodInfo)
		{
			IntPtr fn;
			if (attr.IsInFunctionMap) {
				fn = SigScanner.FindPattern(CLRFunctions.FunctionInfoMap[methodInfo], attr.OffsetGuess);
			}
			else {
				fn = SigScanner.FindPattern(attr.Signature, attr.OffsetGuess);
			}

			return fn;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ApplySigcallIndependent(MethodInfo methodInfo)
		{
			SigcallAttribute attr = methodInfo.GetCustomAttribute<SigcallAttribute>();
			if (attr != null) {
				SelectModule(attr);

				IntPtr fn = GetCorrespondingFunctionPointer(attr, methodInfo);

				Runtime.SetFunctionPointer(methodInfo, fn);

//				Console.WriteLine("{0} -> {1} | {2}", methodInfo.Name, Hex.ToHex(fn), Hex.ToHex(PointerUtils.Subtract(fn, SigScanner.BaseAddress)));
			}
		}

		#endregion


		#region Normal

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ApplySigcall(Pointer<MethodDesc> md)
		{
			SigcallAttribute attr = md.Reference.Info.GetCustomAttribute<SigcallAttribute>();
			if (attr != null) {
				SelectModule(attr);

				IntPtr fn = GetCorrespondingFunctionPointer(attr, md.Reference.Info);
				md.Reference.SetFunctionPointer(fn);

//				Console.WriteLine("{0} -> {1} | {2}", md.Reference.Name, Hex.ToHex(fn), Hex.ToHex(PointerUtils.Subtract(fn, SigScanner.BaseAddress)));
			}
		}

		/// <summary>
		///     Binds all functions in type <typeparamref name="T" /> attributed with <see cref="SigcallAttribute" />
		/// </summary>
		/// <typeparam name="T">Type containing unbound <see cref="SigcallAttribute" /> functions</typeparam>
		public static void Transpile<T>()
		{
			Transpile(typeof(T));
		}


		/// <summary>
		///     Binds all functions in <see cref="Type" /> <paramref name="t" /> attributed with <see cref="SigcallAttribute" />
		/// </summary>
		/// <param name="t">Type containing unbound <see cref="SigcallAttribute" /> functions </param>
		public static void Transpile(Type t)
		{
			Pointer<MethodDesc>[] mds = Runtime.GetMethodDescs(t);

			foreach (Pointer<MethodDesc> md in mds) {
				ApplySigcall(md);
			}

			TranspiledTypes.Add(t);
		}

		/// <summary>
		///     Binds function annotated with <see cref="SigcallAttribute" /> with name <paramref name="name" />
		/// </summary>
		/// <param name="name">Name of the unbound <see cref="SigcallAttribute" /> function </param>
		/// <param name="isGetProperty">Whether the function is a <c>get</c> function of a property </param>
		/// <typeparam name="T">
		///     Type containing unbound <see cref="SigcallAttribute" /> function <paramref name="name" />
		/// </typeparam>
		public static void Transpile<T>(string name, bool isGetProperty = false)
		{
			Transpile(typeof(T), name, isGetProperty);
		}

		/// <summary>
		///     Binds function annotated with <see cref="SigcallAttribute" /> with name <paramref name="name" />
		/// </summary>
		/// <param name="t">Type containing the unbound <see cref="SigcallAttribute" /> function</param>
		/// <param name="name">Name of the unbound <see cref="SigcallAttribute" /> function </param>
		/// <param name="isGetProperty">Whether the function is a <c>get</c> function of a property </param>
		public static void Transpile(Type t, string name, bool isGetProperty = false)
		{
			if (isGetProperty) {
				name = Runtime.NameOfGetPropertyMethod(name);
			}

			Pointer<MethodDesc> md = Runtime.GetMethodDesc(t, name);
			ApplySigcall(md);
		}

		#endregion


	}

}