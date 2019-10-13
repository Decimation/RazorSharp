using System;
using System.Runtime.InteropServices;
using RazorSharp.CoreClr.Meta;
using RazorSharp.Import.Attributes;
using RazorSharp.Import.Enums;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.Interop
{
	
	public static unsafe partial class FunctionFactory
	{
		/// <summary>
		/// Provides utilities for creating delegates from function pointers.
		/// </summary>
		public static class Delegates
		{
			[ImportForwardCall("COMDelegate", nameof(ConvertToDelegate), ImportCallOptions.Map)]
			private static void* ConvertToDelegate(void* fn, void* mt)
			{
				return Functions.Native.CallReturnPointer((void*) Imports[nameof(ConvertToDelegate)],
				                                          fn, mt);
			}

			public static Delegate CreateSafe(Pointer<byte> ptr, Type t)
			{
				return Marshal.GetDelegateForFunctionPointer(ptr.Address, t);
			}

			public static TDelegate CreateSafe<TDelegate>(Pointer<byte> ptr)
			{
				return Marshal.GetDelegateForFunctionPointer<TDelegate>(ptr.Address);
			}

			/// <summary>
			///     Creates a <see cref="Delegate" /> from a function pointer.
			/// <remarks>
			/// Similar to <see cref="Marshal.GetDelegateForFunctionPointer"/>
			/// </remarks>
			/// </summary>
			/// <param name="ptr">Function pointer</param>
			/// <param name="t"><see cref="Delegate" /> type</param>
			/// <returns>A <see cref="Delegate" /> from <paramref name="ptr" /></returns>
			public static Delegate Create(Pointer<byte> ptr, Type t)
			{
				MetaType mt = t;
				return Converter.ToObject<Delegate>(ConvertToDelegate(ptr.ToPointer(), mt.NativePointer));
			}

			/// <summary>
			///     Creates a <see cref="Delegate" /> from a function pointer.
			/// <remarks>
			/// Similar to <see cref="Marshal.GetDelegateForFunctionPointer{TDelegate}"/>
			/// </remarks>
			/// </summary>
			/// <param name="ptr">Function pointer</param>
			/// <typeparam name="TDelegate"><see cref="Delegate" /> type</typeparam>
			/// <returns>A <see cref="Delegate" /> from <paramref name="ptr" /></returns>
			public static TDelegate Create<TDelegate>(Pointer<byte> ptr) where TDelegate : Delegate
			{
				return (TDelegate) Create(ptr, typeof(TDelegate));
			}
		}
	}
}