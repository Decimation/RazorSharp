#region

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorSharp.CLR.Meta;
using RazorSharp.CLR.Structures;
using RazorSharp.CLR.Structures.HeapObjects;
using RazorSharp.Common;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Exceptions;

#endregion

namespace RazorSharp.CLR
{
	/// <summary>
	///     Provides utilities for manipulating, reading, and writing CLR structures.
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
	internal static unsafe class Runtime
	{
		/// <summary>
		///     These specific <see cref="BindingFlags" /> are used because they correspond with the metadata and structures
		///     in CLR structures such as <see cref="MethodTable" />
		/// </summary>
		private const BindingFlags DefaultFlags = BindingFlags.Instance | BindingFlags.NonPublic |
		                                          BindingFlags.Public | BindingFlags.Static;

		#region Compare

		private static void AssertCompare(MemberInfo info, IMetaMember meta)
		{
			RazorContract.Assert(info.MetadataToken == meta.Token);
			RazorContract.Assert(info.Name == meta.Name);
			RazorContract.Assert(info == meta.Info);
		}

		#endregion


		/// <summary>
		///     Reads a reference type's <see cref="ObjHeader" />
		/// </summary>
		/// <returns>A pointer to the reference type's header</returns>
		internal static ObjHeader* ReadObjHeader<T>(ref T t) where T : class
		{
			IntPtr data = Unsafe.AddressOfHeap(ref t).Address;

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
		internal static bool IsBlittable<T>()
		{
			// We'll say arrays and strings are blittable cause they're
			// usable with GCHandle
			if (typeof(T).IsArray || typeof(T) == typeof(string)) {
				return true;
			}

			return typeof(T).GetMethodTable().Reference.IsBlittable;
		}


		#region HeapObjects

		internal static ArrayObject** GetArrayObject<T>(ref T t) where T : class
		{
			RazorContract.RequiresType<Array, T>();

			return (ArrayObject**) Unsafe.AddressOf(ref t);
		}

		internal static StringObject** GetStringObject(ref string s)
		{
			return (StringObject**) Unsafe.AddressOf(ref s);
		}

		internal static HeapObject** GetHeapObject<T>(ref T t) where T : class
		{
			HeapObject** h = (HeapObject**) Unsafe.AddressOf(ref t);
			return h;
		}

		#endregion

		#region MethodTable

		/// <summary>
		///     Gets the corresponding <see cref="Type" /> of <see cref="MethodTable" /> <paramref name="pMt" />.
		/// </summary>
		/// <param name="pMt"><see cref="MethodTable" /> to get the <see cref="Type" /> of</param>
		/// <returns>The <see cref="Type" /> of the specified <see cref="MethodTable" /></returns>
		internal static Type MethodTableToType(Pointer<MethodTable> pMt)
		{
			return ClrFunctions.JIT_GetRuntimeType(pMt.ToPointer());
		}

		/// <summary>
		///     <para>Manually reads a CLR <see cref="MethodTable" /> (TypeHandle).</para>
		///     <para>
		///         If the type is a value type, the <see cref="MethodTable" /> will be returned from
		///         <see cref="RuntimeTypeHandle.Value" />
		///     </para>
		/// </summary>
		/// <returns>A pointer to type <typeparamref name="T" />'s <see cref="MethodTable" /></returns>
		internal static Pointer<MethodTable> ReadMethodTable<T>(ref T t)
		{
			// Value types do not have a MethodTable ptr, but they do have a TypeHandle.
			if (typeof(T).IsValueType) {
				return typeof(T).GetMethodTable();
			}

			// We need to get the heap pointer manually because of type constraints
			IntPtr       ptr = *(IntPtr*) Unsafe.AddressOf(ref t);
			MethodTable* mt  = *(MethodTable**) ptr;

			return mt;
		}


		/// <summary>
		///     Returns a pointer to a type's TypeHandle as a <see cref="MethodTable" />
		/// </summary>
		/// <param name="t">Type to return the corresponding <see cref="MethodTable" /> for.</param>
		/// <returns>A <see cref="Pointer{T}" /> to type <paramref name="t" />'s <see cref="MethodTable" /></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Pointer<MethodTable> GetMethodTable(this Type t)
		{
			IntPtr typeHandle = t.TypeHandle.Value;


			// Special case:
			// If an object is an array, its actual MethodTable* is stored at the address pointed to by its
			// given MethodTable* returned from TypeHandle.Value (which is invalid), offset by ARRAY_MT_PTR_OFFSET bytes.

			// See ARRAY_MT_PTR_OFFSET documentation

			// Example:
			// 00 00 00 00 00 00 18 91 C6 83 F9 7F
			//				     ^

			// I don't know why this is, but whatever

			return t.IsArray ? Mem.ReadPointer<MethodTable>(typeHandle, Offsets.ARRAY_MT_PTR_OFFSET) : typeHandle;
		}

		#endregion

		#region FieldDesc

		/// <summary>
		///     Reads all <see cref="FieldDesc" />s from <paramref name="mt" />'s <see cref="MethodTable.FieldDescList" />
		///     <remarks>
		///         Note: this does not include literal (<c>const</c>) fields.
		///     </remarks>
		/// </summary>
		/// <returns></returns>
		private static Pointer<FieldDesc>[] ReadFieldDescs(Pointer<MethodTable> mt)
		{
			int                  len  = mt.Reference.FieldDescListLength;
			Pointer<FieldDesc>[] lpFd = new Pointer<FieldDesc>[len];

			for (int i = 0; i < len; i++) {
				lpFd[i] = &mt.Reference.FieldDescList[i];
			}

			return lpFd;
		}

		internal static Pointer<FieldDesc>[] GetFieldDescs<T>(T t)
		{
			return ReadFieldDescs(ReadMethodTable(ref t));
		}


		internal static Pointer<FieldDesc>[] GetFieldDescs(this Type t)
		{
//			RazorContract.Requires(!t.IsArray, "Arrays do not have fields");
			// Adds about 1k ns
//			lpFd = lpFd.OrderBy(x => x.ToInt64()).ToArray();
			return ReadFieldDescs(t.GetMethodTable());
		}


		// todo: add support for getting FieldDesc of fixed buffers (like isAutoProperty) - use an enum probably

		internal static Pointer<FieldDesc> GetFieldDesc(this FieldInfo fieldInfo)
		{
			RazorContract.RequiresNotNull(fieldInfo);
			Pointer<FieldDesc> fieldDesc = fieldInfo.FieldHandle.Value;
			RazorContract.Assert(fieldDesc.Reference.Info == fieldInfo);
			RazorContract.Assert(fieldDesc.Reference.Token == fieldInfo.MetadataToken);


			return fieldDesc;
		}

		/// <summary>
		///     Gets the corresponding <see cref="FieldDesc" /> for a specified field
		/// </summary>
		/// <param name="t"></param>
		/// <param name="name"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <exception cref="RuntimeException">If the type is an array</exception>
		internal static Pointer<FieldDesc> GetFieldDesc(this Type t, string name, BindingFlags flags = DefaultFlags)
		{
			RazorContract.Requires(!t.IsArray, "Arrays do not have fields"); // ehh...

			return t.GetField(name, flags).GetFieldDesc();
		}


		internal static FieldInfo[] GetFields<T>()
		{
			return GetFields(typeof(T));
		}

		/// <summary>
		///     Gets the corresponding <see cref="FieldInfo" />s equivalent to the fields
		///     in <see cref="MethodTable.FieldDescList" />
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		internal static FieldInfo[] GetFields(Type t)
		{
			FieldInfo[] fields = t.GetFields(DefaultFlags);
			Collections.RemoveAll(ref fields, f => f.IsLiteral);
			return fields;
		}

		#endregion

		#region MethodDesc

		internal static Pointer<MethodDesc> GetMethodDesc(this MethodInfo methodInfo)
		{
			RazorContract.RequiresNotNull(methodInfo);

			RuntimeMethodHandle methodHandle = methodInfo.MethodHandle;
			MethodDesc*         md           = (MethodDesc*) methodHandle.Value;

			RazorContract.Assert(md->Info.MetadataToken == methodInfo.MetadataToken);

			// todo
//			RazorContract.Assert(md->Info == methodInfo);
			return md;
		}

		internal static Pointer<MethodDesc> GetMethodDesc(this Type t, string name, BindingFlags flags = DefaultFlags)
		{
			return t.GetMethod(name, flags).GetMethodDesc();
		}

		internal static Pointer<MethodDesc>[] GetMethodDescs(this Type t, BindingFlags flags = DefaultFlags)
		{
			MethodInfo[] methods = t.GetMethods(flags);
			RazorContract.RequiresNotNull(methods);
			Pointer<MethodDesc>[] arr = new Pointer<MethodDesc>[methods.Length];

			for (int i = 0; i < arr.Length; i++) {
				arr[i] = methods[i].MethodHandle.Value;
				RazorContract.Assert(arr[i].Reference.Info.MetadataToken == methods[i].MetadataToken);
			}

//			arr = arr.OrderBy(x => x.ToInt64()).ToArray();

			return arr;
		}

		#endregion

		#region Sigcall functions

		internal static MethodInfo[] GetAnnotatedMethods<TAttribute>(Type t, BindingFlags flags = DefaultFlags)
			where TAttribute : Attribute
		{
			MethodInfo[]     methods           = t.GetMethods(flags);
			List<MethodInfo> attributedMethods = new List<MethodInfo>();

			foreach (MethodInfo t1 in methods) {
				TAttribute attr = t1.GetCustomAttribute<TAttribute>();
				if (attr != null) {
					attributedMethods.Add(t1);
				}
			}

			return attributedMethods.ToArray();
		}

		internal static MethodInfo[] GetAnnotatedMethods<TAttribute>(Type t, string name,
			BindingFlags                                                  flags = DefaultFlags)
			where TAttribute : Attribute
		{
			MethodInfo[]     methods = GetAnnotatedMethods<TAttribute>(t, flags);
			var matches = new List<MethodInfo>();
			foreach (MethodInfo v in methods) {
				if (v.Name == name) {
					matches.Add(v);
				}
			}

			return matches.ToArray();
		}


		internal static MethodInfo GetMethod(Type t, string name, BindingFlags flags = DefaultFlags)
		{
			return t.GetMethod(name, flags);
		}

		internal static MethodInfo[] GetMethods(Type t, BindingFlags flags = DefaultFlags)
		{
			return t.GetMethods(flags);
		}

		#endregion
	}
}