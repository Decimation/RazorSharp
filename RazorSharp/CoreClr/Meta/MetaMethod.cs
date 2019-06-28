#region

using System;
using System.Reflection;
using RazorSharp.CoreClr.Meta.Interfaces;
using SimpleSharp;
using SimpleSharp.Diagnostics;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.Enums;
using RazorSharp.Memory.Pointers;
using SimpleSharp.Enums;
using SimpleSharp.Strings;
using SimpleSharp.Strings.Formatting;

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
	public class MetaMethod : IMetadata<MethodDesc>
	{
		internal MetaMethod(Pointer<MethodDesc> methodDesc)
		{
			Value = methodDesc;
			MetaInfoType = MetaInfoType.METHOD;
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
		public int Token => Value.Reference.Token;

		public string Name => Value.Reference.Name;

		public Type EnclosingType => Value.Reference.EnclosingType;

		/// <summary>
		///     The corresponding <see cref="MethodInfo" /> of this <see cref="Structures.MethodDesc" />
		/// </summary>
		public MethodInfo MethodInfo => Value.Reference.Info;

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
			get => Value.Reference.Function;
			set => Value.Reference.Function = value.Address;
		}

		/// <summary>
		///     Returns the address of the native code. The native code can be one of jitted code if
		///     <see cref="IsPreImplemented" /> is <c>false</c> or
		///     ngened code if <see cref="IsPreImplemented" /> is <c>true</c>.
		///     <returns><see cref="IntPtr.Zero" /> if the method has no native code.</returns>
		/// </summary>
		public Pointer<byte> NativeCode => Value.Reference.NativeCode;

		public Pointer<byte> PreImplementedCode => Value.Reference.PreImplementedCode;

		// ChunkIndex
		// MethodDescChunk
		// SizeOf
		// EnclosingMethodTable

		public MetaType EnclosingMetaType => new MetaType(Value.Reference.EnclosingMethodTable);

		public int SizeOf => Value.Reference.SizeOf;

		// RVA


		#region bool

		public bool IsConstructor    => Value.Reference.IsConstructor;
		public bool IsPreImplemented => Value.Reference.IsPreImplemented;
		public bool HasThis          => Value.Reference.HasThis;
		public bool HasILHeader      => Value.Reference.HasILHeader;
		public bool IsUnboxingStub   => Value.Reference.IsUnboxingStub;
		public bool IsIL             => Value.Reference.IsIL;
		public bool IsStatic         => Value.Reference.IsStatic;

		public bool IsPointingToNativeCode => Value.Reference.IsPointingToNativeCode;

		#endregion

		public Pointer<MethodDesc> Value { get; }
		public MetaInfoType MetaInfoType { get; }


		#region Flags

		public MethodClassification     Classification => Value.Reference.Classification;
		public MethodAttributes         Attributes     => Value.Reference.Attributes;
		public MethodDescClassification Flags          => Value.Reference.Flags;
		public MethodDescFlags2         Flags2         => Value.Reference.Flags2;
		public MethodDescFlags3         Flags3         => Value.Reference.Flags3;

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
			return new MetaIL(Value.Reference.GetILHeader(fAllowOverrides));
		}

		public void Reset()
		{
			Value.Reference.Reset();
		}

		public TDelegate GetDelegate<TDelegate>() where TDelegate : Delegate
		{
			return Value.Reference.GetDelegate<TDelegate>();
		}

		/// <summary>
		///     JIT the method
		/// </summary>
		public void Prepare()
		{
			Value.Reference.Prepare();
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