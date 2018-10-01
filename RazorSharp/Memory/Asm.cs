#region

#region

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using RazorSharp.Native;
using RazorSharp.Native.Enums;

#endregion

// ReSharper disable InconsistentNaming

#endregion

namespace RazorSharp.Memory
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion

	/// <summary>
	///     Class for executing Assembly code
	/// </summary>
	public static unsafe class Asm
	{
		private delegate void Exec();

		private delegate IntPtr GetValue();

		/// <summary>
		///     Execute arbitrary Assembly opcodes
		/// </summary>
		/// <param name="memory">Opcodes</param>
		public static void asm(params byte[] memory)
		{
			IntPtr buf = Kernel32.VirtualAlloc(IntPtr.Zero, (UIntPtr) memory.Length, AllocationType.Commit,
				MemoryProtection.ExecuteReadWrite);
			Marshal.Copy(memory, 0, buf, memory.Length);
			Marshal.GetDelegateForFunctionPointer<Exec>(buf)();
			if (!Kernel32.VirtualFree(buf, (uint) memory.Length, FreeTypes.Decommit)) {
//				Logger.Log(Level.Error, Flags.Memory, "Asm::asm failed to free memory");
			}
		}

		/// <summary>
		///     Execute arbitrary Assembly opcodes and return the result.
		/// </summary>
		/// <param name="hex">String of hexadecimal opcodes, in the format of "0x??"</param>
		/// <typeparam name="T">Type to return</typeparam>
		/// <returns>The value returned by the execution</returns>
		public static T asm<T>(string hex)
		{
			string[] bytestrs = hex.Split(' ');
			byte[]   mem      = new byte[bytestrs.Length];

			for (int i = 0; i < mem.Length; i++) {
				bytestrs[i] = bytestrs[i].Replace("0x", "");
				if (bytestrs[i] == "0") {
					bytestrs[i] += "0";
				}

				mem[i] = Byte.Parse(bytestrs[i], NumberStyles.HexNumber);
			}


			/*var b = Enumerable.Range(0, hex.Length)
				.Where(x => x % 2 == 0)
				.Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
				.ToArray();*/

			return asm<T>(mem);
		}

		/// <summary>
		///     Execute arbitrary Assembly opcodes and return the result.
		/// </summary>
		/// <param name="memory">Byte array of opcodes</param>
		/// <typeparam name="T">Type to return</typeparam>
		/// <returns>The value returned by the execution</returns>
		public static T asm<T>(params byte[] memory)
		{
			IntPtr buf = Kernel32.VirtualAlloc(IntPtr.Zero, (UIntPtr) memory.Length, AllocationType.Commit,
				MemoryProtection.ExecuteReadWrite);
			Marshal.Copy(memory, 0, buf, memory.Length);
			IntPtr p = Marshal.GetDelegateForFunctionPointer<GetValue>(buf)();

			if (!Kernel32.VirtualFree(buf, (uint) memory.Length, FreeTypes.Decommit)) {
//				Logger.Log(Level.Error, Flags.Memory, "Asm::asm failed to free memory");
			}

			return CSUnsafe.Read<T>(&p);
		}


	}

}