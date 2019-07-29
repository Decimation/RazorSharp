using RazorSharp.CoreClr.Metadata;
using RazorSharp.Memory.Pointers;

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Meta.Base
{
	/// <summary>
	/// Describes a <see cref="ClrStructure{TClr}"/> that is enclosed by an accompanying <see cref="MethodTable"/>
	/// </summary>
	/// <typeparam name="TClr">CLR structure type</typeparam>
	public abstract unsafe class EmbeddedClrStructure<TClr> : ClrStructure<TClr> where TClr : unmanaged
	{
		#region Constructors

		protected EmbeddedClrStructure(Pointer<TClr> ptr) : base(ptr) { }

		#endregion
		
		#region MethodTable

		public abstract MetaType EnclosingType { get; }

		public MetaType EnclosingRuntimeType => EnclosingType.RuntimeType;

		#endregion
	}
}