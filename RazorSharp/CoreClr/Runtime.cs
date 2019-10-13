using System;
using System.Reflection;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Metadata;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Interop;
using RazorSharp.Memory;
using RazorSharp.Memory.Enums;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Security;
using SimpleSharp.Diagnostics;

namespace RazorSharp.CoreClr
{
	public static unsafe partial class Runtime
	{
		static Runtime()
		{
			const string FN_NAME = "GetTypeFromHandleUnsafe";
			var          method  = typeof(Type).GetAnyMethod(FN_NAME);

			GetTypeFromHandle = (GetTypeFromHandleFunction) method.CreateDelegate(typeof(GetTypeFromHandleFunction));
		}

		public static MetaType GetMetaType(this Type t) => t;

		/// <summary>
		/// Allocates an object of type <typeparamref name="T"/> in the GC heap.
		/// </summary>
		/// <param name="args">Constructor arguments</param>
		/// <typeparam name="T">Type to allocate</typeparam>
		/// <returns>An initialized object</returns>
		public static T AllocObject<T>(params object[] args)
		{
			var value = GlobalHeap.AllocateObject<T>();
			Functions.Reflection.CallConstructor(value, args);
			return value;
		}

		/// <summary>
		/// <c>PTR_HOST_MEMBER_TADDR</c>
		/// </summary>
		internal static Pointer<byte> HostMemberOffset<T>(ref T value, long ofs, Pointer<byte> fieldValue)
			where T : struct
		{
			return Unsafe.AddressOf(ref value).Add((long) fieldValue).Add(ofs).Cast();
		}
		
		internal static TypeHandle ReadTypeHandle<T>(T value)
		{
			// Value types do not have a MethodTable ptr, but they do have a TypeHandle.
			if (Runtime.Info.IsStruct(value))
				return ReadTypeHandle(value.GetType());

			Unsafe.TryGetAddressOfHeap(value, out Pointer<byte> ptr);
			Conditions.Ensure(!ptr.IsNull);
			return *(TypeHandle*) ptr;
		}

		internal static TypeHandle ReadTypeHandle(Type t)
		{
			var handle          = t.TypeHandle.Value;
			var typeHandleValue = *(TypeHandle*) &handle;
			return typeHandleValue;
		}

		internal static MetaType ReadMetaType<T>(T value)
		{
			return new MetaType(ReadTypeHandle(value).MethodTable);
		}


		/// <summary>
		/// Returns a pointer to the internal CLR metadata structure of <paramref name="member"/>
		/// </summary>
		/// <param name="member">Reflection type</param>
		/// <returns>A pointer to the corresponding structure</returns>
		/// <exception cref="InvalidOperationException">The type of <see cref="MemberInfo"/> doesn't have a handle</exception>
		internal static Pointer<byte> ResolveHandle(MemberInfo member)
		{
			if (member == null) {
				throw new ArgumentNullException(nameof(member));
			}

			return member switch
			{
				Type t => ReadTypeHandle(t).MethodTable.Cast(),
				FieldInfo field => field.FieldHandle.Value,
				MethodInfo method => method.MethodHandle.Value,
				_ => throw Guard.NotSupportedMemberFail(member)
			};
		}


		#region Type from handle

		/// <summary>
		/// Returns the corresponding Reflection <see cref="Type"/> of the handle specified by <paramref name="handle"/>
		/// </summary>
		/// <param name="handle"></param>
		/// <returns></returns>
		public static Type ResolveType(Pointer<byte> handle)
		{
			return GetTypeFromHandle(handle.Address);
		}

		private delegate Type GetTypeFromHandleFunction(IntPtr handle);

		private static readonly GetTypeFromHandleFunction GetTypeFromHandle;

		#endregion


		internal static ObjHeader ReadObjHeader<T>(T value) where T : class
		{
			Pointer<ObjHeader> ptr = Unsafe.AddressOfHeap(value, OffsetOptions.Header).Cast<ObjHeader>();
			return ptr.Value;
		}
	}
}