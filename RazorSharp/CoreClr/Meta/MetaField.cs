#region

#region

using System;
using System.Reflection;
using RazorSharp.CoreClr.Enums;
using RazorSharp.CoreClr.Enums.FieldDesc;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Pointers;

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
		private readonly Pointer<FieldDesc> m_value;

		
		internal MetaField(Pointer<FieldDesc> p)
		{
			m_value = p;
		}


		public override string ToString()
		{
			var info = m_value.Reference.Info;
			return String.Format("{0} {1} (offset: {2}) (size: {3})", info.FieldType.Name, info.Name, Offset, Size);
		}

		#region Accessors

		#region bool

		public bool IsPublic => m_value.Reference.IsPublic;

		public bool IsPrivate => m_value.Reference.IsPrivate;

		public bool IsInternal => m_value.Reference.IsInternal;

		public bool IsPrivateProtected => m_value.Reference.IsPrivateProtected;

		public bool IsProtectedInternal => m_value.Reference.IsProtectedInternal;

		public bool IsPointer => m_value.Reference.IsPointer;

		/// <summary>
		///     Whether the field is <c>static</c>
		/// </summary>
		public bool IsStatic => m_value.Reference.IsStatic;

		/// <summary>
		///     Whether the field is decorated with a <see cref="ThreadStaticAttribute" /> attribute
		/// </summary>
		public bool IsThreadLocal => m_value.Reference.IsThreadLocal;

		public bool IsRVA => m_value.Reference.IsRVA;

		public bool IsFixedBuffer => m_value.Reference.IsFixedBuffer;

		public bool IsAutoProperty => m_value.Reference.IsAutoProperty;

		#endregion

		/// <summary>
		///     Access level of the field
		/// </summary>
		public ProtectionLevel Protection => m_value.Reference.Protection;

		public CorElementType CorType => m_value.Reference.CorType;

		/// <summary>
		///     <para>Size of the field</para>
		/// </summary>
		public int Size => m_value.Reference.Size;

		/// <summary>
		///     Field metadata token
		///     <remarks>
		///         <para>Equal to <see cref="System.Reflection.FieldInfo.MetadataToken" /></para>
		///         <para>Equal to WinDbg's <c>!DumpObj</c> <c>"Field"</c> column in hexadecimal format.</para>
		///     </remarks>
		/// </summary>
		public int Token => m_value.Reference.Token;

		/// <summary>
		///     Offset in memory
		///     <remarks>
		///         <para>Equal to WinDbg's <c>!DumpObj</c> <c>"Offset"</c> column in hexadecimal format.</para>
		///     </remarks>
		/// </summary>
		public int Offset {
			get {
				
				return m_value.Reference.Offset;
				
			}
			set {
				m_value.Reference.Offset = value;
			}
		}

		/// <summary>
		///     The corresponding <see cref="FieldInfo" /> of this <see cref="FieldDesc" />
		/// </summary>
		public FieldInfo FieldInfo => m_value.Reference.Info;

		public MemberInfo Info => FieldInfo;

		public string Name => m_value.Reference.Name;

		public Type EnclosingType => m_value.Reference.EnclosingType;

		public Type FieldType => m_value.Reference.FieldType;

		public MetaType EnclosingMetaType => new MetaType(m_value.Reference.EnclosingMethodTable);

		

//		public Pointer<MethodTable> FieldMethodTable => m_pFieldDesc.Reference.FieldMethodTable;

//		public Pointer<MethodTable> EnclosingMethodTable => m_pFieldDesc.Reference.EnclosingMethodTable;

		#endregion

		#region Methods

		public unsafe Pointer<byte> GetStaticAddress()
		{
			return m_value.Reference.GetStaticAddress();
		}
		
		public unsafe Pointer<byte> GetStaticAddress(Pointer<byte> value)
		{
			return m_value.Reference.GetStaticAddress(value.ToPointer());
		}
		
		public unsafe Pointer<byte> GetStaticAddressHandle()
		{
			return m_value.Reference.GetStaticAddressHandle();
		}
		
		public unsafe Pointer<byte> GetStaticAddressContext()
		{
			return m_value.Reference.GetStaticAddressContext();
		}

		public object GetValue(object value)
		{
			return m_value.Reference.GetValue(value);
		}
		
		public object GetValue<TInstance>(TInstance t)
		{
			return m_value.Reference.GetValue(t);
		}

		public void SetValue<TInstance>(TInstance t, object value)
		{
			m_value.Reference.SetValue(t, value);
		}

		public Pointer<byte> GetAddress<TInstance>(ref TInstance t)
		{
			
			return m_value.Reference.GetAddress(ref t);
		}

		#endregion
	}
}