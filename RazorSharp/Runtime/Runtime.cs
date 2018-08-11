#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorSharp.Pointers;
using RazorSharp.Runtime.CLRTypes;
using RazorSharp.Runtime.CLRTypes.HeapObjects;
using RazorSharp.Utilities;

#endregion

namespace RazorSharp.Runtime
{

	/// <summary>
	/// Provides utilities for manipulating CLR structures<para></para>
	///
	/// https://github.com/dotnet/coreclr/blob/fcb04373e2015ae12b55f33fdd0dd4580110db98/src/vm/runtimehandles.h<para></para>
	/// https://github.com/dotnet/coreclr/blob/fcb04373e2015ae12b55f33fdd0dd4580110db98/src/vm/runtimehandles.cpp<para></para>
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

		internal static readonly Dictionary<FieldDesc, FieldInfo>   FieldMap;
		internal static readonly Dictionary<MethodDesc, MethodInfo> MethodMap;
		// wip
		internal static readonly Dictionary<MethodTable, TypeInfo> MethodTableMap;

		static Runtime()
		{
			FieldMap  = new Dictionary<FieldDesc, FieldInfo>();
			MethodMap = new Dictionary<MethodDesc, MethodInfo>();
			MethodTableMap = new Dictionary<MethodTable, TypeInfo>();
		}

		private static void AddSet<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey tk, TValue tv)
		{
			if (dict.ContainsValue(tv)) {
				dict.Remove(tk);
				dict.Add(tk, tv);
			}

			if (dict.ContainsKey(tk)) {
				dict[tk] = tv;
			}

			if (!(dict.ContainsKey(tk) || dict.ContainsValue(tv))) {
				dict.Add(tk, tv);
			}
		}

		private static void AddField(FieldDesc fd, FieldInfo fi)
		{
			AddSet(FieldMap, fd, fi);
		}

		private static void AddMethod(MethodDesc md, MethodInfo mi)
		{
			AddSet(MethodMap, md, mi);
		}

		private static void AddMethodTable(MethodTable mt, TypeInfo ti)
		{
			AddSet(MethodTableMap, mt, ti);
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
			MethodTable* mt;

			// Value types do not have a MethodTable ptr, but they do have a TypeHandle.
			if (typeof(T).IsValueType) {
				mt = MethodTableOf<T>();
			}

			else {
				// We need to get the heap pointer manually because of type constraints
				var ptr = Marshal.ReadIntPtr(Unsafe.AddressOf(ref t));
				mt = *(MethodTable**) ptr;
			}


			AddMethodTable(*mt, typeof(T).GetTypeInfo());
			return mt;

			//return (*((HeapObject**) Unsafe.AddressOf(ref t)))->MethodTable;
		}

		private static void WriteMethodTable<TOrig, TNew>(ref TOrig t) where TOrig : class
		{
			WriteMethodTable(ref t, MethodTableOf<TNew>());
		}

		/// <summary>
		/// Returns a type's TypeHandle as a MethodTable
		///
		/// <exception cref="RuntimeException">If the type is an array</exception>
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

		#endregion


		#region FieldDesc

		// ReSharper disable once ReturnTypeCanBeEnumerable.Global
		public static Pointer<FieldDesc>[] GetFieldDescs<T>()
		{
			var mt   = MethodTableOf<T>();
			var len  = mt->FieldDescListLength;
			var lpFd = new Pointer<FieldDesc>[len];

			for (int i = 0; i < len; i++) {
				lpFd[i] = &mt->FieldDescList[i];
			}

			var fieldHandles = typeof(T).GetFields(DefaultFlags);

			// Remove all const fields
			Collections.RemoveAll(ref fieldHandles, x => x.IsLiteral);


			Debug.Assert(fieldHandles.Length == mt->FieldDescListLength);


			fieldHandles = fieldHandles.OrderBy(x => x.FieldHandle.Value.ToInt64()).ToArray();
			lpFd         = lpFd.OrderBy(x => x.ToInt64()).ToArray();

			for (int i = 0; i < lpFd.Length; i++) {
				AddField(lpFd[i].Reference, fieldHandles[i]);
			}


			return lpFd;
		}

		private const BindingFlags DefaultFlags =
			BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;


		/// <summary>
		/// Gets the corresponding FieldDesc for a specified field
		/// </summary>
		/// <param name="t"></param>
		/// <param name="name"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <exception cref="RuntimeException">If the field is const</exception>
		public static FieldDesc* GetFieldDesc(Type t, string name, BindingFlags flags = DefaultFlags)
		{
			var fieldInfo = t.GetField(name, flags);
			if (fieldInfo.IsLiteral)
				throw new RuntimeException("Const field");
			var fieldHandle = fieldInfo.FieldHandle;
			var fd          = (FieldDesc*) fieldHandle.Value;

			AddField(*fd, fieldInfo);
			return fd;
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
			var methods = t.GetMethods(flags);
			var arr     = new Pointer<MethodDesc>[methods.Length];
			Debug.Assert(arr.Length == methods.Length);


			for (int i = 0; i < arr.Length; i++) {
				arr[i] = (MethodDesc*) methods[i].MethodHandle.Value;
			}

			methods = methods.OrderBy(x => x.MethodHandle.Value.ToInt64()).ToArray();
			arr     = arr.OrderBy(x => x.ToInt64()).ToArray();


			for (int i = 0; i < arr.Length; i++) {
				AddMethod(arr[i].Reference, methods[i]);
			}

			return arr;
		}

		public static MethodDesc* GetMethodDesc(Type t, string name, BindingFlags flags = DefaultFlags)
		{
			var methodInfo   = t.GetMethod(name, flags);
			var methodHandle = methodInfo.MethodHandle;
			var md           = (MethodDesc*) methodHandle.Value;

			AddMethod(*md, methodInfo);
			return md;
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
			// We'll say arrays and strings are blittable cause they're
			// usable with GCHandle
			if (typeof(T).IsArray || typeof(T) == typeof(string))
				return true;

			return MethodTableOf<T>()->IsBlittable;
		}
	}

}