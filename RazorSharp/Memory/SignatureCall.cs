#region

#region

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using RazorSharp.CLR;

#endregion

// ReSharper disable InconsistentNaming

#endregion

namespace RazorSharp.Memory
{


	/// <inheritdoc />
	/// <summary>
	///     Indicates that the attributed function is exposed via signature scanning (using
	///     <see cref="T:RazorSharp.Memory.SigScanner" /> internally). The original function pointer (
	///     <see cref="RuntimeMethodHandle.GetFunctionPointer" />) will be overwritten
	///     with a pointer to the matched signature found by <see cref="SigScanner" />.
	///     <remarks>
	///         <c>virtual</c> and <c>abstract</c> functions cannot be annotated.
	///     </remarks>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class SigcallAttribute : Attribute
	{
		/// <summary>
		///     Module (DLL) containing <see cref="Signature" />
		/// </summary>
		public string Module { get; set; }

		/// <summary>
		///     Unique byte-sequence-string signature of the function
		/// </summary>
		public string Signature { get; set; }

		/// <summary>
		/// </summary>
		public bool IsInFunctionMap { get; set; }

		/// <summary>
		///     Relative to the module's <see cref="SigScanner.BaseAddress" />
		/// </summary>
		public long OffsetGuess { get; set; }

		public SigcallAttribute() { }

		public SigcallAttribute(string module, string signature)
		{
			Module          = module;
			Signature       = signature;
			IsInFunctionMap = false;
		}
	}

	/// <inheritdoc />
	/// <summary>
	///     <see cref="T:RazorSharp.Memory.SigcallAttribute" /> for module <see cref="F:RazorSharp.CLR.CLRFunctions.ClrDll" />
	/// </summary>
	public class CLRSigcallAttribute : SigcallAttribute
	{
		public CLRSigcallAttribute() : base(CLRFunctions.ClrDll, null)
		{
			IsInFunctionMap = true;
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
		///     Fully bound types
		///     Not including individual methods
		/// </summary>
		private static readonly ISet<Type> BoundTypes = new HashSet<Type>();

		private static readonly SigScanner                                  SigScanner   = new SigScanner();
		private const           string                                      TEXT_SEGMENT = ".text";
		public static           bool                                        UseTextSegment { get; set; } = true;
		private static readonly Dictionary<MethodInfo, Tuple<byte[], long>> SigcallMethodMap;

		static SignatureCall()
		{
			SigcallMethodMap = new Dictionary<MethodInfo, Tuple<byte[], long>>();
			CLRFunctions.AddAll();
		}


		public static bool IsBound<T>()
		{
			return IsBound(typeof(T));
		}

		public static bool IsBound(Type t)
		{
			return BoundTypes.Contains(t);
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
			IntPtr fn = attr.IsInFunctionMap
				? SigScanner.FindPattern(SigcallMethodMap[methodInfo].Item1,
					attr.OffsetGuess == 0 ? SigcallMethodMap[methodInfo].Item2 : attr.OffsetGuess)
				: SigScanner.FindPattern(attr.Signature, attr.OffsetGuess);

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

//				Console.WriteLine("Bind {0} @ {1}", methodInfo.Name, Hex.ToHex(fn));
#if DEBUG

//				Console.WriteLine("{0} | {1} | {2}", methodInfo.Name, Hex.ToHex(fn),
//					Hex.ToHex(PointerUtils.Subtract(fn, SigScanner.BaseAddress).Address));
#endif
			}
		}


		#region Normal

		/// <summary>
		///     Binds all functions in type <typeparamref name="T" /> attributed with <see cref="SigcallAttribute" />
		/// </summary>
		/// <typeparam name="T">Type containing unbound <see cref="SigcallAttribute" /> functions</typeparam>
		public static void DynamicBind<T>()
		{
			DynamicBind(typeof(T));
		}


		/// <summary>
		///     Binds all functions in <see cref="Type" /> <paramref name="t" /> attributed with <see cref="SigcallAttribute" />
		/// </summary>
		/// <param name="t">Type containing unbound <see cref="SigcallAttribute" /> functions </param>
		public static void DynamicBind(Type t)
		{
			if (IsBound(t)) {
				return;
			}

			MethodInfo[] methodInfos = Runtime.GetMethods(t);

			foreach (MethodInfo mi in methodInfos) {
				ApplySigcallIndependent(mi);
			}

			BoundTypes.Add(t);
		}

		/// <summary>
		///     Binds function annotated with <see cref="SigcallAttribute" /> with name <paramref name="name" />
		/// </summary>
		/// <param name="name">Name of the unbound <see cref="SigcallAttribute" /> function </param>
		/// <param name="isGetProperty">Whether the function is a <c>get</c> function of a property </param>
		/// <typeparam name="T">
		///     Type containing unbound <see cref="SigcallAttribute" /> function <paramref name="name" />
		/// </typeparam>
		public static void DynamicBind<T>(string name, bool isGetProperty = false)
		{
			DynamicBind(typeof(T), name, isGetProperty);
		}

		/// <summary>
		///     Binds function annotated with <see cref="SigcallAttribute" /> with name <paramref name="name" />
		/// </summary>
		/// <param name="t">Type containing the unbound <see cref="SigcallAttribute" /> function</param>
		/// <param name="name">Name of the unbound <see cref="SigcallAttribute" /> function </param>
		/// <param name="isGetProperty">Whether the function is a <c>get</c> function of a property </param>
		public static void DynamicBind(Type t, string name, bool isGetProperty = false)
		{
			if (isGetProperty) {
				name = SpecialNames.NameOfGetPropertyMethod(name);
			}

			MethodInfo mi = Runtime.GetMethod(t, name);
			ApplySigcallIndependent(mi);
		}

		#endregion

		private static void AddToMap(MethodInfo mi, byte[] rgBytes, long offsetGuess = 0)
		{
			if (!SigcallMethodMap.ContainsKey(mi)) {
				SigcallMethodMap.Add(mi, new Tuple<byte[], long>(rgBytes, offsetGuess));
			}
		}

		public static void CacheFunction(MethodInfo mi, byte[] rgBytes, long offsetGuess = 0)
		{
			AddToMap(mi, rgBytes, offsetGuess);
		}

		public static void CacheFunction(Type t, string funcName, byte[] rgBytes, long offsetGuess = 0)
		{
			MethodInfo mi = Runtime.GetAnnotatedMethods<SigcallAttribute>(t, funcName)[0];
			CacheFunction(mi, rgBytes, offsetGuess);
		}

		public static void CacheFunction<T>(string funcName, byte[] rgBytes, long offsetGuess = 0)
		{
			CacheFunction(typeof(T), funcName, rgBytes, offsetGuess);
		}
	}

}