#region

using System;
using System.Reflection;
using RazorSharp.CLR.Structures;
using RazorSharp.CLR.Structures.EE;
using RazorSharp.Pointers;

#endregion

namespace RazorSharp.CLR.Meta
{

	public class MetaType : IMeta
	{
		private readonly Pointer<MethodTable> m_pMethodTable;

		internal MetaType(Pointer<MethodTable> p)
		{
			m_pMethodTable = p;
		}

		public MetaField[] GetFields(Type t)
		{
			Pointer<FieldDesc>[] fieldDescs = Runtime.GetFieldDescs(t);
			MetaField[]          fields     = new MetaField[fieldDescs.Length];
			for (int i = 0; i < fields.Length; i++) {
				fields[i] = new MetaField(fieldDescs[i]);
			}

			return fields;
		}

		public MetaField GetField(Type t, string name)
		{
			return new MetaField(Runtime.GetFieldDesc(t, name));
		}

		public TypeAttributes TypeAttributes => m_pMethodTable.Reference.EEClass.Reference.TypeAttributes;

		public int BaseSizePadding => (int) m_pMethodTable.Reference.EEClass.Reference.BaseSizePadding;

		public VMFlags        VmFlags    => m_pMethodTable.Reference.EEClass.Reference.VMFlags;
		public CorElementType NormalType => m_pMethodTable.Reference.EEClass.Reference.NormalType;
		public bool           HasLayout  => m_pMethodTable.Reference.EEClass.Reference.HasLayout;

		public int BaseSize => m_pMethodTable.Reference.BaseSize;

		public int ComponentSize => m_pMethodTable.Reference.ComponentSize;

		public int NumVirtuals => m_pMethodTable.Reference.NumVirtuals;

		public int NumInterfaces => m_pMethodTable.Reference.NumInterfaces;

		public Pointer<MethodTable> Parent => m_pMethodTable.Reference.Parent;

		public Pointer<EEClass> EEClass => m_pMethodTable.Reference.EEClass;

		public Pointer<MethodTable> Canon => m_pMethodTable.Reference.Canon;

		public Pointer<MethodTable> ElementTypeHandle => m_pMethodTable.Reference.ElementTypeHandle;

		public bool HasComponentSize => m_pMethodTable.Reference.HasComponentSize;

		public bool IsArray => m_pMethodTable.Reference.IsArray;

		public bool IsStringOrArray => m_pMethodTable.Reference.IsStringOrArray;

		public bool IsBlittable => m_pMethodTable.Reference.IsBlittable;

		public bool IsString => m_pMethodTable.Reference.IsString;

		public bool ContainsPointers => m_pMethodTable.Reference.ContainsPointers;

		public string Name => m_pMethodTable.Reference.Name;

		public int Token => m_pMethodTable.Reference.MDToken;

		public Type RuntimeType => m_pMethodTable.Reference.RuntimeType;

		public int NumInstanceFields => m_pMethodTable.Reference.NumInstanceFields;

		public int NumStaticFields => m_pMethodTable.Reference.NumStaticFields;

		public int NumNonVirtualSlots => m_pMethodTable.Reference.NumNonVirtualSlots;

		public int NumMethods => m_pMethodTable.Reference.NumMethods;

		public int NumInstanceFieldBytes => m_pMethodTable.Reference.NumInstanceFieldBytes;


	}

}