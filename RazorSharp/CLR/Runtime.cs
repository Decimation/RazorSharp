#region

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorSharp.CLR.Structures;
using RazorSharp.CLR.Structures.HeapObjects;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Exceptions;

#endregion

namespace RazorSharp.CLR
{

	public enum SpecialFieldTypes
	{
		/// <summary>
		///     The field is an auto-property's backing field.
		/// </summary>
		AutoProperty,

		/// <summary>
		///     The field is normal.
		/// </summary>
		None,
	}

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
		///     These specific <see cref="BindingFlags" /> are used because they correspond with the metadata and structures
		///     in CLR structures such as <see cref="MethodTable" />
		/// </summary>
		private const BindingFlags DefaultFlags =
			BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static string CreateFlagsString(object num, Enum e)
		{
			string join = e.Join();
			return join == String.Empty ? $"{num}" : $"{num} ({e.Join()})";
		}

		#region HeapObjects

		public static ArrayObject** GetArrayObject<T>(ref T t) where T : class
		{
			RazorContract.RequiresType<Array, T>();

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

		#region MethodTable

		/// <summary>
		///     Gets the corresponding <see cref="Type" /> of <see cref="MethodTable" /> <paramref name="pMt" />.
		/// </summary>
		/// <param name="pMt"><see cref="MethodTable" /> to get the <see cref="Type" /> of</param>
		/// <returns>The <see cref="Type" /> of the specified <see cref="MethodTable" /></returns>
		public static Type MethodTableToType(Pointer<MethodTable> pMt)
		{
			return CLRFunctions.JIT_GetRuntimeType(pMt.ToPointer());
		}

		/// <summary>
		///     <para>Manually reads a CLR <see cref="MethodTable" /> (TypeHandle).</para>
		///     <para>
		///         If the type is a value type, the <see cref="MethodTable" /> will be returned from
		///         <see cref="RuntimeTypeHandle.Value" />
		///     </para>
		/// </summary>
		/// <returns>A pointer to type <typeparamref name="T" />'s <see cref="MethodTable" /></returns>
		public static Pointer<MethodTable> ReadMethodTable<T>(ref T t)
		{
			// Value types do not have a MethodTable ptr, but they do have a TypeHandle.
			if (typeof(T).IsValueType) {
				return MethodTableOf<T>();
			}

			// We need to get the heap pointer manually because of type constraints
			IntPtr       ptr = *(IntPtr*) Unsafe.AddressOf(ref t);
			MethodTable* mt  = *(MethodTable**) ptr;

			return mt;
		}

		/// <summary>
		///     Returns a pointer to a type's TypeHandle as a <see cref="MethodTable" />
		/// </summary>
		/// <typeparam name="T">Type to return the corresponding <see cref="MethodTable" /> for.</typeparam>
		/// <returns>A <see cref="Pointer{T}" /> to type <typeparamref name="T" />'s <see cref="MethodTable" /></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Pointer<MethodTable> MethodTableOf<T>()
		{
			return MethodTableOf(typeof(T));
		}


		/// <summary>
		///     Returns a pointer to a type's TypeHandle as a <see cref="MethodTable" />
		/// </summary>
		/// <param name="t">Type to return the corresponding <see cref="MethodTable" /> for.</param>
		/// <returns>A <see cref="Pointer{T}" /> to type <paramref name="t" />'s <see cref="MethodTable" /></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Pointer<MethodTable> MethodTableOf(Type t)
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

		public static Pointer<FieldDesc>[] GetFieldDescs<T>(ref T t)
		{
			Pointer<MethodTable> mt   = ReadMethodTable(ref t);
			int                  len  = mt.Reference.FieldDescListLength;
			Pointer<FieldDesc>[] lpFd = new Pointer<FieldDesc>[len];

			for (int i = 0; i < len; i++) {
				lpFd[i] = &mt.Reference.FieldDescList[i];
			}

			return lpFd;
		}


		/// <summary>
		///     Gets all the <see cref="FieldDesc" />s from <see cref="MethodTable.FieldDescList" />
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Pointer<FieldDesc>[] GetFieldDescs<T>()
		{
			return GetFieldDescs(typeof(T));
		}


		public static Pointer<FieldDesc>[] GetFieldDescs(Type t)
		{
//			RazorContract.Requires(!t.IsArray, "Arrays do not have fields");

			Pointer<MethodTable> mt   = MethodTableOf(t);
			int                  len  = mt.Reference.FieldDescListLength;
			Pointer<FieldDesc>[] lpFd = new Pointer<FieldDesc>[len];

			for (int i = 0; i < len; i++)
				lpFd[i] = &mt.Reference.FieldDescList[i];


			// Adds about 1k ns
//			lpFd = lpFd.OrderBy(x => x.ToInt64()).ToArray();


			return lpFd;
		}

		// todo: add support for getting FieldDesc of fixed buffers (like isAutoProperty) - use an enum probably

		/// <summary>
		///     Gets the corresponding <see cref="FieldDesc" /> for a specified field
		/// </summary>
		/// <param name="t"></param>
		/// <param name="name"></param>
		/// <param name="fieldTypes"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <exception cref="RuntimeException">If the type is an array</exception>
		public static Pointer<FieldDesc> GetFieldDesc(Type t, string name,
			SpecialFieldTypes fieldTypes = SpecialFieldTypes.None, BindingFlags flags = DefaultFlags)
		{
			RazorContract.Requires(!t.IsArray, "Arrays do not have fields");

			switch (fieldTypes) {
				case SpecialFieldTypes.AutoProperty:
					name = SpecialNames.NameOfAutoPropertyBackingField(name);
					break;

				case SpecialFieldTypes.None:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(fieldTypes), fieldTypes, null);
			}


			FieldInfo fieldInfo = t.GetField(name, flags);
			RazorContract.RequiresNotNull(fieldInfo);
			Pointer<FieldDesc> fieldDesc = fieldInfo.FieldHandle.Value;
			RazorContract.Assert(fieldDesc.Reference.Info == fieldInfo);

			return fieldDesc;
		}

		public static Pointer<FieldDesc> GetFieldDesc<T>(string name,
			SpecialFieldTypes fieldTypes = SpecialFieldTypes.None, BindingFlags flags = DefaultFlags)
		{
			return GetFieldDesc(typeof(T), name, fieldTypes, flags);
		}

		internal static FieldInfo[] GetFields<T>()
		{
			return typeof(T).GetFields(DefaultFlags);
		}

		#endregion

		#region MethodDesc

		public static Pointer<MethodDesc>[] GetMethodDescs<T>(BindingFlags flags = DefaultFlags)
		{
			return GetMethodDescs(typeof(T), flags);
		}

		public static Pointer<MethodDesc>[] GetMethodDescs(Type t, BindingFlags flags = DefaultFlags)
		{
			MethodInfo[] methods = t.GetMethods(flags);
			RazorContract.RequiresNotNull(methods);
			Pointer<MethodDesc>[] arr = new Pointer<MethodDesc>[methods.Length];

			for (int i = 0; i < arr.Length; i++) {
				arr[i] = (MethodDesc*) methods[i].MethodHandle.Value;
				RazorContract.Assert(arr[i].Reference.Info.MetadataToken == methods[i].MetadataToken);

				// todo
//				RazorContract.Assert(arr[i].Reference.Info==methods[i]);
			}

//			arr = arr.OrderBy(x => x.ToInt64()).ToArray();

			return arr;
		}

		public static Pointer<MethodDesc> GetMethodDesc(Type t, string name, BindingFlags flags = DefaultFlags)
		{
			MethodInfo methodInfo = t.GetMethod(name, flags);

			RazorContract.RequiresNotNull(methodInfo);

			RuntimeMethodHandle methodHandle = methodInfo.MethodHandle;
			MethodDesc*         md           = (MethodDesc*) methodHandle.Value;

			RazorContract.Assert(md->Info.MetadataToken == methodInfo.MetadataToken);

			// todo
//			RazorContract.Assert(md->Info == methodInfo);
			return md;
		}

		public static Pointer<MethodDesc> GetMethodDesc<T>(string name, BindingFlags flags = DefaultFlags)
		{
			return GetMethodDesc(typeof(T), name, flags);
		}

		#endregion

		#region Sigcall functions

		/// <summary>
		///     Same operation as <see cref="MethodDesc.SetFunctionPointer" /> without using a <see cref="MethodDesc" />
		/// </summary>
		/// <param name="info">Target method</param>
		/// <param name="fn">Pointer to new code</param>
		/// <exception cref="RuntimeException">If the function is <c>virtual</c> or <c>abstract</c></exception>
		internal static void SetFunctionPointer(MethodInfo info, IntPtr fn)
		{
			RazorContract.Requires(!info.IsVirtual && !info.IsAbstract);
			Marshal.WriteIntPtr(info.MethodHandle.Value, IntPtr.Size, fn);
		}

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
			BindingFlags flags = DefaultFlags) where TAttribute : Attribute
		{
			MethodInfo[]     methods = GetAnnotatedMethods<TAttribute>(t, flags);
			List<MethodInfo> matches = new List<MethodInfo>();
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


		/// <summary>
		///     Reads a reference type's <see cref="ObjHeader" />
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

			return MethodTableOf<T>().Reference.IsBlittable;
		}

	}

}