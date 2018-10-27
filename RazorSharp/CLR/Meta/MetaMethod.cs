#region

using System;
using System.Reflection;
using RazorSharp.CLR.Structures;
using RazorSharp.CLR.Structures.ILMethods;
using RazorSharp.Common;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.CLR.Meta
{

	/// <summary>
	///     Exposes metadata from:
	///     <list type="bullet">
	///         <item>
	///             <description>
	///                 <see cref="MethodDesc" />
	///             </description>
	///         </item>
	///     </list>
	/// </summary>
	public class MetaMethod : IMetaMember
	{
		private readonly Pointer<MethodDesc> m_value;

		internal MetaMethod(Pointer<MethodDesc> methodDesc)
		{
			m_value = methodDesc;
		}

		#region Accessors

		/// <summary>
		///     <para>Metadata token of this method</para>
		///     <remarks>
		///         <para>Equal to <see cref="System.Reflection.MethodInfo.MetadataToken" /></para>
		///         <para>Address-sensitive</para>
		///     </remarks>
		/// </summary>
		public int Token => m_value.Reference.Token;

		public string Name => m_value.Reference.Name;

		public Type EnclosingType => m_value.Reference.EnclosingType;

		/// <summary>
		///     The corresponding <see cref="MethodInfo" /> of this <see cref="MethodDesc" />
		/// </summary>
		public MethodInfo MethodInfo => m_value.Reference.Info;

		public MemberInfo Info => MethodInfo;

		/// <summary>
		///     Function pointer (entry point) of this method.
		///     <para>
		///         <see cref="get_Function" /> returns the entry point
		///         (<see cref="RuntimeMethodHandle.GetFunctionPointer()" />) of this method.
		///     </para>
		///     <para>
		///         <see cref="set_Function" /> sets the method entry point (<see cref="SetStableEntryPoint" />).
		///     </para>
		/// </summary>
		public IntPtr Function {
			get => m_value.Reference.Function;
			set => m_value.Reference.Function = value;
		}

		/// <summary>
		///     Returns the address of the native code. The native code can be one of jitted code if
		///     <see cref="IsPreImplemented" /> is <c>false</c> or
		///     ngened code if <see cref="IsPreImplemented" /> is <c>true</c>.
		///     <returns><see cref="IntPtr.Zero" /> if the method has no native code.</returns>
		/// </summary>
		public IntPtr NativeCode => m_value.Reference.NativeCode;

		public IntPtr PreImplementedCode => m_value.Reference.PreImplementedCode;

		// ChunkIndex
		// MethodDescChunk
		// SizeOf

		// EnclosingMethodTable
		public MetaType EnclosingMetaType => new MetaType(m_value.Reference.EnclosingMethodTable);

		// RVA

		#region bool

		public bool IsConstructor    => m_value.Reference.IsConstructor;
		public bool IsPreImplemented => m_value.Reference.IsPreImplemented;
		public bool HasThis          => m_value.Reference.HasThis;
		public bool HasILHeader      => m_value.Reference.HasILHeader;
		public bool IsUnboxingStub   => m_value.Reference.IsUnboxingStub;
		public bool IsIL             => m_value.Reference.IsIL;
		public bool IsStatic         => m_value.Reference.IsStatic;

		#endregion

		#region Flags

		public MethodClassification     Classification => m_value.Reference.Classification;
		public MethodAttributes         Attributes     => m_value.Reference.Attributes;
		public MethodDescClassification Flags          => m_value.Reference.Flags;
		public MethodDescFlags2         Flags2         => m_value.Reference.Flags2;
		public MethodDescFlags3         Flags3         => m_value.Reference.Flags3;

		#endregion

		#endregion

		#region Methods

		public MetaIL GetILHeader(int fAllowOverrides = 0)
		{
			RazorContract.Requires(IsIL);
			return new MetaIL(m_value.Reference.GetILHeader(fAllowOverrides));
		}

		/// <summary>
		///     Sets the entry point for this method.
		/// </summary>
		/// <param name="pCode">Pointer to the new entry point</param>
		public void SetStableEntryPoint(Pointer<byte> pCode)
		{
			m_value.Reference.SetStableEntryPoint(pCode);
		}

		public void Reset()
		{
			m_value.Reference.Reset();
		}

		public TDelegate GetDelegate<TDelegate>() where TDelegate : Delegate
		{
			return m_value.Reference.GetDelegate<TDelegate>();
		}

		/// <summary>
		///     JIT the method
		/// </summary>
		public void Prepare()
		{
			m_value.Reference.Prepare();
		}


		public ConsoleTable ToTable()
		{
			var table = new ConsoleTable("Info", "Value");
			table.AddRow("Name", Name);
			table.AddRow("Token", Token);

			table.AddRow("Signature", Info);

			table.AddRow("Classification", Enums.CreateString(Classification));
			table.AddRow("Attributes", Enums.CreateString(Attributes));
			table.AddRow("Flags", Enums.CreateString(Flags));
			table.AddRow("Flags 2", Enums.CreateString(Flags2));
			table.AddRow("Flags 3", Enums.CreateString(Flags3));

			table.AddRow("Function", Hex.ToHex(Function));


			return table;
		}

		#endregion


		public override string ToString()
		{
			return ToTable().ToMarkDownString();
		}
	}

}