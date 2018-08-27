#region

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorSharp.CLR.Structures;
using RazorSharp.CLR.Structures.HeapObjects;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Exceptions;

#endregion

namespace RazorSharp.CLR
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
		///             <description>+ 8 for <c>MethodTable*</c> (<see cref="IntPtr.Size" />)</description>
		///         </item>
		///         <item>
		///             <description>+ 4 for length (<see cref="UInt32" />) </description>
		///         </item>
		///         <item>
		///             <description>+ 4 for padding (<see cref="UInt32" />) (x64 only)</description>
		///         </item>
		///     </list>
		/// </summary>
		public static readonly int OffsetToArrayData = IntPtr.Size * 2;

		private const BindingFlags DefaultFlags =
			BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

		static Runtime() { }


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
		///     <para>Manually reads a CLR <see cref="MethodTable" /> (TypeHandle).</para>
		///     <para>
		///         If the type is a value type, the <see cref="MethodTable" /> will be returned from
		///         <see cref="Type.TypeHandle" />
		///     </para>
		/// </summary>
		/// <returns>A pointer to type <typeparamref name="T" />'s <see cref="MethodTable" /></returns>
		public static Pointer<MethodTable> ReadMethodTable<T>(ref T t)
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
		public static Pointer<MethodTable> MethodTableOf<T>()
		{
			return MethodTableOf(typeof(T));
		}

		public static Pointer<MethodTable> MethodTableOf(Type t)
		{
			// Array method tables need to be read using ReadMethodTable,
			// they don't have a TypeHandle
			//Assertion.NegativeAssertType<Array, T>();

			// From https://github.com/dotnet/coreclr/blob/6bb3f84d42b9756c5fa18158db8f724d57796296/src/vm/typehandle.h#L74:
			// Array MTs are not valid TypeHandles...
			RazorContract.Requires(!t.IsArray,
				$"{t.Name}: Array MethodTables are not valid TypeHandles.");

			return (MethodTable*) t.TypeHandle.Value;
		}


		public static void WriteMethodTable<T>(ref T t, Pointer<MethodTable> m) where T : class
		{
			IntPtr addrMt = Unsafe.AddressOfHeap(ref t);
			*((MethodTable**) addrMt) = (MethodTable*) m;

			//var h = GetHeapObject(ref t);
			//(**h).MethodTable = m;
		}

		#endregion


		#region FieldDesc

		/// <summary>
		///     Gets all the <see cref="FieldDesc" /> from <see cref="MethodTable.FieldDescList" />
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		/// <exception cref="RuntimeException">If the type is an array</exception>
		public static Pointer<FieldDesc>[] GetFieldDescs<T>()
		{
			RazorContract.Requires(!typeof(T).IsArray, "Arrays do not have fields");

			Pointer<MethodTable> mt   = MethodTableOf<T>();
			int                  len  = mt.Reference.FieldDescListLength;
			Pointer<FieldDesc>[] lpFd = new Pointer<FieldDesc>[len];

			for (int i = 0; i < len; i++)
				lpFd[i] = &mt.Reference.FieldDescList[i];


			// Adds about 1k ns
//			lpFd = lpFd.OrderBy(x => x.ToInt64()).ToArray();


			return lpFd;
		}


		/// <summary>
		///     Gets the corresponding <see cref="FieldDesc" /> for a specified field
		/// </summary>
		/// <param name="t"></param>
		/// <param name="name"></param>
		/// <param name="isAutoProperty"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		/// <exception cref="RuntimeException">If the type is an array</exception>
		public static Pointer<FieldDesc> GetFieldDesc(Type t, string name, bool isAutoProperty = false,
			BindingFlags flags = DefaultFlags)
		{
			RazorContract.Requires(!t.IsArray, "Arrays do not have fields");

			if (isAutoProperty) {
				name = NameOfAutoPropertyBackingField(name);
			}

			FieldInfo fieldInfo = t.GetField(name, flags);
			RazorContract.RequiresNotNull(fieldInfo);
			Pointer<FieldDesc> fieldDesc = fieldInfo.FieldHandle.Value;
			RazorContract.Assert(fieldDesc.Reference.Info == fieldInfo);

			return fieldDesc;
		}

		public static Pointer<FieldDesc> GetFieldDesc<T>(string name, bool isAutoProperty = false,
			BindingFlags flags = DefaultFlags)
		{
			return GetFieldDesc(typeof(T), name, isAutoProperty, flags);
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

			return MethodTableOf<T>().Reference.IsBlittable;
		}

		#region Properties

		/// <summary>
		///     Gets the internal name of an auto-property's backing field.
		///     <example>If the auto-property's name is X, the backing field name is &lt;X&gt;k__BackingField.</example>
		/// </summary>
		/// <param name="propname">Auto-property's name</param>
		/// <returns>Internal name of the auto-property's backing field</returns>
		private static string NameOfAutoPropertyBackingField(string propname)
		{
			const string backingFieldFormat = "<{0}>k__BackingField";
			return String.Format(backingFieldFormat, propname);
		}

		internal static string NameOfGetPropertyMethod(string propname)
		{
			const string getPrefix = "get_";
			return getPrefix + propname;
		}

		#endregion


	}

}