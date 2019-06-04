using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using InlineIL;
using JetBrains.Annotations;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.CoreClr
{
	/// <summary>
	/// Provides auxiliary runtime information
	/// </summary>
	public static class RtInfo
	{
		#region Is

		#region Unmanaged

		/// <summary>
		/// Dummy class for use with <see cref="IsUnmanaged{T}"/> and <see cref="IsUnmanaged"/>
		/// </summary>
		// ReSharper disable once UnusedTypeParameter
		private sealed class U<T> where T : unmanaged { }

		/// <summary>
		/// Determines whether <paramref name="t"/> fits the <c>unmanaged</c> type constraint.
		/// </summary>
		/// <returns><c>true</c> if <paramref name="t"/> fits the unmanaged constraint; <c>false</c> otherwise</returns>
		public static bool IsUnmanaged(Type t)
		{
			try {
				// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
				typeof(U<>).MakeGenericType(t);
				return true;
			}
			catch {
				return false;
			}
		}

		/// <summary>
		/// Determines whether <typeparamref name="T"/> fits the <c>unmanaged</c> type constraint.
		/// </summary>
		/// <returns><c>true</c> if <typeparamref name="T"/> fits the unmanaged constraint; <c>false</c> otherwise</returns>
		public static bool IsUnmanaged<T>() => IsUnmanaged(typeof(T));

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
		///             but <see cref="IsBlittable{T}" /> returns <c>true</c>, as <see cref="GCHandle" /> determines it
		/// blittable.
		///         </para>
		///     </remarks>
		/// </summary>
		public static bool IsBlittable<T>()
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
		public static bool IsNil<T>([CanBeNull, UsedImplicitly] T value)
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
			Pointer<MethodTable> mt = type.GetMethodTable();

			return !type.IsValueType || mt.Reference.ContainsPointers;
		}

		#endregion

		#region Struct

		public static bool IsStruct<T>()        => IsStruct(typeof(T));
		public static bool IsStruct<T>(T value) => IsStruct(value.GetType());
		public static bool IsStruct(Type value) => value.IsValueType;

		#endregion

		#region Pointer

		public static bool IsPointer<T>()
		{
			return IsPointer(typeof(T));
		}

		public static bool IsPointer<T>(T value)
		{
			return !IsNil(value) && IsPointer(value.GetType());
		}

		/// <summary>
		/// Determines whether <paramref name="type"/> is a native pointer, <see cref="IntPtr"/>, <see cref="UIntPtr"/>,
		/// <see cref="Pointer{T}"/>, or <see cref="FastPointer{T}"/>
		/// </summary>
		public static bool IsPointer(Type type)
		{
			if (type.IsPointer || type == typeof(IntPtr) || type == typeof(UIntPtr) || type == typeof(Pointer)) {
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

		public static bool IsString<T>()        => typeof(T) == typeof(string);
		public static bool IsString<T>(T value) => value is string;

		#endregion


		#region Array

		public static bool IsArray<T>(T value) => value is Array;
		public static bool IsArray<T>()        => typeof(T).IsArray || typeof(T) == typeof(Array);

		#endregion

		#region Array or String

		public static bool IsArrayOrString<T>()        => IsArray<T>() || IsString<T>();
		public static bool IsArrayOrString<T>(T value) => IsArray(value) || IsString(value);

		#endregion

		#region Real

		/// <summary>
		/// Determines whether <paramref name="t"/> is a floating-point number.
		/// </summary>
		/// <returns><c>true</c> if <paramref name="t"/> is a floating-point number; <c>false</c> otherwise</returns>
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

		/// <summary>
		/// Determines whether <typeparamref name="T"/> is a floating-point number.
		/// </summary>
		/// <returns><c>true</c> if <typeparamref name="T"/> is a floating-point number; <c>false</c> otherwise</returns>
		public static bool IsReal<T>()
		{
			return IsReal(typeof(T));
		}

		#endregion

		#region Integer

		/// <summary>
		/// Determines whether <paramref name="t"/> is an integer
		/// </summary>
		/// <returns><c>true</c> if <paramref name="t"/> is an integer; <c>false</c> otherwise</returns>
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

		/// <summary>
		/// Determines whether <typeparamref name="T"/> is an integer
		/// </summary>
		/// <returns><c>true</c> if <typeparamref name="T"/> is an integer; <c>false</c> otherwise</returns>
		public static bool IsInteger<T>()
		{
			return IsInteger(typeof(T));
		}

		#endregion

		#region Numeric

		/// <summary>
		/// Determines whether <paramref name="t"/> is an integer or a floating-point number.
		/// </summary>
		/// <returns><c>true</c> if <paramref name="t"/> is an integer or a floating-point number; <c>false</c> otherwise</returns>
		public static bool IsNumeric(Type t)
		{
			return IsReal(t) || IsInteger(t);
		}

		/// <summary>
		/// Determines whether <typeparamref name="T"/> is an integer or a floating-point number.
		/// </summary>
		/// <returns><c>true</c> if <typeparamref name="T"/> is an integer or a floating-point number; <c>false</c> otherwise</returns>
		public static bool IsNumeric<T>()
		{
			return IsNumeric(typeof(T));
		}

		#endregion

		#endregion
	}
}