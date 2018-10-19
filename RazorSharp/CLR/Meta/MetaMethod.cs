#region

using System;
using System.Reflection;
using RazorSharp.CLR.Structures;
using RazorSharp.CLR.Structures.ILMethods;
using RazorSharp.Pointers;

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
	public class MetaMethod : IMeta
	{
		private readonly Pointer<MethodDesc> m_value;

		internal MetaMethod(Pointer<MethodDesc> methodDesc)
		{
			m_value = methodDesc;
		}

		#region Accessors

		public int Token => m_value.Reference.Token;

		public string Name => m_value.Reference.Name;

		public Type EnclosingType => m_value.Reference.EnclosingType;

		public MethodInfo Info => m_value.Reference.Info;

		public IntPtr Function {
			get => m_value.Reference.Function;
			set => m_value.Reference.Function = value;
		}

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

		public Pointer<ILMethod> GetILHeader(int fAllowOverrides = 0)
		{
			return m_value.Reference.GetILHeader(fAllowOverrides);
		}

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

		public void Prepare()
		{
			m_value.Reference.Prepare();
		}

		#endregion

	}

}