#region

using System;
using System.Reflection;
using RazorCommon;
using RazorCommon.Utilities;
using RazorSharp.CoreClr.Enums.MethodDesc;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Meta
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


		public override string ToString()
		{
			return ToTable().ToMarkDownString();
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
		///         <see cref="set_Function" /> sets the method entry point
		///         (<see cref="MethodDesc.SetStableEntryPoint" />).
		///     </para>
		/// </summary>
		public Pointer<byte> Function {
			get => m_value.Reference.Function;
			set => m_value.Reference.Function = value.Address;
		}

		/// <summary>
		///     Returns the address of the native code. The native code can be one of jitted code if
		///     <see cref="IsPreImplemented" /> is <c>false</c> or
		///     ngened code if <see cref="IsPreImplemented" /> is <c>true</c>.
		///     <returns><see cref="IntPtr.Zero" /> if the method has no native code.</returns>
		/// </summary>
		public Pointer<byte> NativeCode => m_value.Reference.NativeCode;

		public Pointer<byte> PreImplementedCode => m_value.Reference.PreImplementedCode;

		// ChunkIndex
		// MethodDescChunk
		// SizeOf
		// EnclosingMethodTable

		public MetaType EnclosingMetaType => new MetaType(m_value.Reference.EnclosingMethodTable);

		public int SizeOf => m_value.Reference.SizeOf;

		// RVA


		#region bool

		public bool IsConstructor    => m_value.Reference.IsConstructor;
		public bool IsPreImplemented => m_value.Reference.IsPreImplemented;
		public bool HasThis          => m_value.Reference.HasThis;
		public bool HasILHeader      => m_value.Reference.HasILHeader;
		public bool IsUnboxingStub   => m_value.Reference.IsUnboxingStub;
		public bool IsIL             => m_value.Reference.IsIL;
		public bool IsStatic         => m_value.Reference.IsStatic;

		public bool IsPointingToNativeCode => m_value.Reference.IsPointingToNativeCode;

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

		/// <summary>
		///     Prepares this method if this method will be the goal of a hook (not the method being hooked).
		/// </summary>
		internal void PrepareOverride()
		{
			Reset();
			if (!IsPointingToNativeCode) Prepare();
		}

		public MetaIL GetILHeader(int fAllowOverrides = 0)
		{
			Conditions.Assert(IsIL);
			return new MetaIL(m_value.Reference.GetILHeader(fAllowOverrides));
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

			table.AddRow("Classification", EnumUtil.CreateString(Classification));
			table.AddRow("Attributes", EnumUtil.CreateString(Attributes));
			table.AddRow("Flags", EnumUtil.CreateString(Flags));
			table.AddRow("Flags 2", EnumUtil.CreateString(Flags2));
			table.AddRow("Flags 3", EnumUtil.CreateString(Flags3));

			table.AddRow("Function", Hex.ToHex(Function.ToInt64()));
			table.AddRow("Native code", Hex.ToHex(NativeCode.ToInt64()));
			table.AddRow("Pointing to native code",
			             String.Format("{0} ({1})", IsPointingToNativeCode.Prettify(), IsPointingToNativeCode));

			return table;
		}

		#endregion
	}
}