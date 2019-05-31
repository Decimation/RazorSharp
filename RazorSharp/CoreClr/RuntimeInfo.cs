using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using InlineIL;
using JetBrains.Annotations;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.CoreClr
{
	public static class RuntimeInfo
	{
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

		#region Other

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

		public static bool IsBoxed<T>(in T value)
		{
			return (typeof(T).IsInterface || typeof(T) == typeof(object))
			       && value != null && IsStruct(value);
		}

		#endregion

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

		public static bool IsNullOrDefault<T>([CanBeNull] T value)
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

		internal static bool IsStruct<T>()        => IsStruct(typeof(T));
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
			return !RuntimeInfo.IsNilFast(value) && IsPointer(value.GetType());
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

		internal static bool IsString<T>()        => typeof(T) == typeof(string);
		internal static bool IsString<T>(T value) => value is string;

		#endregion

		#region Array

		internal static bool IsArray<T>(T value) => value is Array;
		internal static bool IsArray<T>()        => typeof(T).IsArray || typeof(T) == typeof(Array);

		#endregion

		#region Array or String

		internal static bool IsArrayOrString<T>()        => RuntimeInfo.IsArray<T>() || RuntimeInfo.IsString<T>();
		internal static bool IsArrayOrString<T>(T value) => RuntimeInfo.IsArray(value) || RuntimeInfo.IsString(value);

		#endregion

		#region Real

		public static bool IsReal(Type t)
		{
			switch (Type.GetTypeCode(t)) {
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Single:
					return true;
				default:
					return false;
			}
		}

		public static bool IsReal<T>()
		{
			return IsReal(typeof(T));
		}
		
		#endregion

		#region Integer

		public static bool IsInteger(Type t)
		{
			switch (Type.GetTypeCode(t)) {
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.Int16:
				case TypeCode.UInt32:
				case TypeCode.Int32:
				case TypeCode.UInt64:
				case TypeCode.Int64:
					return true;
				default:
					return false;
			}
		}

		public static bool IsInteger<T>()
		{
			return IsInteger(typeof(T));
		}

		#endregion

		#region Numeric

		public static bool IsNumeric(Type t)
		{
			return IsReal(t) || IsInteger(t);
		}

		public static bool IsNumeric<T>()
		{
			return IsNumeric(typeof(T));
		}

		#endregion

		#endregion
	}
}