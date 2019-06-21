#region

#region

using System;
using System.Reflection;
using RazorSharp.CoreClr.Meta.Interfaces;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.Enums;
using RazorSharp.Memory.Pointers;
using SimpleSharp;
using SimpleSharp.Strings;

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
	///                 <see cref="Structures.FieldDesc" />
	///             </description>
	///         </item>
	///     </list>
	/// <remarks>Corresponds to <see cref="System.Reflection.FieldInfo"/></remarks>
	/// </summary>
	public class MetaField : IReadableStructure
	{
		internal MetaField(Pointer<FieldDesc> p)
		{
			FieldDesc = p;
		}

		public MetaField(FieldInfo field) : this(field.GetFieldDesc()) { }

		public string ToValueString(object value)
		{
			var fieldValue = GetValue(value);

			if (IsAnyPointer) {
				if (Hex.TryCreateHex(fieldValue, out string str)) {
					return str;
				}
			}

			return fieldValue.ToString();
		}

		public override string ToString()
		{
			var info = FieldDesc.Reference.Info;
			return String.Format("{0} {1} (offset: {2}) (size: {3})", info.FieldType.Name, info.Name, Offset, Size);
		}

		#region Accessors

		#region bool

		public bool IsPublic => FieldDesc.Reference.IsPublic;

		public bool IsPrivate => FieldDesc.Reference.IsPrivate;

		public bool IsInternal => FieldDesc.Reference.IsInternal;

		public bool IsPrivateProtected => FieldDesc.Reference.IsPrivateProtected;

		public bool IsProtectedInternal => FieldDesc.Reference.IsProtectedInternal;

		public bool IsPointer => FieldDesc.Reference.IsPointer;

		public bool IsAnyPointer => RtInfo.IsPointer(FieldType);

		/// <summary>
		///     Whether the field is <c>static</c>
		/// </summary>
		public bool IsStatic => FieldDesc.Reference.IsStatic;

		/// <summary>
		///     Whether the field is decorated with a <see cref="ThreadStaticAttribute" /> attribute
		/// </summary>
		public bool IsThreadLocal => FieldDesc.Reference.IsThreadLocal;

		public bool IsRVA => FieldDesc.Reference.IsRVA;

		public bool IsFixedBuffer => FieldDesc.Reference.IsFixedBuffer;

		public bool IsBackingField => FieldDesc.Reference.IsBackingField;

		#endregion

		/// <summary>
		///     Access level of the field
		/// </summary>
		public ProtectionLevel Protection => FieldDesc.Reference.Protection;

		public CorElementType CorType => FieldDesc.Reference.CorType;

		public int MemoryOffset {
			get {
				int ofs = Offset;

				if (!EnclosingMetaType.IsStruct) {
					ofs += Offsets.OffsetToData;
				}

				return ofs;
			}
		}

		/// <summary>
		///     <para>Size of the field</para>
		/// </summary>
		public int Size => FieldDesc.Reference.Size;

		/// <summary>
		///     Field metadata token
		///     <remarks>
		///         <para>Equal to <see cref="System.Reflection.FieldInfo.MetadataToken" /></para>
		///         <para>Equal to WinDbg's <c>!DumpObj</c> <c>"Field"</c> column in hexadecimal format.</para>
		///     </remarks>
		/// </summary>
		public int Token => FieldDesc.Reference.Token;

		/// <summary>
		///     Offset in memory
		///     <remarks>
		///         <para>Equal to WinDbg's <c>!DumpObj</c> <c>"Offset"</c> column in hexadecimal format.</para>
		///     </remarks>
		/// </summary>
		public int Offset {
			get => FieldDesc.Reference.Offset;
			set => FieldDesc.Reference.Offset = value;
		}

		/// <summary>
		///     The corresponding <see cref="FieldInfo" /> of this <see cref="Structures.FieldDesc" />
		/// </summary>
		public FieldInfo FieldInfo => FieldDesc.Reference.Info;

		public MemberInfo Info => FieldInfo;

		public string Name => FieldDesc.Reference.Name;

		public string CleanName {
			get {
//				if (IsFixedBuffer) {
//					return Formatting.TypeNameOfFixedBuffer(Name);
//				}

				if (IsBackingField) {
					return Formatting.NameOfBackingField(Name);
				}

				return Name;
			}
		}

		public Type EnclosingType => FieldDesc.Reference.EnclosingType;

		public Type FieldType => FieldDesc.Reference.FieldType;

		public string TypeName => FieldType.Name;

		public MetaType EnclosingMetaType => new MetaType(FieldDesc.Reference.EnclosingMethodTable);

		private Pointer<FieldDesc> FieldDesc { get; }

		/// <summary>
		/// Points to <see cref="FieldDesc"/>
		/// </summary>
		public Pointer<byte> Value => FieldDesc.Cast();

//		public Pointer<MethodTable> FieldMethodTable => m_pFieldDesc.Reference.FieldMethodTable;

//		public Pointer<MethodTable> EnclosingMethodTable => m_pFieldDesc.Reference.EnclosingMethodTable;

		#endregion

		#region Methods

		public Pointer<byte> GetValueAddress<T>(ref T value)
		{
			return IsStatic ? GetCurrentStaticAddress() : GetAddress(ref value);
		}

		public Pointer<byte> GetStaticAddress()
		{
			return FieldDesc.Reference.GetStaticAddress();
		}

		public unsafe Pointer<byte> GetStaticAddress(Pointer<byte> value)
		{
			return FieldDesc.Reference.GetStaticAddress(value.ToPointer());
		}

		public Pointer<byte> GetStaticAddressHandle()
		{
			return FieldDesc.Reference.GetStaticAddressHandle();
		}

		public Pointer<byte> GetCurrentStaticAddress()
		{
			return FieldDesc.Reference.GetCurrentStaticAddress();
		}

		public object GetValue(object value)
		{
			return FieldDesc.Reference.GetValue(value);
		}

		public void SetValue(object t, object value)
		{
			FieldDesc.Reference.SetValue(t, value);
		}

		public Pointer<byte> GetAddress<TInstance>(ref TInstance value)
		{
			return FieldDesc.Reference.GetAddress(ref value);
		}

		#endregion

		public static implicit operator MetaField(FieldInfo fieldInfo) => new MetaField(fieldInfo);
	}
}