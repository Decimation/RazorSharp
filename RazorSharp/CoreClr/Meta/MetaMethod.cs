#region

using System;
using System.Reflection;
using RazorSharp.CoreClr.Meta.Interfaces;
using SimpleSharp;
using SimpleSharp.Diagnostics;
using SimpleSharp.Strings;
using SimpleSharp.Utilities;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.Enums;
using RazorSharp.Memory.Pointers;

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Meta
{
	/// <summary>
	///     Exposes metadata from:
	///     <list type="bullet">
	///         <item>
	///             <description>
	///                 <see cref="Structures.MethodDesc" />
	///             </description>
	///         </item>
	///     </list>
	/// <remarks>Corresponds to <see cref="System.Reflection.MethodInfo"/></remarks>
	/// </summary>
	public class MetaMethod : IMetadata
	{
		internal MetaMethod(Pointer<MethodDesc> methodDesc)
		{
			MethodDesc = methodDesc;
		}

		public MetaMethod(MethodInfo methodInfo) : this(methodInfo.GetMethodDesc()) { }


		public override string ToString()
		{
			return ToTable().ToString();
		}

		#region Accessors

		/// <summary>
		///     <para>Metadata token of this method</para>
		///     <remarks>
		///         <para>Equal to <see cref="System.Reflection.MethodInfo.MetadataToken" /></para>
		///         <para>Address-sensitive</para>
		///     </remarks>
		/// </summary>
		public int Token => MethodDesc.Reference.Token;

		public string Name => MethodDesc.Reference.Name;

		public Type EnclosingType => MethodDesc.Reference.EnclosingType;

		/// <summary>
		///     The corresponding <see cref="MethodInfo" /> of this <see cref="Structures.MethodDesc" />
		/// </summary>
		public MethodInfo MethodInfo => MethodDesc.Reference.Info;

		public MemberInfo Info => MethodInfo;

		/// <summary>
		///     Function pointer (entry point) of this method.
		///     <para>
		///         <see cref="get_Function" /> returns the entry point
		///         (<see cref="RuntimeMethodHandle.GetFunctionPointer()" />) of this method.
		///     </para>
		///     <para>
		///         <see cref="set_Function" /> sets the method entry point
		///         (<see cref="Structures.MethodDesc.SetEntryPoint" />).
		///     </para>
		/// </summary>
		public Pointer<byte> Function {
			get => MethodDesc.Reference.Function;
			set => MethodDesc.Reference.Function = value.Address;
		}

		/// <summary>
		///     Returns the address of the native code. The native code can be one of jitted code if
		///     <see cref="IsPreImplemented" /> is <c>false</c> or
		///     ngened code if <see cref="IsPreImplemented" /> is <c>true</c>.
		///     <returns><see cref="IntPtr.Zero" /> if the method has no native code.</returns>
		/// </summary>
		public Pointer<byte> NativeCode => MethodDesc.Reference.NativeCode;

		public Pointer<byte> PreImplementedCode => MethodDesc.Reference.PreImplementedCode;

		// ChunkIndex
		// MethodDescChunk
		// SizeOf
		// EnclosingMethodTable

		public MetaType EnclosingMetaType => new MetaType(MethodDesc.Reference.EnclosingMethodTable);

		public int SizeOf => MethodDesc.Reference.SizeOf;

		// RVA


		#region bool

		public bool IsConstructor    => MethodDesc.Reference.IsConstructor;
		public bool IsPreImplemented => MethodDesc.Reference.IsPreImplemented;
		public bool HasThis          => MethodDesc.Reference.HasThis;
		public bool HasILHeader      => MethodDesc.Reference.HasILHeader;
		public bool IsUnboxingStub   => MethodDesc.Reference.IsUnboxingStub;
		public bool IsIL             => MethodDesc.Reference.IsIL;
		public bool IsStatic         => MethodDesc.Reference.IsStatic;

		public bool IsPointingToNativeCode => MethodDesc.Reference.IsPointingToNativeCode;

		#endregion

		private Pointer<MethodDesc> MethodDesc { get; }

		/// <summary>
		/// Points to <see cref="MethodDesc"/>
		/// </summary>
		public Pointer<byte> Value => MethodDesc.Cast();

		#region Flags

		public MethodClassification     Classification => MethodDesc.Reference.Classification;
		public MethodAttributes         Attributes     => MethodDesc.Reference.Attributes;
		public MethodDescClassification Flags          => MethodDesc.Reference.Flags;
		public MethodDescFlags2         Flags2         => MethodDesc.Reference.Flags2;
		public MethodDescFlags3         Flags3         => MethodDesc.Reference.Flags3;

		#endregion

		#endregion

		#region Methods

		/// <summary>
		///     Prepares this method if this method will be the goal of a hook (not the method being hooked).
		/// </summary>
		internal void PrepareOverride()
		{
			Reset();
			if (!IsPointingToNativeCode) 
				Prepare();
		}

		public MetaIL GetILHeader(int fAllowOverrides = 0)
		{
			Conditions.Assert(IsIL);
			return new MetaIL(MethodDesc.Reference.GetILHeader(fAllowOverrides));
		}

		public void Reset()
		{
			MethodDesc.Reference.Reset();
		}

		public TDelegate GetDelegate<TDelegate>() where TDelegate : Delegate
		{
			return MethodDesc.Reference.GetDelegate<TDelegate>();
		}

		/// <summary>
		///     JIT the method
		/// </summary>
		public void Prepare()
		{
			MethodDesc.Reference.Prepare();
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

		public static implicit operator MetaMethod(MethodInfo methodInfo)
		{
			return new MetaMethod(methodInfo);
		}
	}
}