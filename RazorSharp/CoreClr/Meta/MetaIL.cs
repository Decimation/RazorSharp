#region

#region

using RazorSharp.Analysis;
using RazorSharp.CoreClr.Meta.Interfaces;
using SimpleSharp;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.Enums;
using RazorSharp.CoreClr.Structures.ILMethods;
using RazorSharp.Memory.Pointers;

#endregion

// ReSharper disable InconsistentNaming

#endregion

namespace RazorSharp.CoreClr.Meta
{
	/// <summary>
	///     Exposes metadata from:
	///     <list type="bullet">
	///         <item>
	///             <description>
	///                 <see cref="FatILMethod" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="TinyILMethod" />
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <see cref="Structures.ILMethods.ILMethod" />
	///             </description>
	///         </item>
	///     </list>
	/// </summary>
	public class MetaIL : IToken
	{
		internal MetaIL(Pointer<ILMethod> value)
		{
			ILMethod = value;
		}

		private Pointer<ILMethod> ILMethod { get; }

		/// <summary>
		/// Points to <see cref="ILMethod"/>
		/// </summary>
		public Pointer<byte> Value => ILMethod.Cast();

		/// <summary>
		///     Whether this type is <see cref="TinyILMethod" />
		/// </summary>
		public bool IsTiny => ILMethod.Reference.IsTiny;

		/// <summary>
		///     Whether this type is <see cref="FatILMethod" />
		/// </summary>
		public bool IsFat => ILMethod.Reference.IsFat;

		/// <summary>
		///     Points to the JIT IL code
		/// </summary>
		public Pointer<byte> Code => ILMethod.Reference.Code;

		/// <summary>
		///     Length/size of the IL code (<see cref="Code" />)
		/// </summary>
		public int CodeSize => ILMethod.Reference.CodeSize;

		/// <summary>
		///     <remarks>
		///         Equals <see cref="System.Reflection.MethodBody.MaxStackSize" />
		///     </remarks>
		/// </summary>
		public int MaxStack => ILMethod.Reference.MaxStack;

		/// <summary>
		///     <remarks>
		///         Equals <see cref="System.Reflection.MethodBody.LocalSignatureMetadataToken" />
		///     </remarks>
		/// </summary>
		public int Token => ILMethod.Reference.LocalVarSigTok;

		/// <summary>
		///     <remarks>
		///         <see cref="IsFat" /> must be <c>true</c>
		///     </remarks>
		/// </summary>
		public CorILMethodFlags Flags => ILMethod.Reference.Flags;

		public void WriteIL(byte[] opCodes)
		{
			ILMethod.Reference.WriteIL(opCodes);
		}

		/// <summary>
		///     <remarks>
		///         Equals <see cref="System.Reflection.MethodBody.GetILAsByteArray()" />
		///     </remarks>
		/// </summary>
		/// <returns></returns>
		public byte[] GetILAsByteArray()
		{
			return ILMethod.Reference.GetILAsByteArray();
		}

		public Instruction[] Instructions => InspectIL.GetInstructions(GetILAsByteArray());
		
		public ConsoleTable ToTable()
		{
			return ILMethod.Reference.ToTable();
		}

		public override string ToString()
		{
			return ToTable().ToString();
		}
	}
}