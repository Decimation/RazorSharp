using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using RazorSharp.CoreClr.Meta.Base;
using RazorSharp.CoreClr.Metadata;
using RazorSharp.CoreClr.Metadata.Enums;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities;
using SimpleSharp;

// ReSharper disable SuggestBaseTypeForParameter


// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Meta
{
	/// <summary>
	///     <list type="bullet">
	///         <item><description>CLR structure: <see cref="MethodDesc"/></description></item>
	///         <item><description>Reflection structure: <see cref="MethodInfo"/></description></item>
	///     </list>
	/// </summary>
	public unsafe class MetaMethod : EmbeddedClrStructure<MethodDesc>
	{
		#region Constructors

		internal MetaMethod(Pointer<MethodDesc> ptr) : base(ptr) { }

		public MetaMethod(MethodInfo member) : this(Runtime.ResolveHandle(member)) { }

		#endregion

		#region Accessors

		public int ChunkIndex => Value.Reference.ChunkIndex;

		public int SlotNumber => Value.Reference.SlotNumber;

		#region bool

		public bool IsRuntimeSupplied =>
			Classification == MethodClassification.FCall ||
			Classification == MethodClassification.Array;

		public bool IsNoMetadata => Classification == MethodClassification.Dynamic;

		public bool HasILHeader => IsIL && !IsUnboxingStub && RVA > default(long);

		private bool IsUnboxingStub => Flags2.HasFlagFast(MethodDescFlags2.IsUnboxingStub);

		public bool IsIL => MethodClassification.IL == Classification ||
		                    MethodClassification.Instantiated == Classification;

		public bool IsPreImplemented => !PreImplementedCode.IsNull;

		#endregion

		#region Flags

		public MethodClassification     Classification => Value.Reference.Classification;
		public MethodDescClassification Flags          => Value.Reference.Flags;
		public MethodDescFlags2         Flags2         => Value.Reference.Flags2;
		public MethodDescFlags3         Flags3         => Value.Reference.Flags3AndTokenRemainder;
		public MethodAttributes         Attributes     => MethodInfo.Attributes;

		#endregion

		public MethodInfo MethodInfo => (MethodInfo) Info;

		public override MemberInfo Info => EnclosingRuntimeType.RuntimeType.Module.ResolveMethod(Token);

		public void Prepare()
		{
			RuntimeHelpers.PrepareMethod(MethodInfo.MethodHandle);
		}

		protected override ConsoleTable InfoTable {
			get {
				var table = base.InfoTable;
				table.AddRow(nameof(Classification), Classification);
				return table;
			}
		}

		#endregion

		#region Delegates

		public long RVA => Value.Reference.RVA;

		[Obsolete]
		private MetaIL GetILHeaderInternal(bool fAllowOverrides = default)
		{
			return new MetaIL(Value.Reference.GetILHeader(Convert.ToInt32(fAllowOverrides)));
		}

		[Obsolete]
		public MetaIL ILHeader => GetILHeaderInternal();

		public override MetaType EnclosingType => (Pointer<MethodTable>) Value.Reference.MethodTable;

		public override int Token => Value.Reference.Token;

		public Pointer<byte> NativeCode {
			get => Value.Reference.NativeCode;
			set => Value.Reference.SetNativeCodeInterlocked(value.ToInt64());
		}

		public Pointer<byte> Function {
			get => MethodInfo.MethodHandle.GetFunctionPointer();
			set => NativeCode = value;
		}

		public void Reset() => Value.Reference.Reset();

		public bool IsPointingToNativeCode => Value.Reference.IsPointingToNativeCode();

		public Pointer<byte> PreImplementedCode => Value.Reference.PreImplementedCode;

		#endregion

		#region Operators

		public static implicit operator MetaMethod(Pointer<MethodDesc> ptr) => new MetaMethod(ptr);

		public static implicit operator MetaMethod(MethodInfo t) => new MetaMethod(t);

		#endregion
	}
}