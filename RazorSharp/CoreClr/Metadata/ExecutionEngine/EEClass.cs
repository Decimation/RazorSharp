using System.Reflection;
using System.Runtime.InteropServices;
using RazorSharp.CoreClr.Metadata.Enums;
using RazorSharp.Memory.Pointers;

// ReSharper disable ArrangeAccessorOwnerBody

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Metadata.ExecutionEngine
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct EEClass
	{
		#region Fields

		internal void* GuidInfo { get; }

		internal void* OptionalFields { get; }

		internal MethodTable* MethodTable { get; }

		private FieldDesc* FieldDescListRaw { get; }

		internal void* Chunks { get; }


		#region Union 1

		/// <summary>
		///     <para>Union 1</para>
		///     <para>void* <see cref="OhDelegate" /></para>
		///     <para>uint <see cref="NativeSize" /></para>
		///     <para>int <see cref="ComInterfaceType" /></para>
		/// </summary>
		private void* m_union1;

		internal void* OhDelegate => m_union1;

		internal uint NativeSize {
			get {
				fixed (EEClass* value = &this) {
					Pointer<uint> ptr = &value->m_union1;
					return ptr.Reference;
				}
			}
		}

		internal CorInterfaceAttr ComInterfaceType {
			get {
				fixed (EEClass* value = &this) {
					Pointer<int> ptr = &value->m_union1;
					return (CorInterfaceAttr) ptr.Reference;
				}
			}
		}

		#endregion


		internal void* CCWTemplate { get; }

		internal TypeAttributes Attributes { get; }

		internal VMFlags VMFlags { get; }

		internal CorElementType NormType { get; }

		internal bool FieldsArePacked { get; }

		internal byte FixedEEClassFields { get; }

		internal byte BaseSizePadding { get; }

		#endregion

		#region Accessors

		internal FieldDesc* FieldDescList {
			get {
				//PTR_HOST_MEMBER_TADDR(EEClass, this, m_pFieldDescList)
				return (FieldDesc*) Runtime.HostMemberOffset(ref this, FD_LIST_FIELD_OFFSET, FieldDescListRaw);
			}
		}


		internal Pointer<EEClassLayoutInfo> LayoutInfo {
			get {
				fixed (EEClass* value = &this) {
					var thisptr = ((Pointer<byte>) value)
					             .Add(sizeof(EEClass))
					             .Address;

					return &((LayoutEEClass*) thisptr)->m_LayoutInfo;
				}
			}
		}

		/// <summary>
		///     Abstracted to <see cref="MethodTable"/>
		/// </summary>
		internal int FieldDescListLength {
			get {
				Pointer<EEClass>     pClass     = MethodTable->EEClass;
				int                  fieldCount = pClass.Reference.NumInstanceFields + pClass.Reference.NumStaticFields;
				Pointer<MethodTable> pParentMT  = MethodTable->Parent;

				if (!pParentMT.IsNull)
					fieldCount -= pParentMT.Reference.EEClass.Reference.NumInstanceFields;

				return fieldCount;
			}
		}

		// todo: fd list

		#endregion

		#region Packed fields

		internal int NumInstanceFields  => GetPackableField(EEClassFieldId.NumInstanceFields);
		internal int NumStaticFields    => GetPackableField(EEClassFieldId.NumStaticFields);
		internal int NumMethods         => GetPackableField(EEClassFieldId.NumMethods);
		internal int NumNonVirtualSlots => GetPackableField(EEClassFieldId.NumNonVirtualSlots);

		private int GetPackableField(EEClassFieldId eField)
		{
			uint u = (uint) eField;
			return (int) (FieldsArePacked ? PackedFields->GetPackedField(u) : PackedFields->GetUnpackedField(u));
		}

		private PackedDWORDFields* PackedFields {
			get {
				fixed (EEClass* value = &this) {
					var bp = (Pointer<byte>) value;
					return (PackedDWORDFields*) bp.Add(FixedEEClassFields);
				}
			}
		}

		#endregion

		#region EEClass offsets

		/// <summary>
		///     Offset for the field <see cref="EEClass.FieldDescList" />
		///     <remarks>
		///         Relative to address of a <see cref="EEClass" />
		///     </remarks>
		/// </summary>
		private const int FD_LIST_FIELD_OFFSET = 24;

		#endregion
	}
}