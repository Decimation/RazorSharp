#region

#region

using System;
using System.Reflection;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory.Pointers;

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
	/// </summary>
	public class MetaField : IMetaMember
	{
		internal MetaField(Pointer<FieldDesc> p)
		{
			Value = p;
		}


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

		public MetaType EnclosingMetaType => new MetaType(Value.Reference.EnclosingMethodTable);

		internal Pointer<FieldDesc> Value { get; }

//		public Pointer<MethodTable> FieldMethodTable => m_pFieldDesc.Reference.FieldMethodTable;

//		public Pointer<MethodTable> EnclosingMethodTable => m_pFieldDesc.Reference.EnclosingMethodTable;

		#endregion

		#region Methods

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

		public object GetValue<TInstance>(TInstance t)
		{
			return Value.Reference.GetValue(t);
		}

		public void SetValue<TInstance>(TInstance t, object value)
		{
			Value.Reference.SetValue(t, value);
		}

		public Pointer<byte> GetAddress<TInstance>(ref TInstance t)
		{
			return Value.Reference.GetAddress(ref t);
		}

		public void SetValueByAddr<TInstance, TField>(ref TInstance inst, TField value)
		{
			Pointer<byte> addr = GetAddress(ref inst);
			addr.WriteAny(value);
		}

		#endregion
	}
}