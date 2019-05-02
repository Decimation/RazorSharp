#region

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Diagnostics;
using RazorCommon.Strings;
using Unsafe = RazorSharp.Memory.Unsafe;

// ReSharper disable FieldCanBeMadeReadOnly.Local

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoProperty

#endregion

namespace RazorSharp.CoreClr.Structures.HeapObjects
{
	#region

	#endregion


	/// <summary>
	///     <para>Represents the layout of <see cref="string" /> in heap memory.</para>
	///     <para>Corresponding files:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/object.h</description>
	///         </item>
	///         <item>
	///             <description>/src/vm/object.cpp</description>
	///         </item>
	///         <item>
	///             <description>/src/vm/object.inl</description>
	///         </item>
	///     </list>
	///     <para>Lines of interest:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/object.h: 1082</description>
	///         </item>
	///     </list>
	///     <remarks>
	///         Should be used with <see cref="Runtime.GetStringObject(ref string)" /> and double indirection.
	///     </remarks>
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct StringObject : IHeapObject
	{
		#region Fields

		#endregion

		public uint Length { get; }

		public char FirstChar { get; }

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		public ObjHeader* Header => (ObjHeader*) (Unsafe.AddressOf(ref this) - IntPtr.Size);

		public MethodTable* MethodTable { get; }

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		public char this[int index] {
			get {
				char* __this = GetPointer();
				return __this[GetIndex(index)];
			}
			set {
				char* __this = GetPointer();
				__this[GetIndex(index)] = value;
			}
		}

		private char* GetPointer()
		{
			var __this = (char*) Unsafe.AddressOf(ref this);
			Conditions.NotNull(__this, nameof(__this));
			return __this;
		}

		private static int GetIndex(int index)
		{
			return index + RuntimeHelpers.OffsetToStringData / sizeof(char);
		}

		public string StringValue {
			get {
				var __this = (char*) Unsafe.AddressOf(ref this);
				__this += RuntimeHelpers.OffsetToStringData / sizeof(char);
				return new string(__this);

				// get { return ref System.Runtime.CompilerServices.Unsafe.AsRef<string>(Unsafe.AddressOf(ref this).ToPointer()); }
			}
		}


		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Header*", Hex.ToHex(Header));
			table.AddRow("MethodTable*", Hex.ToHex(MethodTable));
			table.AddRow("Length", Length);
			table.AddRow("First char", FirstChar);

			return table.ToString();
		}
	}
}