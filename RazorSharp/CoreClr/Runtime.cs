#region

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorCommon.Diagnostics;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.HeapObjects;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

#endregion

namespace RazorSharp.CoreClr
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
		#region Compare

		private static void AssertCompare(MemberInfo info, IMetaMember meta)
		{
			Conditions.Assert(info.MetadataToken == meta.Token);
			Conditions.Assert(info.Name == meta.Name);
			Conditions.Assert(info == meta.Info);
		}

		#endregion


		/// <summary>
		///     Reads a reference type's <see cref="ObjHeader" />
		/// </summary>
		/// <returns>A pointer to the reference type's header</returns>
		internal static Pointer<ObjHeader> ReadObjHeader<T>(T t) where T : class
		{
			var data = Unsafe.AddressOfHeap(ref t).Address;

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
			if (typeof(T).IsArray || typeof(T) == typeof(string))
				return true;

			return typeof(T).GetMethodTable().Reference.IsBlittable;
		}


		#region HeapObjects

		internal static ArrayObject** GetArrayObject<T>(ref T t) where T : class
		{
			Conditions.RequiresType<Array, T>();

			return (ArrayObject**) Unsafe.AddressOf(ref t);
		}

		internal static StringObject** GetStringObject(ref string s)
		{
			return (StringObject**) Unsafe.AddressOf(ref s);
		}

		internal static HeapObject** GetHeapObject<T>(ref T t) where T : class
		{
			var h = (HeapObject**) Unsafe.AddressOf(ref t);
			return h;
		}

		#endregion

		#region MethodTable

		


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
			if (typeof(T).IsValueType)
				return typeof(T).GetMethodTable();

			// We need to get the heap pointer manually because of type constraints
			var          ptr = *(IntPtr*) Unsafe.AddressOf(ref t);
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
			var typeHandle = t.TypeHandle.Value;


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

		private static Pointer<byte> GetFieldDescDwordAddress(FieldInfo fi)
		{
			Pointer<byte> fieldDesc = fi.FieldHandle.Value;
			fieldDesc.Add(IntPtr.Size);
			return fieldDesc;
		}

		internal static int ReadOffset(FieldInfo fi)
		{
			Pointer<byte> fieldDesc = GetFieldDescDwordAddress(fi);
			var           dword2    = fieldDesc.ReadAny<uint>(1);
			return Bits.ReadBits((int) dword2, Offsets.FIELDDESC_DW2_OFFSET_BITS, 0);
		}

		internal static void WriteOffset(FieldInfo fi, int offset)
		{
			Pointer<byte> fieldDesc = GetFieldDescDwordAddress(fi);
			var           dword2    = fieldDesc.ReadAny<uint>(1);
			fieldDesc.WriteAny(Bits.WriteTo(data: (int) dword2, index: 0,
			                                Offsets.FIELDDESC_DW2_OFFSET_BITS, offset), 1);
		}

		/// <summary>
		///     Reads all <see cref="FieldDesc" />s from <paramref name="mt" />'s <see cref="MethodTable.FieldDescList" />
		///     <remarks>
		///         Note: this does not include literal (<c>const</c>) fields.
		///     </remarks>
		/// </summary>
		/// <returns></returns>
		private static Pointer<FieldDesc>[] ReadFieldDescs(Pointer<MethodTable> mt)
		{
			int len  = mt.Reference.FieldDescListLength;
			var lpFd = new Pointer<FieldDesc>[len];

			for (int i = 0; i < len; i++)
				lpFd[i] = &mt.Reference.FieldDescList[i];

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
			Conditions.RequiresNotNull(fieldInfo, nameof(fieldInfo));
			Pointer<FieldDesc> fieldDesc = fieldInfo.FieldHandle.Value;
			
			if (Environment.Is64BitProcess) {
				Conditions.Assert(fieldDesc.Reference.Info == fieldInfo);
				Conditions.Assert(fieldDesc.Reference.Token == fieldInfo.MetadataToken);
			}


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
		internal static Pointer<FieldDesc> GetFieldDesc(this Type    t, string name,
		                                                BindingFlags flags = ReflectionUtil.ALL_FLAGS)
		{
			Conditions.Assert(!t.IsArray, "Arrays do not have fields"); // ehh...
			// (they have implicit fields such as length)

			return t.GetField(name, flags).GetFieldDesc();
		}

		#endregion

		#region MethodDesc

		internal static Pointer<MethodDesc> GetMethodDesc(this MethodInfo methodInfo)
		{
			Conditions.RequiresNotNull(methodInfo, nameof(methodInfo));

			var methodHandle = methodInfo.MethodHandle;
			var md           = (MethodDesc*) methodHandle.Value;

			Conditions.Assert(md->Info.MetadataToken == methodInfo.MetadataToken);

			// todo
//			RazorContract.Assert(md->Info == methodInfo);
			return md;
		}

		internal static Pointer<MethodDesc> GetMethodDesc(this Type    t, string name,
		                                                  BindingFlags flags = ReflectionUtil.ALL_FLAGS)
		{
			return t.GetMethod(name, flags).GetMethodDesc();
		}

		internal static Pointer<MethodDesc>[] GetMethodDescs(this Type t, BindingFlags flags = ReflectionUtil.ALL_FLAGS)
		{
			MethodInfo[] methods = t.GetMethods(flags);
			Conditions.RequiresNotNull(methods, nameof(methods));
			var arr = new Pointer<MethodDesc>[methods.Length];

			for (int i = 0; i < arr.Length; i++) {
				arr[i] = methods[i].MethodHandle.Value;
				Conditions.Assert(arr[i].Reference.Info.MetadataToken == methods[i].MetadataToken);
			}

//			arr = arr.OrderBy(x => x.ToInt64()).ToArray();

			return arr;
		}

		#endregion
	}
}