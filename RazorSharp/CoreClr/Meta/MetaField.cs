#region

#region

using System;
using System.Reflection;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.Enums;
using RazorSharp.Memory.Pointers;
using SimpleSharp;

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
	///                 <see cref="FieldDesc" />
	///             </description>
	///         </item>
	///     </list>
	/// <remarks>Corresponds to <see cref="System.Reflection.FieldInfo"/></remarks>
	/// </summary>
	public class MetaField : IReadableStructure
	{
		internal MetaField(Pointer<FieldDesc> p)
		{
			Value = p;
		}

		public MetaField(FieldInfo field) : this(field.GetFieldDesc()) { }

		public override string ToString()
		{
			var info = Value.Reference.Info;
			return String.Format("{0} {1} (offset: {2}) (size: {3})", info.FieldType.Name, info.Name, Offset, Size);
		}

		#region Accessors

		#region bool

		public bool IsPublic => Value.Reference.IsPublic;

		public bool IsPrivate => Value.Reference.IsPrivate;

		public bool IsInternal => Value.Reference.IsInternal;

		public bool IsPrivateProtected => Value.Reference.IsPrivateProtected;

		public bool IsProtectedInternal => Value.Reference.IsProtectedInternal;

		public bool IsPointer => Value.Reference.IsPointer;

		/// <summary>
		///     Whether the field is <c>static</c>
		/// </summary>
		public bool IsStatic => Value.Reference.IsStatic;

		/// <summary>
		///     Whether the field is decorated with a <see cref="ThreadStaticAttribute" /> attribute
		/// </summary>
		public bool IsThreadLocal => Value.Reference.IsThreadLocal;

		public bool IsRVA => Value.Reference.IsRVA;

		public bool IsFixedBuffer => Value.Reference.IsFixedBuffer;

		public bool IsAutoProperty => Value.Reference.IsAutoProperty;

		#endregion

		/// <summary>
		///     Access level of the field
		/// </summary>
		public ProtectionLevel Protection => Value.Reference.Protection;

		public CorElementType CorType => Value.Reference.CorType;

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
		public int Size => Value.Reference.Size;

		/// <summary>
		///     Field metadata token
		///     <remarks>
		///         <para>Equal to <see cref="System.Reflection.FieldInfo.MetadataToken" /></para>
		///         <para>Equal to WinDbg's <c>!DumpObj</c> <c>"Field"</c> column in hexadecimal format.</para>
		///     </remarks>
		/// </summary>
		public int Token => Value.Reference.Token;

		/// <summary>
		///     Offset in memory
		///     <remarks>
		///         <para>Equal to WinDbg's <c>!DumpObj</c> <c>"Offset"</c> column in hexadecimal format.</para>
		///     </remarks>
		/// </summary>
		public int Offset {
			get => Value.Reference.Offset;
			set => Value.Reference.Offset = value;
		}

		/// <summary>
		///     The corresponding <see cref="FieldInfo" /> of this <see cref="FieldDesc" />
		/// </summary>
		public FieldInfo FieldInfo => Value.Reference.Info;

		public MemberInfo Info => FieldInfo;

		public string Name => Value.Reference.Name;

		public Type EnclosingType => Value.Reference.EnclosingType;

		public Type FieldType => Value.Reference.FieldType;

		public string TypeName => FieldType.Name;

		public MetaType EnclosingMetaType => new MetaType(Value.Reference.EnclosingMethodTable);

		internal Pointer<FieldDesc> Value { get; }

//		public Pointer<MethodTable> FieldMethodTable => m_pFieldDesc.Reference.FieldMethodTable;

//		public Pointer<MethodTable> EnclosingMethodTable => m_pFieldDesc.Reference.EnclosingMethodTable;

		#endregion

		#region Methods

		public Pointer<byte> GetValueAddress<T>(ref T value)
		{
			return IsStatic ? GetStaticAddressContext() : GetAddress(ref value);
		}

		public Pointer<byte> GetStaticAddress()
		{
			return Value.Reference.GetStaticAddress();
		}

		public unsafe Pointer<byte> GetStaticAddress(Pointer<byte> value)
		{
			return Value.Reference.GetStaticAddress(value.ToPointer());
		}

		public Pointer<byte> GetStaticAddressHandle()
		{
			return Value.Reference.GetStaticAddressHandle();
		}

		public Pointer<byte> GetStaticAddressContext()
		{
			return Value.Reference.GetStaticAddressContext();
		}

		public object GetValue(object value)
		{
			return Value.Reference.GetValue(value);
		}

		public void SetValue(object t, object value)
		{
			Value.Reference.SetValue(t, value);
		}

		public Pointer<byte> GetAddress<TInstance>(ref TInstance value)
		{
			return Value.Reference.GetAddress(ref value);
		}

		#endregion

		public static implicit operator MetaField(FieldInfo fieldInfo) => new MetaField(fieldInfo);
	}
}