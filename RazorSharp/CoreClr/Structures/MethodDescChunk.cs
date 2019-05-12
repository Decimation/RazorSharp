#region

#region

using System;
using System.Runtime.InteropServices;
using SimpleSharp;
using SimpleSharp.Strings;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;

// ReSharper disable FieldCanBeMadeReadOnly.Local

#endregion

// ReSharper disable ConvertToAutoPropertyWhenPossible

#endregion

namespace RazorSharp.CoreClr.Structures
{
	//todo: WIP, verify
	/// <summary>
	///     <para>Corresponding files:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/method.hpp</description>
	///         </item>
	///         <item>
	///             <description>/src/vm/method.cpp</description>
	///         </item>
	///         <item>
	///             <description>/src/vm/method.inl</description>
	///         </item>
	///     </list>
	///     <para>Lines of interest:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/method.hpp: 1949</description>
	///         </item>
	///     </list>
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct MethodDescChunk
	{
		#region Fields

		/// <summary>
		///     Relative fixup pointer
		/// </summary>
		private MethodTable* m_methodTable;

		/// <summary>
		///     Relative pointer
		/// </summary>
		private MethodDescChunk* m_next;

		private byte m_size;

		/// <summary>
		///     The number of <see cref="MethodDesc" />s in this chunk minus 1
		/// </summary>
		private byte m_count;

		private ushort m_flagsAndTokenRange;

		#endregion


		/// <summary>
		///     The size of this chunk minus 1 (in multiples of <see cref="MethodDesc.ALIGNMENT" />)
		/// </summary>
		internal byte Size => m_size;


		internal byte Count => (byte) (m_count + 1); //(byte) (m_count + 1);

// PTR_HOST_MEMBER_TADDR(type, host, memb)
// Retrieves the target address of a host instance pointer and
// offsets it by the given member's offset within the type.

		internal Pointer<MethodDescChunk> Next {
			get {
				// return m_next.GetValueMaybeNull(PTR_HOST_MEMBER_TADDR(MethodDescChunk, this, m_next));

				/*Pointer<MethodDescChunk> __this = Unsafe.AddressOf(ref this);
				__this.Add(IntPtr.Size);
				__this.Add((int) m_next);
				return __this;*/

				return null;
			}
		}

		internal Pointer<MethodTable> MethodTable {
			get {
				// return m_methodTable.GetValue(PTR_HOST_MEMBER_TADDR(MethodDescChunk, this, m_methodTable));

				Pointer<MethodTable> __this = Unsafe.AddressOf(ref this).Address;
				__this.Add(0);
				__this.Add((int) m_methodTable);
				return __this;
			}
			set => m_methodTable = (MethodTable*) value.ToPointer();
		}

		internal Pointer<MethodDesc> FirstMethodDesc {
			get {
				// return PTR_MethodDesc(dac_cast<TADDR>(this) + sizeof(MethodDescChunk));

				Pointer<MethodDesc> __this = Unsafe.AddressOf(ref this).Address;
				__this.Add(sizeof(MethodDescChunk));
				return __this;
			}
		}

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("MethodTable", Hex.ToHex(MethodTable.Address));

//			table.AddRow("m_methodTable", (int) m_methodTable);
			table.AddRow("Next chunk", Hex.ToHex(Next.Address));

//			table.AddRow("m_next", (int) m_next);
			table.AddRow("First MethodDesc", Hex.ToHex(FirstMethodDesc.Address));
			table.AddRow("Size", m_size);
			table.AddRow("Count", Count);
			table.AddRow("Flags and token range", m_flagsAndTokenRange);

			return table.ToString();
		}
	}
}