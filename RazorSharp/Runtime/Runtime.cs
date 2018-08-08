using System;
using System.Diagnostics;
using System.Linq;
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
			//StringHandle = typeof(string).TypeHandle;
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

		#region Method Table

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

		private static void WriteMethodTable<TOrig, TNew>(ref TOrig t) where TOrig : class
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
				//return null;
				throw new RuntimeException($"{typeof(T).Name} does not have a TypeHandle.");
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

//		internal static void SpoofMethodTable<TOrig, TSpoof>(ref TOrig t) where TOrig : class
//		{
//			IntPtr handle = typeof(TSpoof) == typeof(string) ? StringHandle.Value : typeof(TSpoof).TypeHandle.Value;
//			WriteMethodTable(ref t, (MethodTable*) handle);
//		}

//		internal static void RestoreMethodTable<TSpoof, TOrig>(ref TOrig t) where TOrig : class
//		{
//			// Make sure it was spoofed in the first place
//			Debug.Assert(t.GetType() == typeof(TSpoof));
//
//			WriteMethodTable(ref t, (MethodTable*) typeof(TOrig).TypeHandle.Value);
//		}

		public static IntPtr ReadMethodTablePointer<T>(ref T t) where T : class
		{
			var heapMem = Unsafe.AddressOfHeap(ref t);
			return Marshal.ReadIntPtr(heapMem);
		}

		#endregion


		#region FieldDesc

		// ReSharper disable once ReturnTypeCanBeEnumerable.Global
		public static Pointer<FieldDesc>[] GetFieldDescs<T>()
		{
			var mt = MethodTableOf<T>();

			var len = mt->FieldDescListLength;

			var lpFd = new Pointer<FieldDesc>[len];
			for (int i = 0; i < len; i++) {
				lpFd[i] = &mt->FieldDescList[i];
			}

			return lpFd;
		}

		private const BindingFlags DefaultFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;


		public static FieldDesc* GetFieldDesc(Type t, string name, BindingFlags flags = DefaultFlags)
		{
			var fieldHandle = t.GetField(name, flags).FieldHandle;
			return (FieldDesc*) fieldHandle.Value;
		}

		public static FieldDesc* GetFieldDesc<T>(string name, BindingFlags flags = DefaultFlags)
		{
			return GetFieldDesc(typeof(T), name, flags);
		}

		#endregion

		#region MethodDesc

		public static Pointer<MethodDesc>[] GetMethodDescs<T>(BindingFlags flags = DefaultFlags)
		{
			return GetMethodDescs(typeof(T), flags);
		}

		public static Pointer<MethodDesc>[] GetMethodDescs(Type t, BindingFlags flags = DefaultFlags)
		{
			var fields = t.GetMethods(flags);


			var arr = new Pointer<MethodDesc>[fields.Length];
			arr = arr.OrderBy(x => x.Address.ToInt64()).ToArray();

			for (int i = 0; i < arr.Length; i++) {
				arr[i] = (MethodDesc*) fields[i].MethodHandle.Value;
			}

			return arr;
		}

		public static MethodDesc* GetMethodDesc(Type t, string name, BindingFlags flags = DefaultFlags)
		{
			var methodHandle = t.GetMethod(name, flags).MethodHandle;
			return (MethodDesc*) methodHandle.Value;
		}

		public static MethodDesc* GetMethodDesc<T>(string name, BindingFlags flags = DefaultFlags)
		{
			return GetMethodDesc(typeof(T), name, flags);
		}

		#endregion


		/// <summary>
		/// Reads a reference type's object header.
		/// </summary>
		/// <returns>A pointer to the reference type's header</returns>
		public static ObjHeader* ReadObjHeader<T>(ref T t) where T : class
		{
			IntPtr data = Unsafe.AddressOfHeap(ref t);

			return (ObjHeader*) (data - IntPtr.Size);
		}


		/// <summary>
		/// Determines whether a type is blittable, that is, they don't
		/// require conversion between managed and unmanaged code.
		/// </summary>
		public static bool IsBlittable<T>()
		{
			if (typeof(T).IsArray || typeof(T) == typeof(string))
				return true;
			return MethodTableOf<T>()->EEClass->IsBlittable;
		}
	}

}