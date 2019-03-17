#region

using System;
using System.Linq;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using static RazorSharp.Memory.Mem;

#endregion

namespace RazorSharp.Analysis
{
	/// <summary>
	///     <para>Displays the layout of objects in memory.</para>
	///     <para>If a reference type is specified, the heap layout is displayed.</para>
	///     <para>Does not work with arrays.</para>
	/// </summary>
	/// <typeparam name="T">Type to get the layout of.</typeparam>
	[Obsolete("Use Inspect.LayoutString")]
	public unsafe class ObjectLayout<T>
	{
		private string GetOffsetString(int baseOfs, int rightOfs, int leftOfs)
		{
			string ofsStr;
			if (!m_bFieldsOnly)
				ofsStr = m_bFullOffset ? $"{rightOfs}-{leftOfs}" : rightOfs.ToString();
			else
				ofsStr = m_bFullOffset ? $"{rightOfs - baseOfs}-{leftOfs - baseOfs}" : (rightOfs - baseOfs).ToString();

			return ofsStr;
		}

		private byte[] TryGetObjHeaderAsBytes()
		{
			if (m_bIsArray) return ReadBytes(m_pAddr, -IntPtr.Size, sizeof(uint));

			// Only read the second DWORD; the first DWORD is alignment padding
			byte[] mem = typeof(T).IsValueType ? null : ReadBytes(m_pAddr, -IntPtr.Size, sizeof(uint));


			return mem;
		}

		private IntPtr TryGetMethodTablePointer()
		{
			if (m_bIsArray) return Marshal.ReadIntPtr(m_pAddr);

			return typeof(T).IsValueType ? IntPtr.Zero : Marshal.ReadIntPtr(m_pAddr);
		}


		private void CreateInternalReferenceTypeInfo()
		{
			if (!m_bFieldsOnly) {
				if (m_bIsDefault) {
					// ObjHeader
					Table.AddRow(-IntPtr.Size, Omitted, IntPtr.Size, OBJHEADER_TYPE_STR,
					             OBJHEADER_NAME_STR, Omitted, UniqueAttributes.None);

					// MethodTable*
					Table.AddRow(0, Omitted, IntPtr.Size, METHODTABLE_TYPE_STR, METHODTABLE_NAME_STR,
					             Omitted, UniqueAttributes.None);

					return;
				}

				byte[] objHeaderMem   = TryGetObjHeaderAsBytes();
				var    methodTablePtr = TryGetMethodTablePointer();


				// ObjHeader
				Table.AddRow(-IntPtr.Size, Hex.ToHex(m_pAddr - IntPtr.Size), IntPtr.Size, OBJHEADER_TYPE_STR,
				             OBJHEADER_NAME_STR,
				             objHeaderMem == null
					             ? Omitted.ToString()
					             : String.Format("[{0}]", Collections.CreateString(objHeaderMem)),
				             UniqueAttributes.None);

				// MethodTable*
				Table.AddRow(0, Hex.ToHex(m_pAddr), IntPtr.Size, METHODTABLE_TYPE_STR, METHODTABLE_NAME_STR,
				             methodTablePtr == IntPtr.Zero ? Omitted.ToString() : Hex.ToHex(methodTablePtr),
				             UniqueAttributes.None);
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

				var addr = m_pAddr + charOffset + i * sizeof(char) + baseOfs;
				Table.AddRow(ofsStr, Hex.ToHex(addr), sizeof(char), typeof(char).Name, $"(Character {i + 2})",
				             Read<char>(addr), UniqueAttributes.None);
			}
		}

		private static UniqueAttributes FindUniqueAttributes(Pointer<FieldDesc> fd)
		{
			if (fd.Reference.IsFixedBuffer) return UniqueAttributes.FixedBuffer;

			if (fd.Reference.IsAutoProperty) return UniqueAttributes.AutoProperty;

			return UniqueAttributes.None;
		}


		private void ArrayCreate(int rgLen)
		{
			Pointer<T>   rgPtr           = m_pAddr;
			int          baseOfs         = 0;
			const string arrayLengthStr  = "Array length";
			const int    arrayDataOffset = sizeof(int) * 2;

			if (!m_bFieldsOnly) {
				CreateInternalReferenceTypeInfo();
				baseOfs += IntPtr.Size;
			}

			rgPtr.Add(sizeof(MethodTable*));


			// Array length int
			Table.AddRow(baseOfs, Hex.ToHex(rgPtr.Address), sizeof(int), typeof(int).Name, arrayLengthStr,
			             rgPtr.Reference, UniqueAttributes.None);
			rgPtr.Add(sizeof(int));
			baseOfs += sizeof(int);

			// Padding int
			Table.AddRow(baseOfs, Hex.ToHex(rgPtr.Address), sizeof(int), typeof(int).Name, PADDING_STR, rgPtr.Reference,
			             UniqueAttributes.None);
			rgPtr.Add(sizeof(int));
			baseOfs += sizeof(int);


			for (int i = 0; i < rgLen; i++) {
				int rightOfs = baseOfs + i * rgPtr.ElementSize + arrayDataOffset;
				int leftOfs  = rightOfs + (rgPtr.ElementSize - 1);

				string ofsStr = GetOffsetString(baseOfs, rightOfs, leftOfs);


				Table.AddRow(ofsStr, Hex.ToHex(rgPtr.Address), rgPtr.ElementSize,
				             typeof(T).Name, $"(Element {i})", rgPtr.Reference,
				             UniqueAttributes.None);


				rgPtr++;
			}
		}

		private void Create(ref T t)
		{
			const string paddingByte = "Byte";

			Pointer<FieldDesc>[] fieldDescs = typeof(T).GetFieldDescs();
			fieldDescs = fieldDescs.OrderBy(x => x.Reference.Offset).ToArray();
			Arrays.RemoveAll(ref fieldDescs, x => x.Reference.IsStatic);


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

				if (m_bIsDefault)
					Table.AddRow(ofsStr, Omitted, v.Reference.Size,
					             v.Reference.Info.FieldType.Name, v.Reference.Name, Omitted, FindUniqueAttributes(v));
				else
					Table.AddRow(ofsStr, Hex.ToHex(v.Reference.GetAddress(ref t)), v.Reference.Size,
					             v.Reference.Info.FieldType.Name, v.Reference.Name, v.Reference.GetValue(t),
					             FindUniqueAttributes(v));


				// start padding
				int nextOffsetOrSize                             = Unsafe.BaseFieldsSize<T>();
				if (i != fieldDescs.Length - 1) nextOffsetOrSize = fieldDescs[i + 1].Reference.Offset;

				int nextSectOfsCandidate = fieldDescs[i].Reference.Offset + fieldDescs[i].Reference.Size;

				if (nextSectOfsCandidate < nextOffsetOrSize) {
					int padSize   = nextOffsetOrSize - nextSectOfsCandidate;
					int ro        = baseOfs + nextSectOfsCandidate;
					int lo        = ro + (padSize - 1);
					int padOffset = nextSectOfsCandidate + baseOfs;


					if (m_bIsDefault)
						Table.AddRow(GetOffsetString(baseOfs, ro, lo), Omitted, padSize,
						             paddingByte, PADDING_STR, 0, UniqueAttributes.Padding);
					else
						Table.AddRow(GetOffsetString(baseOfs, ro, lo), Hex.ToHex(m_pAddr + padOffset), padSize,
						             paddingByte, PADDING_STR, 0, UniqueAttributes.Padding);
				}

				// end padding
			}

			if (m_bIsDefault) Table.RemoveColumn(1).RemoveColumn(4);
		}


		public override string ToString()
		{
			return Table.ToMarkDownString();
		}

		private enum UniqueAttributes
		{
			None,
			FixedBuffer,
			AutoProperty,
			Padding
		}

		// todo: add option to inline value types

		#region Fields

		private readonly IntPtr m_pAddr;
		private readonly T      m_value;
		private const    char   Omitted = '-';
		private readonly bool   m_bFieldsOnly;
		private readonly bool   m_bIsArray;

		private const string OBJHEADER_TYPE_STR   = "ObjHeader";
		private const string METHODTABLE_TYPE_STR = "MethodTable*";
		private const string OBJHEADER_NAME_STR   = "(Object header)";
		private const string METHODTABLE_NAME_STR = "(MethodTable ptr)";
		private const string PADDING_STR          = "(padding)";

		/// <summary>
		///     Whether to include the full byte range of offsets (not included for MethodTable* or Object Header).
		///     todo: Disabled for now.
		/// </summary>
		private const bool m_bFullOffset = false;


		/// <summary>
		///     If a value of type <typeparamref name="T" /> wasn't supplied in the constructor, we pass a value of <c>default</c>
		///     of type
		///     <typeparamref name="T" /> to <see cref="Create" />. Omit addresses and values in <see cref="Table" />.
		/// </summary>
		private readonly bool m_bIsDefault;

		public ConsoleTable Table { get; }

		#endregion

		#region Constructors

		// Base constructor
		private ObjectLayout(IntPtr pAddr, T value, bool bFieldsOnly, bool bIsArray, bool bIsDefault)
		{
			m_pAddr       = pAddr;
			m_value       = value;
			m_bFieldsOnly = bFieldsOnly;
			m_bIsArray    = bIsArray;
			m_bIsDefault  = bIsDefault;

			if (!typeof(T).IsValueType && !m_bIsDefault) m_pAddr = *(IntPtr*) pAddr;

			// If we're only displaying fields, we'll display the offset relative to the first field
			Table = new ConsoleTable(bFieldsOnly ? "Field Offset" : "Memory Offset", "Address", "Size", "Type", "Name",
			                         "Value", "Unique attributes");
		}

		/// <summary>
		///     Creates the layout of an object in its default state.
		/// </summary>
		/// <param name="bFieldsOnly">
		///     When <c>false</c>, internal metadata such as the <see cref="MethodTable" />
		///     pointer is also included.
		/// </param>
		public ObjectLayout(bool bFieldsOnly = true) : this(IntPtr.Zero, default, bFieldsOnly, false, true)
		{
			Conditions.Assert(!typeof(T).IsArray, "You cannot get the layout of an array (yet)");

//			m_bFullOffset = bFullOffset;

			T def = default;
			Create(ref def);
		}

		/// <summary>
		///     Creates the layout of an array.
		/// </summary>
		/// <param name="t">Array of type <typeparamref name="T" /></param>
		/// <param name="bFieldsOnly">
		///     When <c>false</c>, internal metadata such as the <see cref="MethodTable" />
		///     pointer is also included.
		/// </param>
		public ObjectLayout(ref T[] t, bool bFieldsOnly = true) : this(Unsafe.AddressOfHeap(ref t).Address, default,
		                                                               bFieldsOnly, true, false)
		{
//			m_bFullOffset = bFullOffset;

			ArrayCreate(t.Length);
		}

		/// <summary>
		///     Creates the layout of an object of type <typeparamref name="T" /> with the supplied value <paramref name="t" />.
		/// </summary>
		/// <param name="t">Value of type <typeparamref name="T" /></param>
		/// <param name="bFieldsOnly">
		///     When <c>false</c>, internal metadata such as the <see cref="MethodTable" />
		///     pointer is also included.
		/// </param>
		public ObjectLayout(ref T t, bool bFieldsOnly = true) : this(Unsafe.AddressOf(ref t).Address, t, bFieldsOnly,
		                                                             false, false)
		{
			Conditions.Assert(!typeof(T).IsArray, "You cannot get the layout of an array (yet)");

//			m_bFullOffset = bFullOffset;

			Create(ref t);

			// Write the remaining chars of the string
			if (typeof(T) == typeof(string)) StringCreate();
		}

		#endregion
	}
}