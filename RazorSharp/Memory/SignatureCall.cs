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
using RazorCommon.Utilities;
using RazorSharp.Clr;
using RazorSharp.Utilities;
using Serilog.Context;

#endregion

// ReSharper disable InconsistentNaming

#endregion

namespace RazorSharp.Memory
{
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
			if (UseTextSegment && Environment.Is64BitProcess) // todo: Segments 32-bit
				SigScanner.SelectModuleBySegment(attr.Module, Segments.TEXT_SEGMENT);
			else
				SigScanner.SelectModule(attr.Module);
		}

		private static IntPtr GetCorrespondingFunctionPointer(SigcallAttribute attr, MethodInfo methodInfo)
		{
			const int BYTE_TOL = 3;


			return attr.IsInFunctionMap
				? SigScanner.FindPattern(SigcallMethodMap[methodInfo].Item1,
				                         attr.OffsetGuess == 0
					                         ? SigcallMethodMap[methodInfo].Item2
					                         : attr.OffsetGuess, BYTE_TOL)
				: SigScanner.FindPattern(attr.Signature, attr.OffsetGuess, BYTE_TOL);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ApplySigcallIndependent(MethodInfo methodInfo)
		{
			Debug.Assert(methodInfo != null);
			var attr = methodInfo.GetCustomAttribute<SigcallAttribute>();
			if (attr != null) {
				SelectModule(attr);

				// todo: this is a cheap fix
				if (!attr.IsInFunctionMap && SigcallMethodMap.ContainsKey(methodInfo))
					attr.IsInFunctionMap = true;


				var fn = GetCorrespondingFunctionPointer(attr, methodInfo);
				using (SignatureCallLogContext) {
					Global.Log.Verbose("Binding {Name} to {Addr:X}", methodInfo.Name, fn.ToInt64());
					if (fn == IntPtr.Zero)
						Global.Log.Error("Could not resolve address for func {Name}", methodInfo.Name);
				}

				if (fn != IntPtr.Zero)
				ClrFunctions.SetStableEntryPoint(methodInfo, fn);
			}
		}


		#region Serilog

		// todo: maybe follow this pattern for other classes

		private static IDisposable SignatureCallLogContext =>
			LogContext.PushProperty(Global.CONTEXT_PROP, CONTEXT_PROP_TAG);

		private const string CONTEXT_PROP_TAG = "SignatureCall";

		#endregion

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
			if (IsBound(t)) 
				return;

			MethodInfo[] methodInfos = Runtime.GetMethods(t);


			foreach (var mi in methodInfos) 
				ApplySigcallIndependent(mi);

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
			if (isGetProperty) name = Identifiers.NameOfGetPropertyMethod(name);

			var mi = Runtime.GetMethod(t, name);
			ApplySigcallIndependent(mi);
		}

		#endregion

		#region Cache

		private static void AddToMap(MethodInfo mi, byte[] rgBytes, long offsetGuess = 0)
		{
			if (!SigcallMethodMap.ContainsKey(mi))
				SigcallMethodMap.Add(mi, new Tuple<byte[], long>(rgBytes, offsetGuess));
		}

		public static void Clear()
		{
			SigcallMethodMap.Clear();
		}

		internal static void CacheFunction<T>(int token, byte[] rgBytes, long offsetGuess = 0)
		{
			var mb = typeof(T).Module.ResolveMethod(token);
			AddToMap((MethodInfo) mb, rgBytes, offsetGuess);
		}

		private static void CacheFunction(MethodInfo mi, byte[] rgBytes, long offsetGuess = 0)
		{
			AddToMap(mi, rgBytes, offsetGuess);
		}

		private static void CacheFunction(Type t, string funcName, byte[] rgBytes, long offsetGuess = 0)
		{
			var mi = Runtime.GetAnnotatedMethods<SigcallAttribute>(t, funcName)[0];
			CacheFunction(mi, rgBytes, offsetGuess);
		}

		internal static void CacheFunction<T>(string funcName, byte[] rgBytes, long offsetGuess = 0)
		{
			CacheFunction(typeof(T), funcName, rgBytes, offsetGuess);
		}

		private static string Get(string url)
		{
			using (var wc = new WebClient()) {
				return wc.DownloadString(url);
			}
		}

		public static void ReadCacheJsonUrl(Type[] t, string url)
		{
			string localFile =
				Environment.CurrentDirectory + @"\..\..\..\RazorSharp\Clr\ClrFunctions.json";

			localFile = Path.GetFullPath(localFile);

			string js;

			if (File.Exists(localFile)) {
				Global.Log.Debug("Using local js");
				js = File.ReadAllText(localFile);
			}
			else {
				js = Get(url);
			}


			Conditions.RequiresNotNullOrWhiteSpace(js, nameof(js));
			foreach (var type in t)
				ReadCacheJson(type, js);
		}

		public static void ReadCacheJsonUrl(Type t, string url)
		{
			ReadCacheJsonUrl(new[] {t}, url);
		}

		public static void ReadCacheJson(Type t, string json)
		{
			var js = JObject.Parse(json).GetValue(t.Name);
			var r  = (List<SigCache>) js.ToObject(typeof(List<SigCache>));


			foreach (var data in r) {
				//Global.Log.Debug("Binding {Name}", data.Name);
				byte[] opcodes;
				string offset;
				if (Environment.Is64BitProcess) {
					opcodes = StringUtil.ParseByteArray(data.Opcodes64Signature);
					offset  = data.Offset64String;
				}
				else {
					opcodes = StringUtil.ParseByteArray(data.Opcodes32Signature);
					offset  = data.Offset32String;
				}

				CacheFunction(t, data.Name, opcodes, Int64.Parse(offset, NumberStyles.HexNumber));
			}
		}

		#endregion
	}
}