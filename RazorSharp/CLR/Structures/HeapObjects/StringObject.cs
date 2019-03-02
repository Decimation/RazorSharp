#region

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorSharp.Utilities;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoProperty

#endregion

namespace RazorSharp.CLR.Structures.HeapObjects
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
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct StringObject : IHeapObject
	{
// 		[FieldOffset(-8) public ObjHeader _header
		[FieldOffset(0)]  private readonly MethodTable* m_methodTablePtr;
		[FieldOffset(8)]  private readonly uint         m_stringLength;
		[FieldOffset(12)] private readonly char         m_firstChar;

		public uint Length    => m_stringLength;
		public char FirstChar => m_firstChar;

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		public ObjHeader* Header => (ObjHeader*) (Unsafe.AddressOf(ref this) - IntPtr.Size);

		public MethodTable* MethodTable => m_methodTablePtr;

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		public char this[int index] {
			get {
				var __this = (char*) Unsafe.AddressOf(ref this);
				Conditions.RequiresNotNull(__this);

				return __this[index + RuntimeHelpers.OffsetToStringData / sizeof(char)];
			}
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
			table.AddRow("MethodTable*", Hex.ToHex(m_methodTablePtr));
			table.AddRow("Length", m_stringLength);
			table.AddRow("First char", m_firstChar);

			return table.ToMarkDownString();
		}
	}
}