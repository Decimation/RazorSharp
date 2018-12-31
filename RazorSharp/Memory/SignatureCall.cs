#region

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RazorSharp.CLR;

#endregion

// ReSharper disable InconsistentNaming

#endregion

namespace RazorSharp.Memory
{


	/// <inheritdoc />
	/// <summary>
	///     <para>
	///         Indicates that the attributed function is exposed via signature scanning (using
	///         <see cref="T:RazorSharp.Memory.SigScanner" /> internally).
	///     </para>
	///     <para>
	///         The annotated method's entry point (<see cref="RazorSharp.CLR.Structures.MethodDesc.Function" />)
	///         will be set (<see cref="ClrFunctions.SetStableEntryPoint" />) to the address of the matched signature found by
	///         <see cref="SigScanner" />.
	///     </para>
	///     <para>This allows the calling of non-exported DLL functions, so long as the function signature matches.</para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class SigcallAttribute : Attribute
	{

		/// <summary>
		/// </summary>
		public bool IsInFunctionMap;

		/// <summary>
		///     Module (DLL) containing <see cref="Signature" />
		/// </summary>
		public string Module;

		/// <summary>
		///     Relative to the module's <see cref="SigScanner.BaseAddress" />
		/// </summary>
		public long OffsetGuess;

		/// <summary>
		///     Unique byte-sequence-string signature of the function
		/// </summary>
		public string Signature;

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
	///     <see cref="T:RazorSharp.Memory.SigcallAttribute" /> for module <see cref="ClrFunctions.CLR_DLL" />
	/// </summary>
	public class ClrSigcallAttribute : SigcallAttribute
	{
		public ClrSigcallAttribute() : base(ClrFunctions.CLR_DLL, null)
		{
			IsInFunctionMap = true;
		}

		public ClrSigcallAttribute(string signature) : base(ClrFunctions.CLR_DLL, signature) { }
	}

	// todo: WIP
	// todo: make caching more efficient

	/// <summary>
	///     Contains methods for operating with <see cref="SigcallAttribute" />-annotated functions
	/// </summary>
	public static class SignatureCall
	{


		/// <summary>
		///     <para>Fully bound types</para>
		///     <para>Not including individual methods</para>
		/// </summary>
		private static readonly ISet<Type> BoundTypes = new HashSet<Type>();

		private static readonly SigScanner SigScanner = new SigScanner();

		/// <summary>
		///     Cached functions
		/// </summary>
		private static readonly Dictionary<MethodInfo, Tuple<byte[], long>> SigcallMethodMap;


		static SignatureCall()
		{
			SigcallMethodMap = new Dictionary<MethodInfo, Tuple<byte[], long>>();
		}

		/// <summary>
		///     When <c>true</c>, only the text (code) segment (which contains executable code)
		///     of the target DLL will be scanned by <see cref="SigScanner" />.
		/// </summary>
		public static bool UseTextSegment { get; set; } = true;


		private static void SelectModule(SigcallAttribute attr)
		{
			if (UseTextSegment) {
				SigScanner.SelectModuleBySegment(attr.Module, Segments.TEXT_SEGMENT);
			}
			else {
				SigScanner.SelectModule(attr.Module);
			}
		}

		private static IntPtr GetCorrespondingFunctionPointer(SigcallAttribute attr, MethodInfo methodInfo)
		{
			//Console.WriteLine("{0} | {1}",attr.Signature, attr.IsInFunctionMap);
			IntPtr fn = attr.IsInFunctionMap
				? SigScanner.FindPattern(SigcallMethodMap[methodInfo].Item1,
					attr.OffsetGuess == 0 ? SigcallMethodMap[methodInfo].Item2 : attr.OffsetGuess)
				: SigScanner.FindPattern(attr.Signature, attr.OffsetGuess);

			return fn;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ApplySigcallIndependent(MethodInfo methodInfo)
		{
			Debug.Assert(methodInfo != null);
			SigcallAttribute attr = methodInfo.GetCustomAttribute<SigcallAttribute>();
			if (attr != null) {
				SelectModule(attr);

				// todo: this is a cheap fix
				if (!attr.IsInFunctionMap && SigcallMethodMap.ContainsKey(methodInfo)) {
					attr.IsInFunctionMap = true;
				}


				IntPtr fn = GetCorrespondingFunctionPointer(attr, methodInfo);

				ClrFunctions.SetStableEntryPoint(methodInfo, fn);

//				Console.WriteLine("Bind {0} @ {1}", methodInfo.Name, Hex.ToHex(fn));
#if DEBUG

//				Console.WriteLine("{0} | {1} | {2}", methodInfo.Name, Hex.ToHex(fn),
//					Hex.ToHex(PointerUtils.Subtract(fn, SigScanner.BaseAddress).Address));
#endif
			}
		}

		#region IsBound

		public static bool IsBound<T>()
		{
			return IsBound(typeof(T));
		}

		public static bool IsBound(Type t)
		{
			return BoundTypes.Contains(t);
		}

		#endregion


		#region Bind

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

		#region Cache

		private static void AddToMap(MethodInfo mi, byte[] rgBytes, long offsetGuess = 0)
		{
			if (!SigcallMethodMap.ContainsKey(mi)) {
				SigcallMethodMap.Add(mi, new Tuple<byte[], long>(rgBytes, offsetGuess));
			}
		}

		public static void Clear()
		{
			SigcallMethodMap.Clear();
		}

		internal static void CacheFunction<T>(int token, byte[] rgBytes, long offsetGuess = 0)
		{
			MethodBase mb = typeof(T).Module.ResolveMethod(token);
			AddToMap((MethodInfo) mb, rgBytes, offsetGuess);
		}

		private static void CacheFunction(MethodInfo mi, byte[] rgBytes, long offsetGuess = 0)
		{
			AddToMap(mi, rgBytes, offsetGuess);
		}

		private static void CacheFunction(Type t, string funcName, byte[] rgBytes, long offsetGuess = 0)
		{
			MethodInfo mi = Runtime.GetAnnotatedMethods<SigcallAttribute>(t, funcName)[0];
			CacheFunction(mi, rgBytes, offsetGuess);
		}

		internal static void CacheFunction<T>(string funcName, byte[] rgBytes, long offsetGuess = 0)
		{
			CacheFunction(typeof(T), funcName, rgBytes, offsetGuess);
		}

		private static string Get(string url)
		{
			using (WebClient wc = new WebClient())
				return wc.DownloadString(url);
		}

		public static void ReadCacheJsonUrl(Type[] t, string url)
		{
			foreach (Type type in t) {
				ReadCacheJsonUrl(type, url);
			}
		}

		public static void ReadCacheJsonUrl(Type t, string url)
		{
			ReadCacheJson(t, Get(url));
		}

		public static void ReadCacheJson(Type t, string json)
		{
			var js = JObject.Parse(json).GetValue(t.Name);
			var r  = (List<Data>) js.ToObject(typeof(List<Data>));

			foreach (Data data in r) {
				CacheFunction(t, data.Name, SigScanner.ParsePatternString(data.OpcodesSignature),
					long.Parse(data.OffsetString, NumberStyles.HexNumber));
			}
		}

		#endregion

		internal class Data
		{
			[JsonProperty("name")]
			internal string Name { get; set; }

			[JsonProperty("opcodes")]
			internal string OpcodesSignature { get; set; }

			[JsonProperty("offset")]
			internal string OffsetString { get; set; }

			public override string ToString()
			{
				return string.Format("Name: {0}\nOpcodes: {1}\nOffset: {2}\n", Name, OpcodesSignature, OffsetString);
			}
		}


	}

}