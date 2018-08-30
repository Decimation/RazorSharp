#region

using System;
using System.Linq;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

#endregion

namespace RazorSharp.Analysis
{

	/// <summary>
	///     <para>Displays the layout of objects in memory.</para>
	///     <para>If a reference type is specified, the heap layout is displayed.</para>
	///     <para>Does not work with arrays.</para>
	/// </summary>
	/// <typeparam name="T">Type to get the layout of.</typeparam>
	public unsafe class ObjectLayout<T>
	{

		#region Fields

		private readonly IntPtr m_pAddr;
		private readonly T      m_value;
		private const    char   Omitted = '-';
		private readonly bool   m_bFieldsOnly;

		/// <summary>
		///     Whether to include the full byte range of offsets (not included for MethodTable* or Object Header)
		/// </summary>
		private readonly bool m_bFullOffset;


		/// <summary>
		///     If a value of type <typeparamref name="T" /> wasn't supplied in the constructor, we pass a value of <c>default</c>
		///     of type
		///     <typeparamref name="T" /> to <see cref="Create" />. Omit addresses and values in <see cref="Table" />.
		/// </summary>
		private readonly bool m_bIsDefault;

		public ConsoleTable Table { get; }

		#endregion

		#region Constructors

		public ObjectLayout(bool bFieldsOnly = true, bool bFullOffset = false)
		{
			RazorContract.Requires(!typeof(T).IsArray, "You cannot get the layout of an array (yet)");

			m_bFieldsOnly = bFieldsOnly;
			m_bFullOffset = bFullOffset;
			m_pAddr       = IntPtr.Zero;
			m_bIsDefault  = true;


			Table = new ConsoleTable(bFieldsOnly ? "Field Offset" : "Memory Offset", "Address", "Size", "Type", "Name",
				"Value", "Unique attributes");

			T def = default;
			Create(ref def);
		}

		public ObjectLayout(ref T t, bool bFieldsOnly = true, bool bFullOffset = false)
		{
			RazorContract.Requires(!typeof(T).IsArray, "You cannot get the layout of an array (yet)");


			m_bFieldsOnly = bFieldsOnly;
			m_bFullOffset = bFullOffset;
			m_value       = t;
			IntPtr addr = Unsafe.AddressOf(ref t);
			m_pAddr      = addr;
			m_bIsDefault = false;

			// If we're only displaying fields, we'll display the offset relative to the first field

			Table = new ConsoleTable(bFieldsOnly ? "Field Offset" : "Memory Offset", "Address", "Size", "Type", "Name",
				"Value", "Unique attributes");

			if (!typeof(T).IsValueType) {
				// Point to heap
				m_pAddr = *(IntPtr*) addr;
			}


			Create(ref t);


			// Write the remaining chars of the string
			if (typeof(T) == typeof(string)) {
				StringCreate();
			}
		}

		#endregion


		private string GetOffsetString(int baseOfs, int rightOfs, int leftOfs)
		{
			string ofsStr;
			if (!m_bFieldsOnly) {
				ofsStr = m_bFullOffset ? $"{rightOfs}-{leftOfs}" : rightOfs.ToString();
			}
			else {
				// Offset relative to the fields (-IntPtr.Size)
				ofsStr = m_bFullOffset ? $"{rightOfs - baseOfs}-{leftOfs - baseOfs}" : (rightOfs - baseOfs).ToString();
			}

			return ofsStr;
		}

		private byte[] TryGetObjHeaderAsBytes()
		{
			// Only read the second DWORD; the first DWORD is alignment padding
			return typeof(T).IsValueType
				? null
				: Memory.Memory.ReadBytes(m_pAddr, -IntPtr.Size / 2, sizeof(ObjHeader) / 2);
		}

		private IntPtr TryGetMethodTablePointer()
		{
			return typeof(T).IsValueType ? IntPtr.Zero : Marshal.ReadIntPtr(m_pAddr);
		}

		private void CreateInternalReferenceTypeInfo()
		{
			const string objHeaderType   = "ObjHeader";
			const string methodTableType = "MethodTable*";
			const string objHeaderName   = "(Object header)";
			const string methodTableName = "(MethodTable ptr)";

			if (!m_bFieldsOnly) {
				if (m_bIsDefault) {
					// ObjHeader
					Table.AddRow(-IntPtr.Size, Omitted, IntPtr.Size, objHeaderType,
						objHeaderName, Omitted, UniqueAttributes.None);

					// MethodTable*
					Table.AddRow(0, Omitted, IntPtr.Size, methodTableType, methodTableName,
						Omitted, UniqueAttributes.None);

					return;
				}

				byte[] objHeaderMem   = TryGetObjHeaderAsBytes();
				IntPtr methodTablePtr = TryGetMethodTablePointer();


				// ObjHeader
				Table.AddRow(-IntPtr.Size, Hex.ToHex(m_pAddr - IntPtr.Size), IntPtr.Size, objHeaderType,
					objHeaderName, objHeaderMem == null ? Omitted.ToString() : Collections.ToString(objHeaderMem), UniqueAttributes.None);

				// MethodTable*
				Table.AddRow(0, Hex.ToHex(m_pAddr), IntPtr.Size, methodTableType, methodTableName,
					methodTablePtr == IntPtr.Zero ? Omitted.ToString() : Hex.ToHex(methodTablePtr), UniqueAttributes.None);
			}
		}

		private void StringCreate()
		{
			int       baseOfs    = IntPtr.Size;
			string    str        = m_value as string;
			const int charOffset = 6;


			// ReSharper disable once PossibleNullReferenceException
			for (int i = 0; i < str.Length - 1; i++) {
				int rightOfs = baseOfs + charOffset + i * sizeof(char);
				int leftOfs  = rightOfs + (sizeof(char) - 1);

				string ofsStr = GetOffsetString(baseOfs, rightOfs, leftOfs);

				IntPtr addr = m_pAddr + charOffset + i * sizeof(char) + baseOfs;
				Table.AddRow(ofsStr, Hex.ToHex(addr), sizeof(char), typeof(char).Name, $"(Character {i + 2})",
					Memory.Memory.Read<char>(addr), UniqueAttributes.None);
			}
		}

		private enum UniqueAttributes
		{
			None,
			FixedBuffer,
			AutoProperty,
			Padding,
		}

		private static UniqueAttributes FindUniqueAttributes(Pointer<FieldDesc> fd)
		{
			if (fd.Reference.IsFixedBuffer) {
				return UniqueAttributes.FixedBuffer;
			}

			if (fd.Reference.IsAutoProperty) {
				return UniqueAttributes.AutoProperty;
			}

			return UniqueAttributes.None;
		}


		private void Create(ref T t)
		{
			const string paddingStr  = "(padding)";
			const string paddingByte = "Byte";

			Pointer<FieldDesc>[] fieldDescs = Runtime.GetFieldDescs<T>();
			fieldDescs = fieldDescs.OrderBy(x => x.Reference.Offset).ToArray();
			Collections.RemoveAll(ref fieldDescs, x => x.Reference.IsStatic);


			int baseOfs = 0;
			if (!typeof(T).IsValueType) {
				baseOfs += IntPtr.Size;
				CreateInternalReferenceTypeInfo();
			}

			for (int i = 0; i < fieldDescs.Length; i++) {
				Pointer<FieldDesc> v        = fieldDescs[i];
				int                rightOfs = baseOfs + v.Reference.Offset;
				int                leftOfs  = rightOfs + (v.Reference.Size - 1);

				string ofsStr = GetOffsetString(baseOfs, rightOfs, leftOfs);

				if (m_bIsDefault) {
					Table.AddRow(ofsStr, Omitted, v.Reference.Size,
						v.Reference.Info.FieldType.Name, v.Reference.Name, Omitted, FindUniqueAttributes(v));
				}
				else {
					Table.AddRow(ofsStr, Hex.ToHex(v.Reference.GetAddress(ref t)), v.Reference.Size,
						v.Reference.Info.FieldType.Name, v.Reference.Name, v.Reference.GetValue(t),
						FindUniqueAttributes(v));
				}


				// start padding
				int nextOffsetOrSize = Unsafe.BaseFieldsSize<T>();
				if (i != fieldDescs.Length - 1) {
					nextOffsetOrSize = fieldDescs[i + 1].Reference.Offset;
				}

				int nextSectOfsCandidate = fieldDescs[i].Reference.Offset + fieldDescs[i].Reference.Size;

				if (nextSectOfsCandidate < nextOffsetOrSize) {
					int padSize   = nextOffsetOrSize - nextSectOfsCandidate;
					int ro        = baseOfs + nextSectOfsCandidate;
					int lo        = ro + (padSize - 1);
					int padOffset = nextSectOfsCandidate + baseOfs;


					if (m_bIsDefault) {
						Table.AddRow(GetOffsetString(baseOfs, ro, lo), Omitted, padSize,
							paddingByte, paddingStr, 0, UniqueAttributes.Padding);
					}
					else {
						Table.AddRow(GetOffsetString(baseOfs, ro, lo), Hex.ToHex(m_pAddr + padOffset), padSize,
							paddingByte, paddingStr, 0, UniqueAttributes.Padding);
					}
				}

				// end padding
			}

			if (m_bIsDefault) {
				// Remove value and address columns
				Table.RemoveColumn(1).RemoveColumn(4);
			}
		}


		public override string ToString()
		{
			return InspectorHelper.CreateLabelString("Memory layout:", Table);
		}
	}

}