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
	///     Provides utilities for manipulating CLR structures.
	///     <para>Related files:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/runtimehandles.h</description>
	///         </item>
	///         <item>
	///             <description>/src/vm/runtimehandles.cpp</description>
	///         </item>
	///     </list>
	/// </summary>
	public static unsafe class Runtime
	{
		/// <summary>
		///     Heap offset to the first array element.
		///     <list type="bullet">
		///         <item>
		///             <description>+8 for <c>MethodTable*</c> (<see cref="IntPtr.Size" />)</description>
		///         </item>
		///         <item>
		///             <description>+4 for length <c>(uint)</c></description>
		///         </item>
		///         <item>
		///             <description>+4 for padding <c>(uint)</c> (x64 only)</description>
		///         </item>
		///     </list>
		/// </summary>
		public static readonly int OffsetToArrayData = IntPtr.Size * 2;

		internal static readonly Dictionary<FieldDesc, FieldInfo>   FieldMap;
		internal static readonly Dictionary<MethodDesc, MethodInfo> MethodMap;

		static Runtime()
		{
			FieldMap  = new Dictionary<FieldDesc, FieldInfo>();
			MethodMap = new Dictionary<MethodDesc, MethodInfo>();
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
		///     <para>Manually reads a CLR MethodTable (TypeHandle).</para>
		///     <para>If the type is a value type, the MethodTable will be returned from the TypeHandle.</para>
		/// </summary>
		/// <returns>A pointer to the object type's MethodTable</returns>
		public static MethodTable* ReadMethodTable<T>(ref T t)
		{
			MethodTable* mt;

			// Value types do not have a MethodTable ptr, but they do have a TypeHandle.
			if (typeof(T).IsValueType) {
				return MethodTableOf<T>();
			}

			else {
				// We need to get the heap pointer manually because of type constraints
				IntPtr ptr = Marshal.ReadIntPtr(Unsafe.AddressOf(ref t));
				mt = *(MethodTable**) ptr;
			}


			return mt;

			//return (*((HeapObject**) Unsafe.AddressOf(ref t)))->MethodTable;
		}

		private static void WriteMethodTable<TOrig, TNew>(ref TOrig t) where TOrig : class
		{
			WriteMethodTable(ref t, MethodTableOf<TNew>());
		}

		/// <summary>
		///     Returns a type's TypeHandle as a MethodTable
		///     <exception cref="RuntimeException">If the type is an array</exception>
		/// </summary>
		/// <typeparam name="T">Type to return the corresponding MethodTable for.</typeparam>
		/// <returns></returns>
		public static MethodTable* MethodTableOf<T>()
		{
			// Array method tables need to be read using ReadMethodTable,
			// they don't have a TypeHandle
			//Assertion.NegativeAssertType<Array, T>();

			// From https://github.com/dotnet/coreclr/blob/6bb3f84d42b9756c5fa18158db8f724d57796296/src/vm/typehandle.h#L74:
			// Array MTs are not valid TypeHandles...
			if (typeof(T).IsArray) {
				throw new RuntimeException($"{typeof(T).Name}: Array MethodTables are not valid TypeHandles.");
			}

			return (MethodTable*) typeof(T).TypeHandle.Value;
		}


		public static void WriteMethodTable<T>(ref T t, MethodTable* m) where T : class
		{
			IntPtr addrMt = Unsafe.AddressOfHeap(ref t);
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

		public static FieldDesc* GetFieldDescForFieldInfo(FieldInfo fi)
		{
			if (fi.IsLiteral) {
				throw new RuntimeException("Const field");
			}

			FieldDesc* fd = (FieldDesc*) fi.FieldHandle.Value;
			AddField(*fd, fi);
			return fd;
		}

		/// <summary>
		///     Gets all the <see cref="FieldDesc" /> from <see cref="MethodTable.FieldDescList" />
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		/// <exception cref="RuntimeException">If the type is an array</exception>
		public static Pointer<FieldDesc>[] GetFieldDescs<T>()
		{
			if (typeof(T).IsArray) {
				throw new RuntimeException("Arrays do not have fields");
			}

			MethodTable*         mt   = MethodTableOf<T>();
			int                  len  = mt->FieldDescListLength;
			Pointer<FieldDesc>[] lpFd = new Pointer<FieldDesc>[len];

			for (int i = 0; i < len; i++)
				lpFd[i] = &mt->FieldDescList[i];

			FieldInfo[] fieldHandles = typeof(T).GetFields(DefaultFlags);

			// Remove all const fields
			Collections.RemoveAll(ref fieldHandles, x => x.IsLiteral);

			Trace.Assert(fieldHandles.Length == mt->FieldDescListLength);

			fieldHandles = fieldHandles.OrderBy(x => x.FieldHandle.Value.ToInt64()).ToArray();
			lpFd         = lpFd.OrderBy(x => x.ToInt64()).ToArray();

			for (int i = 0; i < lpFd.Length; i++)
				AddField(lpFd[i].Reference, fieldHandles[i]);


			return lpFd;
		}

		private const BindingFlags DefaultFlags =
			BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;


		/// <summary>
		///     Gets the corresponding FieldDesc for a specified field
		/// </summary>
		/// <param name="t"></param>
		/// <param name="name"></param>
		/// <param name="isAutoProperty"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <exception cref="RuntimeException">If the field is const</exception>
		/// <exception cref="RuntimeException">If the type is an array</exception>
		public static FieldDesc* GetFieldDesc(Type t, string name, bool isAutoProperty = false,
			BindingFlags flags = DefaultFlags)
		{
			if (t.IsArray) {
				throw new RuntimeException("Arrays do not have fields");
			}

			if (isAutoProperty) {
				name = AutoPropertyBackingFieldName(name);
			}

			FieldInfo fieldInfo = t.GetField(name, flags);

			return GetFieldDescForFieldInfo(fieldInfo);
		}

		public static FieldDesc* GetFieldDesc<T>(string name, bool isAutoProperty = false,
			BindingFlags flags = DefaultFlags)
		{
			return GetFieldDesc(typeof(T), name, isAutoProperty, flags);
		}

		#endregion

		#region MethodDesc

		public static MethodDesc* GetMethodDescForMethodInfo(MethodInfo mi)
		{
			MethodDesc* md = (MethodDesc*) mi.MethodHandle.Value;
			AddMethod(*md, mi);
			return md;
		}

		public static Pointer<MethodDesc>[] GetMethodDescs<T>(BindingFlags flags = DefaultFlags)
		{
			return GetMethodDescs(typeof(T), flags);
		}

		public static Pointer<MethodDesc>[] GetMethodDescs(Type t, BindingFlags flags = DefaultFlags)
		{
			MethodInfo[]          methods = t.GetMethods(flags);
			Pointer<MethodDesc>[] arr     = new Pointer<MethodDesc>[methods.Length];


			for (int i = 0; i < arr.Length; i++)
				arr[i] = (MethodDesc*) methods[i].MethodHandle.Value;

			methods = methods.OrderBy(x => x.MethodHandle.Value.ToInt64()).ToArray();
			arr     = arr.OrderBy(x => x.ToInt64()).ToArray();

			for (int i = 0; i < arr.Length; i++)
				AddMethod(arr[i].Reference, methods[i]);

			return arr;
		}

		public static MethodDesc* GetMethodDesc(Type t, string name, BindingFlags flags = DefaultFlags)
		{
			MethodInfo          methodInfo   = t.GetMethod(name, flags);
			RuntimeMethodHandle methodHandle = methodInfo.MethodHandle;
			MethodDesc*         md           = (MethodDesc*) methodHandle.Value;

			AddMethod(*md, methodInfo);
			return md;
		}

		public static MethodDesc* GetMethodDesc<T>(string name, BindingFlags flags = DefaultFlags)
		{
			return GetMethodDesc(typeof(T), name, flags);
		}

		#endregion


		/// <summary>
		///     Reads a reference type's object header.
		/// </summary>
		/// <returns>A pointer to the reference type's header</returns>
		public static ObjHeader* ReadObjHeader<T>(ref T t) where T : class
		{
			IntPtr data = Unsafe.AddressOfHeap(ref t);

			return (ObjHeader*) (data - IntPtr.Size);
		}


		/// <summary>
		///     Determines whether a type is blittable; that is, they don't
		///     require conversion between managed and unmanaged code.
		///     <remarks>
		///         <para>Returned from <see cref="MethodTable.IsBlittable" /></para>
		///         <para>
		///             Note: If the type is an array or <c>string</c>, <see cref="MethodTable.IsBlittable" /> determines it
		///             unblittable,
		///             but <see cref="IsBlittable{T}" /> returns <c>true</c>, as <see cref="GCHandle" /> determines it blittable.
		///         </para>
		///     </remarks>
		/// </summary>
		public static bool IsBlittable<T>()
		{
			// We'll say arrays and strings are blittable cause they're
			// usable with GCHandle
			if (typeof(T).IsArray || typeof(T) == typeof(string)) {
				return true;
			}

			return MethodTableOf<T>()->IsBlittable;
		}

		/// <summary>
		///     Gets the internal name of an auto-property's backing field.
		///     <example>If the auto-property's name is X, the backing field name is &lt;X&gt;k__BackingField.</example>
		/// </summary>
		/// <param name="propname">Auto-property's name</param>
		/// <returns>Internal name of the auto-property's backing field</returns>
		private static string AutoPropertyBackingFieldName(string propname)
		{
			const string backingFieldFormat = "<{0}>k__BackingField";
			return String.Format(backingFieldFormat, propname);
		}
	}

}