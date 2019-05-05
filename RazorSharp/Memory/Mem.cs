#region

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using InlineIL;
using SimpleSharp.Diagnostics;
using SimpleSharp.Extensions;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native;
using RazorSharp.Native.Win32;

#endregion

// ReSharper disable ConvertToAutoProperty

#endregion

namespace RazorSharp.Memory
{
	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion


	/// <summary>
	///     Provides functions for interacting with memory.
	///     <seealso cref="Unsafe" />
	///     <seealso cref="CSUnsafe" />
	/// </summary>
	public static unsafe class Mem
	{
		public static bool Is64Bit => IntPtr.Size == sizeof(long);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T ReadIL<T>(void* source)
		{
			IL.Emit.Ldarg(nameof(source));
			IL.Emit.Ldobj(typeof(T));
			return IL.Return<T>();
		}

		/// <summary>
		///     Checks whether an address is in range.
		/// </summary>
		/// <param name="hi">The end address</param>
		/// <param name="p">Address to check</param>
		/// <param name="lo">The start address</param>
		/// <returns><c>true</c> if the address is in range; <c>false</c> otherwise</returns>
		public static bool IsAddressInRange(Pointer<byte> hi, Pointer<byte> p, Pointer<byte> lo)
		{
			return p < hi && p >= lo;
		}


		public static Pointer<byte> OffsetAs<TOrig, TAs>(Pointer<byte> p, int origElemCnt)
		{
			return p + OffsetCountAs<TOrig, TAs>(origElemCnt);
		}

		/// <summary>
		///     Calculates the element offset (count) of <paramref name="origElemCnt" /> in terms of <typeparamref name="TAs" />
		/// </summary>
		/// <param name="origElemCnt">Original element count in terms of <typeparamref name="TOrig" /></param>
		/// <typeparam name="TOrig">Type of <paramref name="origElemCnt" /></typeparam>
		/// <typeparam name="TAs">Type to return <paramref name="origElemCnt" /> as</typeparam>
		/// <returns></returns>
		public static int OffsetCountAs<TOrig, TAs>(int origElemCnt)
		{
			int origByteCount = origElemCnt * Unsafe.SizeOf<TOrig>();
			return origByteCount / Unsafe.SizeOf<TAs>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CompleteSize<T>(int elemCnt) => Unsafe.SizeOf<T>() * elemCnt;

		#region Zero

		public static void Destroy<T>(ref T value)
		{
			if (!Runtime.IsStruct(value)) {
				DestroyClass(value);
			}
			else {
				DestroyStruct(ref value);
			}
		}
		
		private static void DestroyStruct<T>(ref T value) /*where T : struct*/
		{
			value = default;
		}

		private static void DestroyClass<T>(T value) /*where T : class*/
		{
			int size = Unsafe.SizeOfData(value);
			Pointer<byte> ptr  = Unsafe.AddressOfData(ref value);
			ptr.ZeroBytes(size);
		}

		public static void Zero<T>(ref T t)
		{
			Zero(Unsafe.AddressOf(ref t).Address, Unsafe.SizeOf<T>());
		}

		public static void Zero(Pointer<byte> ptr, int length)
		{
			for (int i = 0; i < length; i++)
				ptr[i] = 0;
		}

		#endregion


		#region Alloc / free

		/// <summary>
		///     Counts the number of allocations (allocated pointers)
		/// </summary>
		public static int AllocCount { get; private set; }

		public static bool IsMemoryInUse => AllocCount > 0;

		/// <summary>
		///     Allocates basic reference types in the unmanaged heap.
		///     <para>
		///         Once you are done using the memory, dispose using <see cref="Marshal.FreeHGlobal" />,
		///         <see cref="Free{T}(Pointer{T})" />, or <see cref="Free{T}(Pointer{T}, int)" />
		///     </para>
		/// </summary>
		/// <typeparam name="T">
		///     Type to allocate; cannot be <c>string</c> or an array type (for that, use
		///     <see cref="Alloc{T}" />.)
		/// </typeparam>
		/// <returns>A double indirection pointer to the unmanaged instance.</returns>
		public static Pointer<T> AllocInstance<T>() where T : class
		{
			Conditions.Require(!Runtime.IsArrayOrString<T>());


			// Minimum size required for an instance
			int baseSize = Unsafe.SizeOfAuto<T>(SizeOfOptions.BaseInstance);

			// We'll allocate extra bytes (+ IntPtr.Size) for a pointer and write the address of
			// the unmanaged "instance" there, as the CLR can only interpret
			// reference types as a pointer.
			Pointer<byte>        alloc       = Alloc<byte>(baseSize + IntPtr.Size);
			Pointer<MethodTable> methodTable = typeof(T).GetMethodTable();

			// Write the pointer in the extra allocated bytes,
			// pointing to the MethodTable* (skip over the extra pointer and the ObjHeader)
			alloc.WriteAny(alloc.Address + sizeof(MethodTable*) * 2);

			// Write the ObjHeader
			// (this'll already be zeroed, but this is just self-documentation)
			// +4 int (sync block)
			// +4 int (padding, x64)
			alloc.WriteAny(0L, 1);

			// Write the MethodTable
			// Managed pointers point to the MethodTable* in the GC heap
			alloc.WriteAny(methodTable, 2);


			return alloc.Cast<T>();
		}

		/// <summary>
		///     <para>
		///         Allocates <paramref name="elemCnt" /> elements of type <typeparamref name="T" /> in zeroed, unmanaged memory
		///         using <see cref="Marshal.AllocHGlobal(int)" />.
		///     </para>
		///     <para>
		///         If <typeparamref name="T" /> is a reference type, a managed pointer of type <typeparamref name="T" /> will be
		///         created in unmanaged memory, rather than the instance itself. For that, use
		///         <see cref="AllocInstance{T}" />.
		///     </para>
		///     <para>
		///         Once you are done using the memory, dispose using <see cref="Marshal.FreeHGlobal" />,
		///         <see cref="Free{T}(Pointer{T})" />, or <see cref="Free{T}(Pointer{T}, int)" />
		///     </para>
		/// </summary>
		/// <typeparam name="T">Element type to allocate</typeparam>
		/// <returns>A pointer to the allocated memory</returns>
		public static Pointer<T> Alloc<T>(int elemCnt = 1)
		{
			Conditions.Require(elemCnt > 0, nameof(elemCnt));
			int size  = CompleteSize<T>(elemCnt);
			var alloc = Marshal.AllocHGlobal(size);
			Zero(alloc, size);

			AllocCount++;

			return alloc;
		}

		public static Pointer<T> ReAlloc<T>(Pointer<T> ptr, int elemCnt = 1)
		{
			return Marshal.ReAllocHGlobal(ptr.Address, (IntPtr) CompleteSize<T>(elemCnt));
		}

		/// <summary>
		///     <para>Frees memory allocated from <see cref="Alloc{T}" /> using <see cref="Marshal.FreeHGlobal" /></para>
		/// </summary>
		/// <param name="p">Pointer to allocated memory</param>
		public static void Free<T>(Pointer<T> p)
		{
			Marshal.FreeHGlobal(p.Address);
			AllocCount--;
		}

		public static void Free<T>(Pointer<T> p, bool autoZero)
		{
			if (autoZero) {
				uint cb = Kernel32.LocalSize(p.Address);
				Free(p, (int) cb);
			}
			else {
				Free(p);
			}
		}

		public static void Free<T>(Pointer<T> p, int length)
		{
			p.ZeroBytes(length);
			Free(p);
		}

		#region String

		/// <summary>
		///     Allocates a native string from a UTF16 C# string
		/// </summary>
		/// <param name="s">Standard UTF16 C# string</param>
		/// <param name="type">String type</param>
		public static Pointer<byte> AllocString(string s, StringTypes type)
		{
			int           size = s.Length + 1;
			Pointer<byte> ptr  = Alloc<byte>(size);
			ptr.WriteString(s, type);
			Conditions.Assert(ptr.ReadString(type) == s);

			return ptr;
		}

		public static void FreeString(Pointer<byte> ptr)
		{
			int size = ptr.ReadUntil(x => x == 0x00) + 1;
			ptr.ZeroBytes(size);
			Free(ptr);
		}

		#endregion

		#region Code

		public static Pointer<byte> AllocCode(string[] asm, bool isProcess32Bit = false)
		{
			return AllocCode(asm.AsSingleString(), isProcess32Bit);
		}

		public static Pointer<byte> AllocCode(string asm, bool isProcess32Bit = false)
		{
			byte[] code = Assembler.Assemble(asm, isProcess32Bit);
			return AllocCode(code);
		}

		public static Pointer<byte> AllocCode(byte[] opCodes)
		{
			Kernel32.GetNativeSystemInfo(out var si);

			// VirtualAlloc(nullptr, page_size, MEM_COMMIT, PAGE_READWRITE);

			// @formatter:off
			var alloc = Kernel32.VirtualAlloc(IntPtr.Zero, 
			                                  (UIntPtr) si.PageSize, 
			                                  AllocationType.Commit,
			                                  MemoryProtection.ReadWrite);
			// @formatter:on

			AllocCount++;
			Copy(alloc, opCodes);

			// VirtualProtect(buffer, code.size(), PAGE_EXECUTE_READ, &dummy);

			Conditions.Ensure(Kernel32.VirtualProtect(alloc, (uint) opCodes.Length,
			                                          MemoryProtection.ExecuteRead,
			                                          out _));


			return alloc;
		}

		public static void FreeCode(Pointer<byte> fn)
		{
			Conditions.Ensure(Kernel32.VirtualFree(fn.Address, 0, FreeTypes.Release));
			AllocCount--;
		}

		#endregion

		#endregion

		#region Swap

		public static void Swap<T>(void* a, void* b)
		{
			var aval = CSUnsafe.Read<T>(a);
			var bval = CSUnsafe.Read<T>(b);
			CSUnsafe.Write(a, bval);
			CSUnsafe.Write(b, aval);
		}

		public static void Swap<T>(ref T a, ref T b)
		{
			var buf = a;
			a = b;
			b = buf;
		}

		#endregion

		#region Array operations

		public static byte[] ReadBytes(Pointer<byte> p, int size)
		{
			return ReadBytes(p, 0, size);
		}

		public static byte[] ReadBytes(Pointer<byte> p, int byteOffset, int size)
		{
			var rg = new byte[size];
			fixed (byte* b = rg) {
				Copy(b, byteOffset, p, size);
			}

			return rg;
		}

		public static void WriteBytes(Pointer<byte> dest, byte[] src)
		{
			for (int i = 0; i < src.Length; i++)
				dest[i] = src[i];
		}

		#endregion

		#region Read / write

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Pointer<T> ReadPointer<T>(Pointer<byte> ptr, long byteOffset)
		{
			return *(IntPtr*) ptr.Add(byteOffset);
		}

		/*[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write<T>(Pointer<byte> p, int byteOffset, T t)
		{
			CSUnsafe.Write((p + byteOffset).ToPointer(), t);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Read<T>(Pointer<byte> p, int byteOffset = 0)
		{
			return CSUnsafe.Read<T>((p + byteOffset).ToPointer());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T AsRef<T>(Pointer<byte> p, int byteOffset = 0)
		{
			return ref CSUnsafe.AsRef<T>((p + byteOffset).ToPointer());
		}*/

		/// <summary>
		///     <para>This bypasses the restriction that you can't have a pointer to <typeparamref name="T" />,</para>
		///     <para>letting you write very high-performance generic code.</para>
		///     <para>It's dangerous if you don't know what you're doing, but very worth if you do.</para>
		/// </summary>
		public static T ReadUsingTypedRef<T>(Pointer<byte> addr)
		{
			var address = addr.Address;

			var obj = default(T);
			var tr  = __makeref(obj);

			// This is equivalent to shooting yourself in the foot
			// but it's the only high-perf solution in some cases
			// it sets the first field of the TypedReference (which is a pointer)
			// to the address you give it, then it dereferences the value.
			// Better be 10000% sure that your type T is unmanaged/blittable...
			*(IntPtr*) (&tr) = address;

			return __refvalue(tr, T);
		}

		#endregion

		#region Stack

		/// <summary>
		///     Determines whether a variable is on the current thread's stack.
		/// </summary>
		public static bool IsOnStack<T>(ref T t)
		{
			return IsOnStack(Unsafe.AddressOf(ref t).Address);
		}

		public static bool IsOnStack(Pointer<byte> ptr)
		{
//			(IntPtr low, IntPtr high) bounds = Kernel32.GetCurrentThreadStackLimits();
//			return RazorMath.Between(((IntPtr) v).ToInt64(), bounds.low.ToInt64(), bounds.high.ToInt64(), true);

			// https://github.com/dotnet/coreclr/blob/c82bd22d4bab4369c0989a1c2ca2758d29a0da36/src/vm/threads.h
			// 3620
			return IsAddressInRange(StackBase, ptr.Address, StackLimit);
		}

		/// <summary>
		///     Stack Base / Bottom of stack (high address)
		/// </summary>
		public static Pointer<byte> StackBase => Kernel32.GetCurrentThreadStackLimits().High;

		/// <summary>
		///     Stack Limit / Ceiling of stack (low address)
		/// </summary>
		public static Pointer<byte> StackLimit => Kernel32.GetCurrentThreadStackLimits().Low;

		/// <summary>
		///     Should equal <c>4 MB</c> for 64-bit and <c>1 MB</c> for 32-bit
		/// </summary>
		public static long StackSize => StackBase.ToInt64() - StackLimit.ToInt64();

		#endregion

		// todo: the Copy methods should probably be implemented in Pointer for consistency

		#region Copy

		public static void Copy<T>(Pointer<T> dest, int startOfs, Pointer<T> src, int elemCnt)
		{
			for (int i = startOfs; i < elemCnt + startOfs; i++)
				dest[i - startOfs] = src[i];
		}

		public static void Copy<T>(Pointer<T> dest, Pointer<T> src, int elemCnt)
		{
			Copy(dest, 0, src, elemCnt);
		}

		public static void Copy(Pointer<byte> dest, byte[] src)
		{
			fixed (byte* b = src) {
				Copy(dest, 0, b, src.Length);
			}
		}

		public static void Copy<T>(Pointer<T> dest, IEnumerable<T> src)
		{
			dest.WriteAll(src);
		}

		public static void StrCpy(Pointer<char> dest, int startOfs, string src, int elemCnt)
		{
			fixed (char* ptr = src) {
				Copy(dest, startOfs, ptr, elemCnt);
			}
		}

		#endregion
	}
}