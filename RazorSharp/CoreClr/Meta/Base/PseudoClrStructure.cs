using System.Reflection;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.CoreClr.Meta.Base
{
	/// <summary>
	/// Describes a <see cref="ClrStructure{TClr}"/> that doesn't have an accompanying <see cref="Info"/>
	/// or <see cref="Token"/>
	/// </summary>
	/// <typeparam name="TClr">CLR structure type</typeparam>
	public abstract class PseudoClrStructure<TClr> : ClrStructure<TClr> where TClr : unmanaged
	{
		#region Constructors

		internal PseudoClrStructure(Pointer<TClr> ptr) : base(ptr) { }

		#endregion

		/// <summary>
		/// Returns <c>null</c>
		/// </summary>
		public override MemberInfo Info => null;
		
		/// <summary>
		/// Returns <see cref="Constants.INVALID_VALUE"/>
		/// </summary>
		public override int Token => Constants.INVALID_VALUE;
	}
}