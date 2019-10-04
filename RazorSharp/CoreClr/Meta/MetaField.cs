using System;
using System.Reflection;
using RazorSharp.Analysis;
using RazorSharp.CoreClr.Meta.Base;
using RazorSharp.CoreClr.Metadata;
using RazorSharp.CoreClr.Metadata.Enums;
using RazorSharp.Memory;
using RazorSharp.Memory.Components;
using RazorSharp.Memory.Pointers;
using SimpleSharp;
using SimpleSharp.Diagnostics;

// ReSharper disable SuggestBaseTypeForParameter

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Meta
{
	/// <summary>
	///     <list type="bullet">
	///         <item><description>CLR structure: <see cref="FieldDesc"/></description></item>
	///         <item><description>Reflection structure: <see cref="FieldInfo"/></description></item>
	///     </list>
	/// </summary>
	public unsafe class MetaField : EmbeddedClrStructure<FieldDesc>, IStructure
	{
		private const int FIELD_OFFSET_MAX = (1 << 27) - 1;

		private const int FIELD_OFFSET_NEW_ENC = FIELD_OFFSET_MAX - 4;

		private const int DW2_OFFSET_BITS = 27;
		
		#region Constructors

		internal MetaField(Pointer<FieldDesc> ptr) : base(ptr) { }

		public MetaField(FieldInfo info) : this(Runtime.ResolveHandle(info)) { }

		#endregion

		#region Accessors

		public FieldInfo FieldInfo => (FieldInfo) Info;
		
		public CorElementType CorType => Value.Reference.CorType;

		public ProtectionLevel Protection => Value.Reference.ProtectionLevel;

		public int Offset => Value.Reference.Offset;

		public override MemberInfo Info => EnclosingType.RuntimeType.Module.ResolveField(Token);

		public MetaType FieldType => FieldInfo.FieldType;

		public override MetaType EnclosingType {
			get { return (Pointer<MethodTable>) Value.Reference.GetApproxEnclosingMethodTable(); }
		}

		public override int Token => Value.Reference.Token;

		public FieldAttributes Attributes => FieldInfo.Attributes;

//		public bool IsPrivate {
//			get => (((CorFieldAttr)Value.Reference.Dword1) & CorFieldAttr.fdFieldAccessMask) == CorFieldAttr.fdPrivate;
//		}

		#region bool

		public bool IsPointer => Value.Reference.IsPointer;

		public bool IsStatic => Value.Reference.IsStatic;

		public bool IsThreadLocal => Value.Reference.IsThreadLocal;

		public bool IsRVA => Value.Reference.IsRVA;

		public bool IsLiteral => FieldInfo.IsLiteral;

		#endregion

		#region Delegates

		public int Size => Value.Reference.LoadSize();

		public Pointer<byte> GetStaticAddress() => Value.Reference.GetCurrentStaticAddress();

		#endregion

		#endregion
		

		public Pointer<byte> GetValueAddress<T>(ref T value)
		{
			return IsStatic ? GetStaticAddress() : GetAddress(ref value);
		}

		public Pointer<byte> GetAddress<T>(ref T value)
		{
			Conditions.Require(!IsStatic, nameof(IsStatic));
			Conditions.Require(Offset != FIELD_OFFSET_NEW_ENC);

			var data = Unsafe.AddressOfFields(ref value) + Offset;

			return data;
		}

		public object GetValue(object value)
		{
			return FieldInfo.GetValue(value);
		}

		public T GetValue<T>(T value)
		{
			return GetAddress(ref value).Cast<T>().Read();
		}


		#region Operators

		public static implicit operator MetaField(Pointer<FieldDesc> ptr)
		{
			return new MetaField(ptr);
		}

		public static implicit operator MetaField(FieldInfo t)
		{
			return new MetaField(t);
		}

		#region Equality

		

//		public static bool operator ==(MetaField left, MetaField right) => Equals(left, right);

//		public static bool operator !=(MetaField left, MetaField right) => !Equals(left, right);

		#endregion

		#endregion

		#region Overrides

		public override ConsoleTable Debug {
			get {
				var table = base.Debug;

				table.AddRow(nameof(Offset), Offset);
				table.AddRow(nameof(Size), Size);
				table.AddRow(nameof(Type), FieldType.Name);

				if (IsStatic) {
					table.AddRow(nameof(GetStaticAddress), GetStaticAddress());
				}

				return table;
			}
		}

		#endregion
	}
}