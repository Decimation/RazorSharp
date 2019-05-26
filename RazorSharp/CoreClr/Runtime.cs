#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using InlineIL;
using JetBrains.Annotations;
using SimpleSharp.Diagnostics;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.EE;
using RazorSharp.CoreClr.Structures.HeapObjects;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Symbols;
using RazorSharp.Utilities;
using SimpleSharp.Strings;
using Unsafe = RazorSharp.Memory.Unsafe;

#endregion

namespace RazorSharp.CoreClr
{
	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

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
	public static unsafe class Runtime
	{
		public static TDelegate GetClrFunction<TDelegate>(string name) where TDelegate : Delegate
		{
			return Clr.ClrSymbols.GetFunction<TDelegate>(name);
		}


		public static T StackAllocObject<T>(Pointer<byte> stack, params object[] args)
		{
			var methodTable = typeof(T).GetMethodTable();

			var objPtr = stack.Address + Offsets.ObjectOverhead;
			
			// Write the pointer in the extra allocated bytes,
			// pointing to the MethodTable* (skip over the extra pointer and the ObjHeader)
			stack.WriteAny(objPtr);

			// Write the ObjHeader
			// (this'll already be zeroed, but this is just self-documentation)
			// +4 int (sync block)
			// +4 int (padding, x64)
			stack.WriteAny(0L, 1);

			// Write the MethodTable
			// Managed pointers point to the MethodTable* in the GC heap
			stack.WriteAny(methodTable, 2);
			
			var cpy = objPtr;
			var value = CSUnsafe.Read<T>(&cpy);
			
			return value;
		}
		
		public static T AllocObject<T>(params object[] args)
		{
			var value = GlobalHeap.AllocateObject<T>();
			RunConstructor(value, args);
			return value;
		}

		#region Is

		#region Unmanaged

		private class U<T> where T : unmanaged { }

		public static bool IsUnmanaged(this Type t)
		{
			try {
				// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
				typeof(U<>).MakeGenericType(t);
				return true;
			}
			catch (Exception) {
				return false;
			}
		}

		public static bool IsUnmanaged<T>() => typeof(T).IsUnmanaged();

		#endregion

		public static bool IsBoxed<T>(in T value)
		{
			return (typeof(T).IsInterface || typeof(T) == typeof(object))
			       && value != null && IsStruct(value);
		}


		#region Nil

		/// <summary>
		/// Whether the value of <paramref name="value"/> is <c>default</c> or <c>null</c> bytes,
		/// or <paramref name="value"/> is <c>null</c>
		///
		/// <remarks>"Nil" is <c>null</c> or <c>default</c>.</remarks>
		/// </summary>
		public static bool IsNilFast<T>([CanBeNull, UsedImplicitly] T value)
		{
			// Fastest method for calculating whether a value is nil.
			IL.Emit.Ldarg(nameof(value));
			IL.Emit.Ldnull();
			IL.Emit.Ceq();
			IL.Emit.Ret();
			return IL.Return<bool>();
		}

		internal static bool IsNullOrDefault<T>([CanBeNull] T value)
		{
			return EqualityComparer<T>.Default.Equals(value, default);
		}

		#endregion

		#region Reference

		public static bool IsReferenceOrContainsReferences<T>()
		{
			return IsReferenceOrContainsReferences(typeof(T));
		}

		public static bool IsReferenceOrContainsReferences(Type type)
		{
			// https://github.com/dotnet/coreclr/blob/master/src/vm/jitinterface.cpp#L7507
			var mt = type.GetMethodTable();

			return !type.IsValueType || mt.Reference.ContainsPointers;
		}

		#endregion

		#region Struct

		internal static bool IsStruct<T>() => IsStruct(typeof(T));

		internal static bool IsStruct<T>(T value) => IsStruct(value.GetType());

		internal static bool IsStruct(Type value) => value.IsValueType;

		#endregion

		#region Pointer

		internal static bool IsPointer<T>()
		{
			return IsPointer(typeof(T));
		}

		internal static bool IsPointer<T>(T value)
		{
			return !IsNilFast(value) && IsPointer(value.GetType());
		}

		internal static bool IsPointer(Type type)
		{
			if (type.IsPointer || type == typeof(IntPtr)) {
				return true;
			}

			if (type.IsConstructedGenericType) {
				var genDef = type.GetGenericTypeDefinition();
				return genDef == typeof(Pointer<>) || genDef == typeof(FastPointer<>);
			}

			return false;
		}

		#endregion

		#region String

		internal static bool IsString<T>() => typeof(T) == typeof(string);

		internal static bool IsString<T>(T value) => value is string;

		#endregion

		#region Array

		internal static bool IsArray<T>(T value) => value is Array;

		internal static bool IsArray<T>() => typeof(T).IsArray || typeof(T) == typeof(Array);

		#endregion

		#region Array or String

		internal static bool IsArrayOrString<T>() => IsArray<T>() || IsString<T>();

		internal static bool IsArrayOrString<T>(T value) => IsArray(value) || IsString(value);

		#endregion

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
			return IsArrayOrString<T>() || typeof(T).GetMethodTable().Reference.IsBlittable;
		}
		
		#endregion

		public static void Compile(Type t, string n) => Compile(t.GetAnyMethod(n));


		public static void Compile(MethodInfo methodInfo)
		{
			RuntimeHelpers.PrepareMethod(methodInfo.MethodHandle);
		}

		/// <summary>
		///     Reads a reference type's <see cref="ObjHeader" />
		/// </summary>
		/// <returns>A pointer to the reference type's header</returns>
		internal static Pointer<ObjHeader> ReadObjHeader<T>(T value) where T : class
		{
			return Unsafe.AddressOfHeap(value, OffsetOptions.HEADER).Cast<ObjHeader>();
		}


		


		internal static TypeHandle GetHandle<T>(T value) where T : class
		{
			return Unsafe.AddressOfHeap(value).ReadAny<TypeHandle>();
		}

		#region Offset

		/// <summary>
		///     Returns the field offset of the specified field, by name.
		/// </summary>
		/// <remarks>
		///     Returned from <see cref="FieldDesc.Offset" />
		/// </remarks>
		/// <param name="fieldName">Name of the field</param>
		/// <typeparam name="TType">Enclosing type</typeparam>
		/// <returns>Field offset</returns>
		public static int OffsetOf<TType>(string fieldName)
		{
			return typeof(TType).GetFieldDesc(fieldName).Reference.Offset;
		}

		public static int OffsetOf<T>(ref T value, ref void* field)
		{
			// Faster way of calculating offset
			var ptrValue = Unsafe.AddressOf(ref value).Cast();

			fixed (void** ptrField = &field) {
				return (int) (ptrField - ptrValue);
			}
		}

		public static int OffsetOf<T, TField>(ref T value, ref TField field)
		{
			// Faster way of calculating offset
			var ptrValue = Unsafe.AddressOf(ref value).Cast();
			var ptrField = Unsafe.AddressOf(ref field).Cast();

			return (int) (ptrField - ptrValue);
		}

		#endregion

		#region PTR_HOST_MEMBER_TADDR

		/// <summary>
		/// Retrieves the target address of a host instance pointer and
		/// offsets it by the given member's offset within the type.
		/// </summary>
		/// <param name="value"><c>this</c> pointer</param>
		/// <param name="fieldName">Field name</param>
		/// <param name="fieldValue">Value of the field</param>
		/// <typeparam name="T">Type</typeparam>
		internal static Pointer<byte> PTR_HOST_MEMBER_TADDR<T>(ref T value, string fieldName, Pointer<byte> fieldValue)
			where T : struct
		{
			// PTR_HOST_MEMBER_TADDR(type, host, memb)

			// note: this could be done with just "value" and "fieldName" but it causes significant overhead

			return PTR_HOST_MEMBER_TADDR(ref value, OffsetOf<T>(fieldName), fieldValue);
		}

		internal static Pointer<byte> PTR_HOST_MEMBER_TADDR<T>(ref T value, ref void* field) where T : struct
		{
			var ofs       = OffsetOf(ref value, ref field);
			var fieldLong = (long) field;
			return Unsafe.AddressOf(ref value).Add(ofs).Add(fieldLong).Cast();
		}

		internal static Pointer<byte> PTR_HOST_MEMBER_TADDR<T, TField>(ref T value, ref TField field) where T : struct
		{
			var ofs       = OffsetOf(ref value, ref field);
			var fieldLong = CSUnsafe.As<TField, long>(ref field);
			return Unsafe.AddressOf(ref value).Add(ofs).Add(fieldLong).Cast();
		}

		internal static Pointer<byte> PTR_HOST_MEMBER_TADDR<T>(ref T value, long ofs, Pointer<byte> fieldValue)
			where T : struct
		{
			return Unsafe.AddressOf(ref value).Add((long) fieldValue).Add(ofs).Cast();
		}

		#endregion

		/// <summary>
		///     Runs a constructor whose parameters match <paramref name="args" />
		/// </summary>
		/// <param name="value">Instance</param>
		/// <param name="args">Constructor arguments</param>
		/// <returns>
		///     <c>true</c> if a matching constructor was found and executed;
		///     <c>false</c> if a constructor couldn't be found
		/// </returns>
		public static bool RunConstructor<T>(T value, params object[] args)
		{
			ConstructorInfo[] ctors    = value.GetType().GetConstructors();
			Type[]            argTypes = args.Select(x => x.GetType()).ToArray();

			foreach (var ctor in ctors) {
				ParameterInfo[] paramz = ctor.GetParameters();

				if (paramz.Length == args.Length) {
					if (paramz.Select(x => x.ParameterType).SequenceEqual(argTypes)) {
						ctor.Invoke(value, args);
						return true;
					}
				}
			}

			return false;
		}

		#region HeapObjects

		internal static ArrayObject** GetArrayObject<T>(ref T t) where T : class
		{
			Conditions.Require(IsArray(t));

			return (ArrayObject**) Unsafe.AddressOf(ref t);
		}

		internal static StringObject** GetStringObject(ref string s)
		{
			return (StringObject**) Unsafe.AddressOf(ref s);
		}

		internal static HeapObject** GetHeapObject<T>(ref T t) where T : class
		{
			return (HeapObject**) Unsafe.AddressOf(ref t);
		}

		#endregion

		#region MethodTable

		internal static TypeHandle ReadTypeHandle<T>(T value)
		{
			if (IsStruct<T>(value))
				return value.GetType().GetTypeHandle();


			Unsafe.TryGetAddressOfHeap(value, out Pointer<byte> ptr);
			Conditions.Ensure(!ptr.IsNull);
			var mt = *(TypeHandle*) ptr;

			return mt;
		}

		internal static TypeHandle GetTypeHandle(this Type value)
		{
			var typeHandle = value.TypeHandle.Value;
			return *(TypeHandle*) &typeHandle;
		}

		/// <summary>
		///     <para>Manually reads a CLR <see cref="MethodTable" /> (TypeHandle).</para>
		///     <para>
		///         If the type is a value type, the <see cref="MethodTable" /> will be returned from
		///         <see cref="RuntimeTypeHandle.Value" />
		///     </para>
		/// </summary>
		/// <returns>A pointer to type <typeparamref name="T" />'s <see cref="MethodTable" /></returns>
		internal static Pointer<MethodTable> ReadMethodTable<T>(T value)
		{
			// Value types do not have a MethodTable ptr, but they do have a TypeHandle.
			if (IsStruct(value))
				return value.GetType().GetMethodTable();

			Unsafe.TryGetAddressOfHeap(value, out Pointer<byte> ptr);
			Conditions.Ensure(!ptr.IsNull);
			MethodTable* mt = *(MethodTable**) ptr;

			return mt;
		}


		[Obsolete]
		private static Pointer<MethodTable> ReadGetMethodTable(this Type t)
		{
			var typeHandle = t.TypeHandle.Value;

			// TypeHandle::GetMethodTable also returns the correct MethodTable,
			// but we don't have its TypeHandle here

			// Special case:
			// If an object is an array, its actual MethodTable* is stored at the address pointed to by its
			// given MethodTable* returned from TypeHandle.Value (which is invalid),
			// offset by ARRAY_MT_PTR_OFFSET bytes.

			// See ARRAY_MT_PTR_OFFSET documentation

			// Example:
			// 00 00 00 00 00 00 18 91 C6 83 F9 7F
			//				     ^

			// I don't know why this is, but whatever

			return t.IsArray ? Mem.ReadPointer<MethodTable>(typeHandle, Offsets.ARRAY_MT_PTR_OFFSET) : typeHandle;
		}

		/// <summary>
		///     Returns a pointer to a type's TypeHandle as a <see cref="MethodTable" />
		/// </summary>
		/// <param name="t">Type to return the corresponding <see cref="MethodTable" /> for.</param>
		/// <returns>A <see cref="Pointer{T}" /> to type <paramref name="t" />'s <see cref="MethodTable" /></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Pointer<MethodTable> GetMethodTable(this Type t)
		{
			var typeHandle      = t.TypeHandle.Value;
			var typeHandleValue = *(TypeHandle*) &typeHandle;
			return typeHandleValue.MethodTable;
		}

		// ReSharper disable once InconsistentNaming
		internal static Pointer<EEClass> GetEEClass(this Type t)
		{
			return t.GetMethodTable().Reference.EEClass;
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
			int len = mt.Reference.FieldDescListLength;

			if (len == 0) {
				return null;
			}

			var lpFd = new Pointer<FieldDesc>[len];

			for (int i = 0; i < len; i++)
				lpFd[i] = &mt.Reference.FieldDescList[i];

			return lpFd;
		}

		internal static Pointer<FieldDesc>[] GetFieldDescs<T>(T value)
		{
			return ReadFieldDescs(ReadMethodTable(value));
		}

		internal static Pointer<FieldDesc>[] GetFieldDescs(this Type value)
		{
//			RazorContract.Requires(!t.IsArray, "Arrays do not have fields");
			// Adds about 1k ns
//			lpFd = lpFd.OrderBy(x => x.ToInt64()).ToArray();
			return ReadFieldDescs(value.GetMethodTable());
		}


		// todo: add support for getting FieldDesc of fixed buffers (like isAutoProperty) - use an enum probably

		internal static Pointer<FieldDesc> GetFieldDesc(this FieldInfo fieldInfo)
		{
			Conditions.NotNull(fieldInfo, nameof(fieldInfo));
			Pointer<FieldDesc> fieldDesc = fieldInfo.FieldHandle.Value;

			if (Mem.Is64Bit) {
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
			Conditions.NotNull(methodInfo, nameof(methodInfo));

			var methodHandle = methodInfo.MethodHandle;
			var md           = (MethodDesc*) methodHandle.Value;

			Conditions.Ensure(md->Info.MetadataToken == methodInfo.MetadataToken);

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
			Conditions.NotNull(methods, nameof(methods));
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