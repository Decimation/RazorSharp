using System;
using System.Reflection;
using RazorSharp.CLR;
using RazorSharp.CLR.Meta;
using RazorSharp.Pointers;

namespace RazorSharp.Experimental
{
	// todo: WIP
	public static class Functions
	{
		public static void Hook(MethodInfo target, Pointer<byte> replacement)
		{
			var mm = new MetaMethod(target.GetMethodDesc())
			{
				Function = replacement
			};
		}

		public static void Hook(MethodInfo target, MethodInfo replacement)
		{
			var mmReplacement = new MetaMethod(replacement.GetMethodDesc());
			mmReplacement.PrepareOverride();

			Hook(target, replacement.GetMethodDesc().Reference.Function);
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