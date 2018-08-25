#region

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using RazorCommon;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

#endregion

namespace RazorSharp.Memory
{


	/// <summary>
	///     Indicates that the attributed function is exposed via signature scanning (using <see cref="SigScanner" />
	///     internally).
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class SigcallAttribute : Attribute
	{
		/// <summary>
		///     Module containing <see cref="Signature" />
		/// </summary>
		internal string Module { get; }

		/// <summary>
		///     Unique byte-sequence-string signature of the function
		/// </summary>
		internal string Signature { get; }

		/// <summary>
		/// Explicit name of the function in <see cref="CLRFunctions.FunctionMap"/>
		/// </summary>
		public string FunctionName = null;

		/// <summary>
		///
		/// </summary>
		public bool IsInFunctionMap = false;


		public SigcallAttribute(string module, string signature)
		{
			Module          = module;
			Signature       = signature;
			FunctionName    = null;
			IsInFunctionMap = false;
		}


	}

	public class CLRSigcallAttribute : SigcallAttribute
	{


		/// <summary>
		/// <para>Assumes:</para>
		/// <para><see cref="SigcallAttribute.Module"/> is <see cref="CLRFunctions.ClrDll"/></para>
		/// <para><see cref="SigcallAttribute.Signature"/> is in <see cref="CLRFunctions.FunctionMap"/> as a
		/// <see cref="T:byte[]"/> and <see cref="SigcallAttribute.FunctionName"/> is the name of the annotated
		/// function </para>
		///
		/// </summary>
		public CLRSigcallAttribute() : base(CLRFunctions.ClrDll, null)
		{
			base.IsInFunctionMap = true;
		}

		public CLRSigcallAttribute(string signature) : base(CLRFunctions.ClrDll, signature) { }
	}


	/// <summary>
	///     Contains methods for operating with <see cref="SigcallAttribute" />-annotated functions
	/// </summary>
	public static class SignatureCall
	{
		static SignatureCall()
		{
			TranspileAllKnown();
		}

		private static readonly SigScanner SigScanner = new SigScanner();

		internal static void TranspileAllKnown()
		{
			TranspileIndependent(typeof(CLRFunctions));
			TranspileIndependent<MethodDesc>();
			Transpile<FieldDesc>();
		}

		internal static void TranspileIndependent(Type t)
		{
			var methodInfos = Runtime.GetMethods(t);

			foreach (var methodInfo in methodInfos) {
				ApplySigcallIndependent(methodInfo);
			}
		}

		internal static void TranspileIndependent<T>()
		{
			TranspileIndependent(typeof(T));
		}

		private const string TEXT_SEGMENT = ".text";

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ApplySigcallIndependent(MethodInfo methodInfo)
		{
			SigcallAttribute attr = methodInfo.GetCustomAttribute<SigcallAttribute>();
			if (attr != null) {
				if (UseTextSegment) {
					SigScanner.SelectModuleBySegment(attr.Module, TEXT_SEGMENT);
				}
				else {
					SigScanner.SelectModule(attr.Module);
				}

				IntPtr fn;
				if (attr.IsInFunctionMap) {
					string fnName = attr.FunctionName ?? methodInfo.Name;
					fn = SigScanner.FindPattern(CLRFunctions.FunctionMap[fnName]);
				}
				else {
					fn = SigScanner.FindPattern(attr.Signature);
				}


				Runtime.SetFunctionPointer(methodInfo, fn);
//				Logger.Log("{0}", Hex.ToHex(fn));
			}
		}

		public static bool UseTextSegment { get; set; } = true;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ApplySigcall(Pointer<MethodDesc> md)
		{
			SigcallAttribute attr = md.Reference.Info.GetCustomAttribute<SigcallAttribute>();
			if (attr != null) {
				if (UseTextSegment) {
					SigScanner.SelectModuleBySegment(attr.Module, TEXT_SEGMENT);
				}
				else {
					SigScanner.SelectModule(attr.Module);
				}

				IntPtr fn;
				if (attr.IsInFunctionMap) {
					string fnName = attr.FunctionName ?? md.Reference.Name;
					fn = SigScanner.FindPattern(CLRFunctions.FunctionMap[fnName]);
				}
				else {
					fn = SigScanner.FindPattern(attr.Signature);
				}

				md.Reference.SetFunctionPointer(fn);
//				Logger.Log("{0}", Hex.ToHex(fn));
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
				name = "get_" + name;
			}

			var md = Runtime.GetMethodDesc(t, name);
			ApplySigcall(md);
		}
	}

}