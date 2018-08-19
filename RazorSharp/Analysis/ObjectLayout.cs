#region

using System;
using System.Linq;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;

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
		private readonly IntPtr m_addr;
		private readonly T      m_value;
		private const    char   Omitted = '-';
		private readonly bool   m_fieldsOnly;

		/// <summary>
		///     Whether to include the full byte range of offsets (not included for MethodTable* or Object Header)
		/// </summary>
		private readonly bool m_fullOffset;

		public ConsoleTable Table { get; }

		public ObjectLayout(ref T t, bool fieldsOnly = true, bool fullOffset = false)
		{
			if (typeof(T).IsArray) {
				throw new Exception("You cannot get the layout of an array (yet)");
			}

			m_fieldsOnly = fieldsOnly;
			m_fullOffset = fullOffset;
			m_value      = t;
			IntPtr addr = Unsafe.AddressOf(ref t);
			m_addr = addr;

			// If we're only displaying fields, we'll display the offset relative to the first field

			Table = new ConsoleTable(fieldsOnly ? "Field Offset" : "Memory Offset", "Address", "Size", "Type", "Name",
				"Value");

			if (!typeof(T).IsValueType) {
				// Point to heap
				m_addr = *(IntPtr*) addr;
			}


			Create(ref t);

			if (typeof(T) == typeof(string)) {
				StringCreate();
			}
		}

		private string GetOffsetString(int baseOfs, int rightOfs, int leftOfs)
		{
			string ofsStr;
			if (!m_fieldsOnly) {
				ofsStr = m_fullOffset ? $"{rightOfs}-{leftOfs}" : rightOfs.ToString();
			}
			else {
				// Offset relative to the fields (-IntPtr.Size)
				ofsStr = m_fullOffset ? $"{rightOfs - baseOfs}-{leftOfs - baseOfs}" : (rightOfs - baseOfs).ToString();
			}

			return ofsStr;
		}

		private byte[] TryGetObjHeaderAsBytes()
		{
			return typeof(T).IsValueType ? null : Memory.Memory.ReadBytes(m_addr, -IntPtr.Size, sizeof(ObjHeader));
		}

		private IntPtr TryGetMethodTablePointer()
		{
			return typeof(T).IsValueType ? IntPtr.Zero : Marshal.ReadIntPtr(m_addr);
		}

		private void CreateInternalReferenceTypeInfo()
		{
			const string objHeaderType   = "ObjHeader";
			const string methodTableType = "MethodTable*";
			const string objHeaderName   = "(Object header)";
			const string methodTableName = "(MethodTable ptr)";

			if (!m_fieldsOnly) {
				byte[] objHeaderMem   = TryGetObjHeaderAsBytes();
				IntPtr methodTablePtr = TryGetMethodTablePointer();

				// ObjHeader
				Table.AddRow(-IntPtr.Size, Hex.ToHex(m_addr - IntPtr.Size), IntPtr.Size, objHeaderType,
					objHeaderName, objHeaderMem == null ? Omitted.ToString() : Collections.ToString(objHeaderMem));

				// MethodTable*
				Table.AddRow(0, Hex.ToHex(m_addr), IntPtr.Size, methodTableType, methodTableName,
					methodTablePtr == IntPtr.Zero ? Omitted.ToString() : Hex.ToHex(methodTablePtr));
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

				IntPtr addr = m_addr + charOffset + i * sizeof(char) + baseOfs;
				Table.AddRow(ofsStr, Hex.ToHex(addr), sizeof(char), typeof(char).Name, $"(Character {i + 2})",
					Memory.Memory.Read<char>(addr));
			}
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

				Table.AddRow(ofsStr, Hex.ToHex(v.Reference.GetAddress(ref t)), v.Reference.Size,
					v.Reference.FieldInfo.FieldType.Name, v.Reference.Name, v.Reference.GetValue(t));


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


					Table.AddRow(GetOffsetString(baseOfs, ro, lo), Hex.ToHex(m_addr + padOffset), padSize,
						paddingByte,
						paddingStr, 0);
				}

				// end padding
			}
		}


		public override string ToString()
		{
			return Inspector<T>.CreateLabelString("Memory layout:", Table);
		}
	}

}