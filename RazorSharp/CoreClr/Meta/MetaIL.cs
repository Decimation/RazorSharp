#region

#region

using RazorSharp.Analysis;
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
	///                 <see cref="ILMethod" />
	///             </description>
	///         </item>
	///     </list>
	/// </summary>
	public class MetaIL
	{
		internal MetaIL(Pointer<ILMethod> value)
		{
			Value = value;
		}

		private Pointer<ILMethod> Value { get; }

		/// <summary>
		///     Whether this type is <see cref="TinyILMethod" />
		/// </summary>
		public bool IsTiny => Value.Reference.IsTiny;

		/// <summary>
		///     Whether this type is <see cref="FatILMethod" />
		/// </summary>
		public bool IsFat => Value.Reference.IsFat;

		/// <summary>
		///     Points to the JIT IL code
		/// </summary>
		public Pointer<byte> Code => Value.Reference.Code;

		/// <summary>
		///     Length/size of the IL code (<see cref="Code" />)
		/// </summary>
		public int CodeSize => Value.Reference.CodeSize;

		/// <summary>
		///     <remarks>
		///         Equals <see cref="System.Reflection.MethodBody.MaxStackSize" />
		///     </remarks>
		/// </summary>
		public int MaxStack => Value.Reference.MaxStack;

		/// <summary>
		///     <remarks>
		///         Equals <see cref="System.Reflection.MethodBody.LocalSignatureMetadataToken" />
		///     </remarks>
		/// </summary>
		public int LocalVarSigTok => Value.Reference.LocalVarSigTok;

		/// <summary>
		///     <remarks>
		///         <see cref="IsFat" /> must be <c>true</c>
		///     </remarks>
		/// </summary>
		public CorILMethodFlags Flags => Value.Reference.Flags;

		public void WriteIL(byte[] opCodes)
		{
			Value.Reference.WriteIL(opCodes);
		}

		/// <summary>
		///     <remarks>
		///         Equals <see cref="System.Reflection.MethodBody.GetILAsByteArray()" />
		///     </remarks>
		/// </summary>
		/// <returns></returns>
		public byte[] GetILAsByteArray()
		{
			return Value.Reference.GetILAsByteArray();
		}

		public Instruction[] Instructions => InspectIL.GetInstructions(GetILAsByteArray());
		
		public ConsoleTable ToTable()
		{
			return Value.Reference.ToTable();
		}

		public override string ToString()
		{
			return ToTable().ToString();
		}
	}
}