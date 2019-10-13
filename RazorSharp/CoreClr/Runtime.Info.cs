using System;
using InlineIL;
using JetBrains.Annotations;
using RazorSharp.CoreClr.Meta;
using RazorSharp.Interop.Utilities;
using RazorSharp.Memory;
using RazorSharp.Utilities.Security;

namespace RazorSharp.CoreClr
{
	
	/// <summary>
	/// Provides auxiliary runtime information and convenience functions
	/// </summary>
	public static unsafe partial class Runtime
	{
		public static class Info
		{
			#region Is

			#region Other

			public static bool IsBoxed<T>(T value)
			{
				return (typeof(T).IsInterface || typeof(T) == typeof(object)) && value != null && IsStruct(value);
			}

			public static bool IsPinnable<T>(T value) where T : class
			{
				// https://github.com/dotnet/coreclr/blob/adecd858f558489d8f52c9187fca395ec669a715/src/vm/marshalnative.cpp#L257

				if (value == null) {
					return true;
				}

				var mt = Runtime.ReadMetaType(value);

				if (mt.IsString) {
					return true;
				}

				if (mt.IsArray) {
					// todo
					throw Guard.NotImplementedFail(nameof(mt.IsArray));
				}


				return mt.IsBlittable;
			}

			/// <summary>
			/// Determines whether <typeparamref name="TPointer"/> is the same size as <see cref="IntPtr.Size"/>.
			/// If they are equal, <typeparamref name="TPointer"/> can act as a surrogate pointer. This means
			/// it can be substituted for a pointer.
			/// </summary>
			/// <typeparam name="TPointer">Type to test</typeparam>
			/// <returns><c>true</c> if <typeparamref name="TPointer"/> equals <see cref="IntPtr.Size"/>;
			/// <c>false</c> otherwise.</returns>
			public static bool IsPointerSubstitute<TPointer>() where TPointer : struct
			{
				return IntPtr.Size == Unsafe.SizeOf<TPointer>();
			}

			#endregion

			#region Nil

			/// <summary>
			/// Whether the value of <paramref name="value"/> is <c>default</c> or <c>null</c> bytes,
			/// or <paramref name="value"/> is <c>null</c>
			///
			/// <remarks>"Nil" is <c>null</c> or <c>default</c>.</remarks>
			/// </summary>
			public static bool IsNil<T>([CanBeNull, Native] T value)
			{
				// Fastest method for calculating whether a value is nil.
				IL.Emit.Ldarg(nameof(value));
				IL.Emit.Ldnull();
				IL.Emit.Ceq();
				IL.Emit.Ret();
				return IL.Return<bool>();
			}

			/*public static bool IsNullOrDefault<T>([CanBeNull] T value)
			{
				return EqualityComparer<T>.Default.Equals(value, default);
			}*/

			#endregion

			#region String

			internal static bool IsString<T>() => typeof(T) == typeof(string);

			internal static bool IsString<T>(T value) => value is string;

			#endregion

			#region Array

			internal static bool IsArray<T>() => typeof(T).IsArray || typeof(T) == typeof(Array);

			internal static bool IsArray<T>(T value) => value is Array;

			#endregion

			#region Struct

			internal static bool IsStruct<T>() => ((MetaType) typeof(T)).IsStruct;

			internal static bool IsStruct<T>(T value) => ((MetaType) value.GetType()).IsStruct;

			#endregion

			#endregion
		}
	}
}