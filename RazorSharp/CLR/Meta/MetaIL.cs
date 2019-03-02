// ReSharper disable InconsistentNaming

#region

using RazorSharp.CLR.Structures.ILMethods;
using RazorCommon;
using RazorCommon.Utilities;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.CLR.Meta
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
		private readonly Pointer<ILMethod> m_value;

		internal MetaIL(Pointer<ILMethod> value)
		{
			m_value = value;
		}

		/// <summary>
		///     Whether this type is <see cref="TinyILMethod" />
		/// </summary>
		public bool IsTiny => m_value.Reference.IsTiny;

		/// <summary>
		///     Whether this type is <see cref="FatILMethod" />
		/// </summary>
		public bool IsFat => m_value.Reference.IsFat;

		/// <summary>
		///     Points to the JIT IL code
		/// </summary>
		public Pointer<byte> Code => m_value.Reference.Code;

		/// <summary>
		///     Length/size of the IL code (<see cref="Code" />)
		/// </summary>
		public int CodeSize => m_value.Reference.CodeSize;

		/// <summary>
		///     <remarks>
		///         Equals <see cref="System.Reflection.MethodBody.MaxStackSize" />
		///     </remarks>
		/// </summary>
		public int MaxStack => m_value.Reference.MaxStack;

		/// <summary>
		///     <remarks>
		///         Equals <see cref="System.Reflection.MethodBody.LocalSignatureMetadataToken" />
		///     </remarks>
		/// </summary>
		public int LocalVarSigTok => m_value.Reference.LocalVarSigTok;

		/// <summary>
		///     <remarks>
		///         <see cref="IsFat" /> must be <c>true</c>
		///     </remarks>
		/// </summary>
		public CorILMethodFlags Flags => m_value.Reference.Flags;

		public void WriteIL(byte[] opCodes)
		{
			m_value.Reference.WriteIL(opCodes);
		}

		/// <summary>
		///     <remarks>
		///         Equals <see cref="System.Reflection.MethodBody.GetILAsByteArray()" />
		///     </remarks>
		/// </summary>
		/// <returns></returns>
		public byte[] GetILAsByteArray()
		{
			return m_value.Reference.GetILAsByteArray();
		}

		public ConsoleTable ToTable()
		{
			var table = new ConsoleTable("Info", "Value");

			table.AddRow("Type", IsTiny ? "Tiny" : "Fat");
			table.AddRow("Code", Code.ToString("P"));
			table.AddRow("Code size", CodeSize);
			table.AddRow("Max stack", MaxStack);
			table.AddRow("Local sig token", LocalVarSigTok);
			table.AddRow("Flags", IsFat ? EnumUtil.CreateString(Flags) : "-");

			return table;
		}

		public override string ToString()
		{
			return ToTable().ToMarkDownString();
		}
	}
}