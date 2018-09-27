#region

using System;
using System.Reflection;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.CLR.Meta
{

	public class MetaField : IMeta
	{
		private readonly Pointer<FieldDesc> m_pFieldDesc;

		public int Token => m_pFieldDesc.Reference.Token;

		public int Offset => m_pFieldDesc.Reference.Offset;

		public bool IsPublic => m_pFieldDesc.Reference.IsPublic;

		public bool IsPrivate => m_pFieldDesc.Reference.IsPrivate;

		public bool IsInternal => m_pFieldDesc.Reference.IsInternal;

		public bool IsPrivateProtected => m_pFieldDesc.Reference.IsPrivateProtected;

		public bool IsProtectedInternal => m_pFieldDesc.Reference.IsProtectedInternal;

		public CorElementType CorType => m_pFieldDesc.Reference.CorType;

		public bool IsPointer => m_pFieldDesc.Reference.IsPointer;

		public bool IsStatic => m_pFieldDesc.Reference.IsStatic;

		public bool IsThreadLocal => m_pFieldDesc.Reference.IsThreadLocal;

		public bool IsRVA => m_pFieldDesc.Reference.IsRVA;

		public bool IsFixedBuffer => m_pFieldDesc.Reference.IsFixedBuffer;

		public bool IsAutoProperty => m_pFieldDesc.Reference.IsAutoProperty;

		public ProtectionLevel Protection => m_pFieldDesc.Reference.Protection;

		public int Size => m_pFieldDesc.Reference.Size;

		public FieldInfo Info => m_pFieldDesc.Reference.Info;

		public string Name => m_pFieldDesc.Reference.Name;

		public Type EnclosingType => m_pFieldDesc.Reference.EnclosingType;

//		public Pointer<MethodTable> FieldMethodTable => m_pFieldDesc.Reference.FieldMethodTable;

//		public Pointer<MethodTable> EnclosingMethodTable => m_pFieldDesc.Reference.EnclosingMethodTable;

		public object GetValue<TInstance>(TInstance t)
		{
			return m_pFieldDesc.Reference.GetValue(t);
		}

		public void SetValue<TInstance>(TInstance t, object value)
		{
			m_pFieldDesc.Reference.SetValue(t, value);
		}

		public Pointer<byte> GetAddress<TInstance>(ref TInstance t)
		{
			return m_pFieldDesc.Reference.GetAddress(ref t);
		}

		internal MetaField(Pointer<FieldDesc> p)
		{
			m_pFieldDesc = p;
		}

//		public static implicit operator MetaField(Pointer<FieldDesc> p) {}
	}

}