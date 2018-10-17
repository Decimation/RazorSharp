#region

using System;
using System.Reflection;
using RazorSharp.CLR.Structures;
using RazorSharp.CLR.Structures.EE;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

#endregion

namespace RazorSharp.CLR.Meta
{

	/// <summary>
	/// Exposes metadata from <see cref="MethodTable"/> and <see cref="EEClass"/>
	/// </summary>
	public class MetaType : IMeta
	{
		private readonly Pointer<MethodTable> m_pMethodTable;


		internal MetaType(Pointer<MethodTable> p)
		{
			m_pMethodTable = p;


			if (!p.Reference.Canon.IsNull && p.Reference.Canon.Address != p.Address)
				m_canon = new MetaType(p.Reference.Canon);

			if (p.Reference.IsArray)
				m_elementType = new MetaType(p.Reference.ElementTypeHandle);

			if (!p.Reference.Parent.IsNull)
				m_parent = new MetaType(p.Reference.Parent);
		}


		public MetaField this[string name] {
			get { return GetField(name); }
		}

		public MetaField GetField(string name)
		{
			return new MetaField(GetFieldDesc(RuntimeType,name));
		}

		public MetaField[] GetFields()
		{
			var fields = GetFieldDescs(RuntimeType);
			var meta = new MetaField[fields.Length];
			for (int i = 0; i < fields.Length; i++) {
				meta[i] = new MetaField(fields[i]);
			}

			return meta;
		}

		private unsafe Pointer<FieldDesc>[] GetFieldDescs(Type t)
		{
//			RazorContract.Requires(!t.IsArray, "Arrays do not have fields");

			int                  len  = m_pMethodTable.Reference.FieldDescListLength;
			Pointer<FieldDesc>[] lpFd = new Pointer<FieldDesc>[len];

			for (int i = 0; i < len; i++)
				lpFd[i] = &m_pMethodTable.Reference.FieldDescList[i];


			// Adds about 1k ns
//			lpFd = lpFd.OrderBy(x => x.ToInt64()).ToArray();


			return lpFd;
		}

		private static Pointer<FieldDesc> GetFieldDesc(Type t, string name,
			SpecialFieldTypes fieldTypes = SpecialFieldTypes.None, BindingFlags flags = Runtime.DefaultFlags)
		{
			RazorContract.Requires(!t.IsArray, "Arrays do not have fields");

			switch (fieldTypes) {
				case SpecialFieldTypes.AutoProperty:
					name = SpecialNames.NameOfAutoPropertyBackingField(name);
					break;

				case SpecialFieldTypes.None:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(fieldTypes), fieldTypes, null);
			}


			FieldInfo fieldInfo = t.GetField(name, flags);
			RazorContract.RequiresNotNull(fieldInfo);
			Pointer<FieldDesc> fieldDesc = fieldInfo.FieldHandle.Value;
			RazorContract.Assert(fieldDesc.Reference.Info == fieldInfo);
			RazorContract.Assert(fieldDesc.Reference.Token == fieldInfo.MetadataToken);

			return fieldDesc;
		}



		public MetaType Canon => m_canon;

		public MetaType ElementType => m_elementType;

		public MetaType Parent => m_parent;


		public TypeAttributes TypeAttributes => m_pMethodTable.Reference.EEClass.Reference.TypeAttributes;

		public int BaseSizePadding => m_pMethodTable.Reference.EEClass.Reference.BaseSizePadding;

		public VMFlags        VmFlags    => m_pMethodTable.Reference.EEClass.Reference.VMFlags;
		public CorElementType NormalType => m_pMethodTable.Reference.EEClass.Reference.NormalType;


		public int BaseSize => m_pMethodTable.Reference.BaseSize;

		public int ComponentSize => m_pMethodTable.Reference.ComponentSize;


		private readonly MetaType m_parent;
		private readonly MetaType m_canon;
		private readonly MetaType m_elementType;


		public bool HasComponentSize => m_pMethodTable.Reference.HasComponentSize;

		public bool IsArray => m_pMethodTable.Reference.IsArray;

		public bool IsStringOrArray => m_pMethodTable.Reference.IsStringOrArray;

		public bool IsBlittable => m_pMethodTable.Reference.IsBlittable;

		public bool IsString => m_pMethodTable.Reference.IsString;

		public bool ContainsPointers => m_pMethodTable.Reference.ContainsPointers;

		public string Name => m_pMethodTable.Reference.Name;

		public int Token => m_pMethodTable.Reference.Token;

		public Type RuntimeType => m_pMethodTable.Reference.RuntimeType;

		public int NumInstanceFields => m_pMethodTable.Reference.NumInstanceFields;

		public int NumStaticFields => m_pMethodTable.Reference.NumStaticFields;

		public int NumNonVirtualSlots => m_pMethodTable.Reference.NumNonVirtualSlots;

		public int NumMethods => m_pMethodTable.Reference.NumMethods;

		public int NumInstanceFieldBytes => m_pMethodTable.Reference.NumInstanceFieldBytes;

		public int NumVirtuals => m_pMethodTable.Reference.NumVirtuals;

		public int NumInterfaces => m_pMethodTable.Reference.NumInterfaces;

		public MethodTableFlags    Flags    => m_pMethodTable.Reference.Flags;
		public MethodTableFlags2   Flags2   => m_pMethodTable.Reference.Flags2;
		public MethodTableFlagsLow FlagsLow => m_pMethodTable.Reference.FlagsLow;


		public override string ToString()
		{
			return String.Format("{0}", Name);
		}
	}

}