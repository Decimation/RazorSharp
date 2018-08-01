using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using MethodTimer;
using RazorCommon;
using RazorSharp.Pointers;
using RazorSharp.Runtime.CLRTypes;
using RazorSharp.Runtime.CLRTypes.HeapObjects;
using RazorSharp.Utilities;

namespace RazorSharp.Runtime
{

	/// <summary>
	/// Provides utilities for manipulating CLR structures
	///
	/// https://github.com/dotnet/coreclr/blob/fcb04373e2015ae12b55f33fdd0dd4580110db98/src/vm/runtimehandles.h
	/// https://github.com/dotnet/coreclr/blob/fcb04373e2015ae12b55f33fdd0dd4580110db98/src/vm/runtimehandles.cpp
	/// </summary>
	public static unsafe class Runtime
	{
		/// <summary>
		/// Cached for usage with PinHandle
		/// </summary>
		private static readonly RuntimeTypeHandle StringHandle;

		/// <summary>
		/// Heap offset to the first array element.<para></para>
		///  - +8 for MethodTable*<para></para>
		///  - +4 for length<para></para>
		///  - +4 for padding (x64 only)<para></para>
		///
		/// (x64 only)
		/// </summary>
		public static readonly int OffsetToArrayData = IntPtr.Size * 2;

		static Runtime()
		{
			StringHandle = typeof(string).TypeHandle;
		}

		/// <summary>
		/// Manually reads a CLR MethodTable (TypeHandle)
		/// </summary>
		/// <returns>A pointer to the object type's MethodTable</returns>
		public static MethodTable* ReadMethodTable<T>(ref T t)
		{
			// Value types do not have a MethodTable ptr, but they do have a TypeHandle.
			if (typeof(T).IsValueType) {
				return MethodTableOf<T>();
			}

			// We need to get the heap pointer manually because of type constraints
			var ptr  = (Marshal.ReadIntPtr(Unsafe.AddressOf(ref t)));
			var @out = *(MethodTable**) ptr;
			return @out;

			//return (*((HeapObject**) Unsafe.AddressOf(ref t)))->MethodTable;
		}

		#region HeapObjects

		public static ArrayObject** GetArrayObject<T>(ref T t) where T : class
		{
			if (!typeof(T).IsArray) {
				TypeException.Throw<Array, T>();
			}

			return (ArrayObject**) Unsafe.AddressOf(ref t);
		}

		public static StringObject** GetStringObject(ref string s)
		{
			return (StringObject**) Unsafe.AddressOf(ref s);
		}

		public static HeapObject** GetHeapObject<T>(ref T t) where T : class
		{
			HeapObject** h = (HeapObject**) Unsafe.AddressOf(ref t);
			return h;
		}

		#endregion


		public static void WriteMethodTable<TOrig, TNew>(ref TOrig t) where TOrig : class
		{
			WriteMethodTable(ref t, MethodTableOf<TNew>());
		}

		/// <summary>
		/// Returns a type's TypeHandle as a MethodTable
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static MethodTable* MethodTableOf<T>()
		{
			// Array method tables need to be read using ReadMethodTable,
			// they don't have a TypeHandle
			//Assertion.NegativeAssertType<Array, T>();

			if (typeof(T).IsArray) {
				return null;
			}

			return (MethodTable*) typeof(T).TypeHandle.Value;
		}

		public static void WriteMethodTable<T>(ref T t, MethodTable* m) where T : class
		{
			var addrMt = Unsafe.AddressOfHeap(ref t);
			*((MethodTable**) addrMt) = m;

			//var h = GetHeapObject(ref t);
			//(**h).MethodTable = m;
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

		// ReSharper disable once ReturnTypeCanBeEnumerable.Global
		public static LightPointer<FieldDesc>[] GetFieldDescs<T>(BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
		{
			return GetFieldDescs(typeof(T), flags);
		}

		// ReSharper disable once ReturnTypeCanBeEnumerable.Global
		public static LightPointer<FieldDesc>[] GetFieldDescs(Type t,
			BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
		{
			var fields = t.GetFields(flags);
			var arr    = new LightPointer<FieldDesc>[fields.Length];

			for (int i = 0; i < arr.Length; i++) {
				arr[i] = (FieldDesc*) fields[i].FieldHandle.Value;
			}

			PointerUtils.MakeSequential(arr);

			return arr;
		}

		// ReSharper disable once ReturnTypeCanBeEnumerable.Global
		public static LightPointer<MethodDesc>[] GetMethodDescs<T>(
			BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
		{
			return GetMethodDescs(typeof(T), flags);
		}

		// ReSharper disable once ReturnTypeCanBeEnumerable.Global
		public static LightPointer<MethodDesc>[] GetMethodDescs(Type t,
			BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
		{
			var fields = t.GetMethods(flags);


			var arr    = new LightPointer<MethodDesc>[fields.Length];

			for (int i = 0; i < arr.Length; i++) {
				arr[i] = (MethodDesc*) fields[i].MethodHandle.Value;
			}

			return arr;
		}

		public static MethodDesc* GetMethodDesc(Type t, string name,
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
		{
			var methodHandle = t.GetMethod(name, flags).MethodHandle;
			return (MethodDesc*) methodHandle.Value;
		}

		public static MethodDesc* GetMethodDesc<T>(string name,
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
		{
			return GetMethodDesc(typeof(T), name, flags);
		}

		public static FieldDesc* GetFieldDesc(Type t, string name,
			BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
		{
			var fieldHandle = t.GetField(name, flags).FieldHandle;
			return (FieldDesc*) fieldHandle.Value;
		}


		public static FieldDesc* GetFieldDesc<T>(string name,
			BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
		{
			return GetFieldDesc(typeof(T), name, flags);
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