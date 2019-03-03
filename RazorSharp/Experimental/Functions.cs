#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using RazorSharp.CLR;
using RazorSharp.CLR.Meta;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.Experimental
{
	// todo: WIP
	public static class Functions
	{
		/// <summary>
		///     MethodInfo: Hooked function
		///     Pointer: New function pointer
		/// </summary>
		private static readonly Dictionary<MethodInfo, Pointer<byte>> FuncMap;

		/// <summary>
		///     MethodInfo: New function
		///     Pointer: Old function pointer
		/// </summary>
		private static readonly Dictionary<MethodInfo, Pointer<byte>> OrigMap;

		static Functions()
		{
			if (Debugger.IsAttached) //todo
				throw new NotSupportedException("Hooking is not yet supported when a debugger is attached");

			FuncMap = new Dictionary<MethodInfo, Pointer<byte>>();
			OrigMap = new Dictionary<MethodInfo, Pointer<byte>>();
		}

		public static TDelegate Orig<TDelegate>(MethodInfo current) where TDelegate : Delegate
		{
			return Marshal.GetDelegateForFunctionPointer<TDelegate>(OrigMap[current].Address);
		}


		// todo: rewrite these methods because of duplicate code

		// Base
		public static void Hook(MethodInfo target, MethodInfo replacement)
		{
			var mmReplacement = new MetaMethod(replacement.GetMethodDesc());
			mmReplacement.PrepareOverride();

			Pointer<byte> replacementFunc = mmReplacement.Function;
			OrigMap.Add(replacement, replacementFunc);


			// ...

			Pointer<MethodDesc> md = target.GetMethodDesc();

			var mm = new MetaMethod(md);
			mm.PrepareOverride();
			var origFunc = md.Reference.Function;

			mm.Function = replacementFunc;

			FuncMap.Add(target, origFunc);
		}

		public static void Hook(Type host, string hostName, Type subject, string subjName)
		{
			Hook(Meta.GetType(host).Methods[hostName].MethodInfo,
			     Meta.GetType(subject).Methods[subjName].MethodInfo);
		}

		public static void Hook(MethodInfo target, Action replacement)
		{
			Hook(target, replacement.Method);
		}

		public static void Hook<TResult, T1, T2>(MethodInfo target, Func<TResult, T1, T2> fn)
		{
			Hook(target, fn.Method);
		}

		public static void InjectJmp<TResult, T1, T2>(Pointer<byte> addr, Func<TResult, T1, T2> fn)
		{
			InjectJmp(addr, fn.Method);
		}

		public static void InjectJmp(Pointer<byte> addr, Delegate fn)
		{
			InjectJmp(addr, fn.Method);
		}

		public static void InjectJmp(Pointer<byte> addr, Action fn)
		{
			InjectJmp(addr, fn.Method);
		}

		public static void InjectJmp(Pointer<byte> addr, MethodInfo methodInfo)
		{
			var mm = new MetaMethod(methodInfo.GetMethodDesc());
			mm.PrepareOverride();
			Pointer<byte> targetAddr = mm.Function;

			// Opcode: E9 cd
			// Mnemonic: JMP rel32
			// Description: Jump near, relative, displacement relative to next instruction.
			addr.Write(0xE9);
			addr++; // Move over jmp opcode
			Pointer<byte> rel32 = targetAddr - addr;
			rel32 += sizeof(int); // Add size of rel32 arg

			addr.WriteAny(rel32.ToInt32());
			Console.WriteLine("done inject");
		}
	}
}