using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using RazorCommon;
using RazorCommon.Extensions;
using RazorCommon.Strings;
using RazorSharp.Utilities;

namespace RazorSharp
{

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	/// <summary>
	/// Provides a way to interpret an array of bytes as different types
	/// </summary>
	public static unsafe class MemoryLayout
	{

		public static string Create<T>(IntPtr p)
		{
			return Create<T>(p, Unsafe.SizeOf<T>());
		}

		private const int SleepMs = 600;

		public static void Step<T>(IntPtr p, int len)
		{
			for (int i = 0; i < len; i++) {
				Console.Clear();
				Point<T>(p, len * Unsafe.SizeOf<T>(), i);
				Thread.Sleep(SleepMs);
			}
		}

		public static void StepInteractive<T>(IntPtr p, int len)
		{
			for (int i = 0; i < len; i++) {
				Console.Clear();
				Point<T>(p, len * Unsafe.SizeOf<T>(), i);
				Console.ReadKey();
			}
		}


		public static void Point<T>(IntPtr p, int len, int offset)
		{
			// Line 1: Memory
			var str = Create<T>(p, len);

			// Adjust the arrow to point to the first char in the sequence

			int[] indexes = str.AllIndexesOf(" ").ToArray();

			int adjOffset;
			if (offset == 0)
				adjOffset = 0;
			else {
				adjOffset = indexes[offset] - str.JSubstring(indexes[offset - 1], indexes[offset] - 1).Length;
			}


			// Line 2: Arrow
			var pt = new string(' ', adjOffset) + "^";

			// Line 3: Address [index]
			var addr = p + adjOffset;


			// [type] [address]
			var addrStr = String.Format("{0}{1} {2}", new string(' ', adjOffset),
				DataTypes.GetStyle<T>(NamingStyles.CSharpKeyword), Hex.ToHex(addr));


			Console.WriteLine("{0}\n{1}\n{2} [{3}]", str, pt, addrStr, offset);
		}


		public static string Create<T>(IntPtr p, int len, ToStringOptions options = ToStringOptions.Hex | ToStringOptions.PadZeros)
		{
			byte[] mem           = Memory.ReadBytes(p, 0, len);
			int    possibleTypes = mem.Length / Unsafe.SizeOf<T>();

			if (typeof(T) == typeof(byte)) {
				return Collections.ToString(mem, options);
			}

			string As(int ofs)
			{
				if (possibleTypes < 1) {
					throw new Exception($"Insufficient memory for type {typeof(T).Name}");
				}

				var alloc = Marshal.AllocHGlobal(mem.Length);
				Memory.Write(alloc, mem);
				IntPtr adj = alloc + ofs * Unsafe.SizeOf<T>();

				string s;
				switch (options) {
					case ToStringOptions.Hex:
						s = Hex.TryCreateHex(CSUnsafe.Read<T>(adj.ToPointer()));
						break;
					default:
						s = CSUnsafe.Read<T>(adj.ToPointer()).ToString();
						break;
				}


				Marshal.FreeHGlobal(alloc);
				return s;
			}

			string[] @out = new string[possibleTypes];
			for (int i = 0; i < possibleTypes; i++) {
				@out[i] = As(i);
			}


			return Collections.ToString(@out, options | ~ToStringOptions.UseCommas);
		}
	}

}