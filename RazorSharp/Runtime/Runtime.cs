using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RazorCommon;

namespace RazorSharp.Runtime
{

	/// <summary>
	/// Provides utilities for manipulating CLR structures
	/// </summary>
	public static unsafe class Runtime
	{
		/// <summary>
		/// Cached for usage with PinHandle
		/// </summary>
		private static readonly RuntimeTypeHandle StringHandle;

		static Runtime()
		{
			StringHandle = typeof(string).TypeHandle;
		}

		/// <summary>
		/// Reads a CLR MethodTable (TypeHandle)
		/// </summary>
		/// <returns>A pointer to the object type's MethodTable</returns>
		public static MethodTable* ReadMethodTable<T>(ref T t)
		{
			// Value types do not have a MethodTable ptr, but they do have a TypeHandle.
			if (typeof(T).IsValueType) {
				Logger.Log(Level.Warning, Flags.Memory,
					$"typeof({typeof(T).Name}) is a value type, returning TypeHandle MethodTable*");

				return (MethodTable*) typeof(T).TypeHandle.Value;
			}

			// We need to get the heap pointer manually because of type constraints
			var ptr  = (Marshal.ReadIntPtr(Unsafe.AddressOf(ref t)));
			var @out = *(MethodTable**) ptr;


			return @out;
		}


		public static void WriteMethodTable<T>(ref T t, MethodTable* m) where T : class
		{
			var addrMt = Unsafe.AddressOfHeap(ref t);
			*((MethodTable**) addrMt) = m;
		}


		internal static void SpoofMethodTable<TOrig, TSpoof>(ref TOrig t) where TOrig : class
		{
			IntPtr handle = typeof(TSpoof) == typeof(string) ? StringHandle.Value : typeof(TSpoof).TypeHandle.Value;
			WriteMethodTable(ref t, (MethodTable*) handle);
		}

		internal static void RestoreMethodTable<TSpoof, TOrig>(ref TOrig t) where TOrig : class
		{
			// Make sure it was spoofed in the first place
			Debug.Assert(t.GetType() == typeof(TSpoof));

			WriteMethodTable(ref t, (MethodTable*) typeof(TOrig).TypeHandle.Value);
		}


		/// <summary>
		/// Reads a reference type's object header.
		/// </summary>
		/// <returns>A pointer to the reference type's header</returns>
		public static ObjHeader* ReadObjHeader<T>(ref T t) where T : class
		{
			IntPtr data = Unsafe.AddressOfHeap(ref t);

			return (ObjHeader*) (data - IntPtr.Size);
		}


	}

}