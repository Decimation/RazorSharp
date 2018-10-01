#region

using System;
using System.Reflection;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;
using static RazorSharp.CLR.Runtime;

#endregion

namespace RazorSharp.Experimental
{

	/// <summary>
	///     <remarks>
	///         Old namespace: Experimental
	///     </remarks>
	/// </summary>
	internal static unsafe class FunctionHooker
	{
		private struct PointerPair
		{
			private readonly IntPtr m_orig;
			private readonly IntPtr m_hook;

			public IntPtr Original => m_orig;
			public IntPtr Hook     => m_hook;

			internal PointerPair(IntPtr orig, IntPtr hook)
			{
				m_orig = orig;
				m_hook = hook;
			}

			public bool Equals(PointerPair other)
			{
				return m_orig.Equals(other.m_orig) && m_hook.Equals(other.m_hook);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) {
					return false;
				}

				return obj is PointerPair other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked {
					return (m_orig.GetHashCode() * 397) ^ m_hook.GetHashCode();
				}
			}
		}

//		private static readonly Dictionary<IntPtr, IntPtr> OrigFunctions;

		static FunctionHooker()
		{
//			OrigFunctions = new Dictionary<IntPtr, IntPtr>();
		}


//todo
		public static void Restore<TType>(string fnName)
		{
//			var md   = GetMethodDesc<TType>(fnName);
//			var orig = OrigFunctions[md->MethodInfo].Original;
//
//			Console.WriteLine("Restoring {0} | {1}", md->Name, Hex.ToHex(orig));
//			Console.WriteLine(Hex.ToHex(orig));
//			md->SetFunctionPointer(orig);

//			md->Prepare();
		}


		public static void HookFunction<TType>(string fnName, Action action)
		{
			MethodInfo mi = action.Method;

//			Console.WriteLine(Hex.ToHex(mi.MethodHandle.GetFunctionPointer()));
			HookFunction<TType>(fnName, mi.MethodHandle.GetFunctionPointer());
		}

		public static void HookFunction<TType>(string fnName, MethodInfo mi)
		{
			HookFunction<TType>(fnName, mi.MethodHandle.GetFunctionPointer());
		}

		/*public static void HookFunction<TOrig, TNew>(string origFnName, string newFnName)
		{
			var orig = GetMethodDesc<TOrig>(origFnName);
			AddFunction(orig->MethodInfo);
			orig->SetFunctionPointer(GetMethodDesc<TNew>(newFnName)->Function);
		}*/


		// Base function
		public static void HookFunction<TType>(string fnName, IntPtr fn)
		{
			Pointer<MethodDesc> md = GetMethodDesc<TType>(fnName);

//			AddFunction(md->MethodInfo, fn);

//			md.Reference.SetFunctionPointer(fn);
		}
	}

}