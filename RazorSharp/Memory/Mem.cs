using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using RazorSharp.CoreClr;
using RazorSharp.Memory.Allocation;
using RazorSharp.Memory.Enums;
using RazorSharp.Memory.Pointers;
using RazorSharp.Model;

// ReSharper disable UnusedParameter.Global

namespace RazorSharp.Memory
{
	/// <summary>
	///     Provides functions for interacting with memory.
	///     <seealso cref="Unsafe" />
	///     <seealso cref="Mem" />
	///     <para></para>
	/// </summary>
	public static unsafe partial class Mem
	{
		public static bool Is64Bit => IntPtr.Size == sizeof(long) && Environment.Is64BitProcess;

		/// <summary>
		/// Represents a <c>null</c> pointer.
		/// <seealso cref="IntPtr.Zero"/>
		/// </summary>
		public static readonly Pointer<byte> Nullptr = null;

		#region Calculation

		public static int FullSize<T>(int elemCnt) => Unsafe.SizeOf<T>() * elemCnt;

		#endregion


		#region Alloc / free

		public static AllocationManager Allocator { get; } = new AllocationManager(Allocators.Local);

		public static void Destroy<T>(ref T value)
		{
			if (!Runtime.Info.IsStruct(value)) {
				int           size = Unsafe.SizeOf(value, SizeOfOptions.Data);
				Pointer<byte> ptr  = Unsafe.AddressOfFields(ref value);
				ptr.ClearBytes(size);
			}
			else {
				value = default;
			}
		}

		/// <summary>
		/// Zeros the memory of <paramref name="value"/>
		/// </summary>
		/// <param name="value">Value to zero</param>
		/// <typeparam name="T">Type of <paramref name="value"/></typeparam>
		public static void Clear<T>(ref T value)
		{
			var ptr = Unsafe.AddressOf(ref value);
			ptr.Clear();
		}

		#endregion

		#region Read / write

		/// <summary>
		/// Reads in a block from a file and converts it to the struct
		/// type specified by the template parameter
		/// </summary>
		public static T ReadFromBinaryReader<T>(BinaryReader reader)
		{
			// Read in a byte array
			byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

			// Pin the managed memory while, copy it out the data, then unpin it
			var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			var value  = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			handle.Free();

			return value;
		}

		public static string ReadString(sbyte* first, int len)
		{
			if (first == null || len <= 0) {
				return null;
			}

//			return Marshal.PtrToStringAuto(new IntPtr(first), len)
//			              .Erase(StringConstants.NULL_TERMINATOR);

			/*byte[] rg = new byte[len];
			Marshal.Copy(new IntPtr(first), rg, 0, rg.Length);
			return Encoding.ASCII.GetString(rg);*/

			return new string(first, 0, len);
		}

		#endregion
	}
}