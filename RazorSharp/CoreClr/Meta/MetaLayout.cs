using RazorSharp.CoreClr.Meta.Base;
using RazorSharp.CoreClr.Metadata.Enums;
using RazorSharp.CoreClr.Metadata.ExecutionEngine;
using RazorSharp.Memory.Pointers;

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Meta
{
	/// <summary>
	///     <list type="bullet">
	///         <item><description>CLR structure: <see cref="EEClassLayoutInfo"/></description></item>
	///         <item><description>Reflection structure: N/A</description></item>
	///     </list>
	/// </summary>
	public unsafe class MetaLayout : AnonymousClrStructure<EEClassLayoutInfo>
	{
		#region Constructors

		internal MetaLayout(Pointer<EEClassLayoutInfo> ptr) : base(ptr) { }

		#endregion

		#region Accessors

		public int NativeSize => Value.Reference.NativeSize;

		public int ManagedSize => Value.Reference.ManagedSize;

		public LayoutFlags Flags => Value.Reference.Flags;

		public int PackingSize => Value.Reference.PackingSize;

		public int NumCTMFields => Value.Reference.NumCTMFields;

		public Pointer<byte> FieldMarshalers => Value.Reference.FieldMarshalers;

		#endregion
	}
}