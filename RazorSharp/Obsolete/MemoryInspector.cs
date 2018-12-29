#region

using System;
using System.Linq;
using System.Threading;
using RazorSharp.CLR.Fixed;
using RazorSharp.Common;
using RazorSharp.Memory;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.Obsolete
{

	#region

	#endregion

	/// <summary>
	///     Provides a way to interpret memory as different types
	///     <remarks>
	///         Old namespace: Memory
	///     </remarks>
	/// </summary>
	[Obsolete]
	internal static class MemoryInspector
	{

		private const int SleepMs = 600;

		public static string Create<T>(IntPtr p)
		{
			return Create<T>(p, Unsafe.SizeOf<T>());
		}

		public static void Step<T>(IntPtr p, int elemLen)
		{
			for (int i = 0; i < elemLen; i++) {
				Console.Clear();
				Point<T>(p, elemLen * Unsafe.SizeOf<T>(), i);
				Thread.Sleep(SleepMs);
			}
		}

		public static void StepInteractive<T>(IntPtr p, int elemLen)
		{
			for (int i = 0; i < elemLen; i++) {
				Console.Clear();
				Point<T>(p, elemLen * Unsafe.SizeOf<T>(), i);
				Console.ReadKey();
			}
		}

		public static void Point<T>(IntPtr p, int byteLen, int offset)
		{
			// Line 1: Memory
			string str = Create<T>(p, byteLen);

			// Adjust the arrow to point to the first char in the sequence

			int[] indexes = str.AllIndexesOf(" ").ToArray();

			int adjOffset;
			if (offset == 0) {
				adjOffset = 0;
			}
			else {
				adjOffset = indexes[offset] - str.JSubstring(indexes[offset - 1], indexes[offset] - 1).Length;
			}

			// Line 2: Arrow
			string pt = new string(' ', adjOffset) + "^";

			// Line 3: Address [index]
			long addr = p.ToInt64() + offset * Unsafe.SizeOf<T>();

			// [type] [address]
			string addrStr = string.Format("{0}{1} {2}", new string(' ', adjOffset),
				typeof(T).Name, Hex.ToHex(addr));

			Console.WriteLine("{0}\n{1}\n{2} [{3}]", str, pt, addrStr, offset);
		}


		public static string Create<T>(byte[] mem, ToStringOptions options = ToStringOptions.ZeroPadHex)
		{
			int possibleTypes = mem.Length / Unsafe.SizeOf<T>();

			if (typeof(T) == typeof(byte)) {
				return Collections.ToString(mem, options);
			}

			if (possibleTypes < 1) {
				throw new Exception($"Insufficient memory for type {typeof(T).Name}");
			}

			string res = null;

			ObjectPinner.InvokeWhilePinned(mem, delegate
			{
				Pointer<T> ptrMem = Unsafe.AddressOfHeap(ref mem, OffsetType.ArrayData).Address;

				string OfsAs(int o)
				{
					string s = options.HasFlag(ToStringOptions.Hex)
						? Hex.TryCreateHex(ptrMem[o], options)
						: ptrMem[o].ToString();
					return s;
				}

				string[] @out                                   = new string[possibleTypes];
				for (int i = 0; i < possibleTypes; i++) @out[i] = OfsAs(i);

				res = Collections.ToString(list: @out, options & ~ToStringOptions.UseCommas);
			});


			return res;
		}


		public static string Create<T>(IntPtr p, int byteLen, ToStringOptions options = ToStringOptions.ZeroPadHex)
		{
			return Create<T>(Mem.ReadBytes(p, 0, byteLen), options);
		}
	}

}