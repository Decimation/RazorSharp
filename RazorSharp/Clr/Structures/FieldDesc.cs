#region

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Utilities;
using RazorSharp.Clr.Meta;
using RazorSharp.Memory;
using RazorSharp.Memory.Attributes;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Exceptions;
using static RazorSharp.Clr.Offsets;

// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBeMadeStatic.Global

#endregion

namespace RazorSharp.Clr.Structures
{
	/// <summary>
	///     <para>
	///         CLR <see cref="FieldDesc" />. Functionality is implemented in this <c>struct</c> and exposed via
	///         <see cref="MetaField" />
	///     </para>
	///     <para>Internal representation: <see cref="RuntimeFieldHandle.Value" /></para>
	///     <para>Corresponding files:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/field.h</description>
	///         </item>
	///         <item>
	///             <description>/src/vm/field.cpp</description>
	///         </item>
	///     </list>
	///     <para>Lines of interest:</para>
	///     <list type="bullet">
	///         <item>
	///             <description>/src/vm/field.h: 43</description>
	///         </item>
	///     </list>
	///     <remarks>
	///         This should only be accessed via <see cref="Pointer{T}" />
	///     </remarks>
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct FieldDesc
	{
		static FieldDesc()
		{
			SignatureCall.DynamicBind<FieldDesc>();
		}

		private const int FieldOffsetMax    = (1 << 27) - 1;
		private const int FieldOffsetNewEnC = FieldOffsetMax - 4;


		#region Fields

		[FieldOffset(0)]
		private readonly MethodTable* m_pMTOfEnclosingClass;


		// unsigned m_mb                  	: 24;
		// unsigned m_isStatic            	: 1;
		// unsigned m_isThreadLocal       	: 1;
		// unsigned m_isRVA               	: 1;
		// unsigned m_prot                	: 3;
		// unsigned m_requiresFullMbValue 	: 1;
		[FieldOffset(PTR_SIZE)]
		private readonly uint m_dword1;

		// unsigned m_dwOffset         		: 27;
		// unsigned m_type             		: 5;
		[FieldOffset(PTR_SIZE + sizeof(uint))]
		private /*readonly*/ uint m_dword2;

		#endregion

		#region Accessors

		/// <summary>
		///     Unprocessed <see cref="Token" />
		///     <remarks>
		///         Original name: MB
		///     </remarks>
		/// </summary>
		private int OrigToken => (int) (m_dword1 & 0xFFFFFF);

		internal int Token {
			get {
				// Check if this FieldDesc is using the packed mb layout
				if (!RequiresFullMBValue)
					return Constants.TokenFromRid(OrigToken & (int) MbMask.PackedMbLayoutMbMask,
					                              CorTokenType.mdtFieldDef);

				return Constants.TokenFromRid(OrigToken, CorTokenType.mdtFieldDef);
			}
		}


		internal int Offset {
			get { return (int) (m_dword2 & 0x7FFFFFF); }
			set { m_dword2 = (uint)Bits.WriteTo((int)m_dword2, 0, Constants.FIELDDESC_DW2_OFFSET_BITS, value); }
		}

		private int TypeInt       => (int) ((m_dword2 >> 27) & 0x7FFFFFF);
		private int ProtectionInt => (int) ((m_dword1 >> 26) & 0x3FFFFFF);

		/// <summary>
		///     Field type
		/// </summary>
		internal CorElementType CorType => (CorElementType) TypeInt;

		#region bool accessors

		#region Access modifiers

		internal bool IsPublic            => Protection.HasFlag(ProtectionLevel.Public);
		internal bool IsPrivate           => Protection.HasFlag(ProtectionLevel.Private);
		internal bool IsInternal          => Protection.HasFlag(ProtectionLevel.Internal);
		internal bool IsPrivateProtected  => Protection.HasFlag(ProtectionLevel.PrivateProtected);
		internal bool IsProtectedInternal => Protection.HasFlag(ProtectionLevel.ProtectedInternal);

		#endregion

		internal bool IsPointer => CorType == CorElementType.Ptr;


		internal bool IsStatic => Mem.ReadBit(m_dword1, 24);


		internal bool IsThreadLocal => Mem.ReadBit(m_dword1, 25);

		/// <summary>
		///     Unknown (Relative Virtual Address) ?
		/// </summary>
		internal bool IsRVA => Mem.ReadBit(m_dword1, 26);

		internal bool IsFixedBuffer => Identifiers.TypeNameOfFixedBuffer(Name) == Info.FieldType.Name;

		internal bool IsAutoProperty {
			get {
				string demangled = Identifiers.DemangledAutoPropertyName(Name);
				if (demangled != null) return Identifiers.NameOfAutoPropertyBackingField(demangled) == Name;

				return false;
			}
		}

		private bool RequiresFullMBValue => Mem.ReadBit(m_dword1, 31);

		#endregion

		internal ProtectionLevel Protection => (ProtectionLevel) ProtectionInt;

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal int Size {
			get {
				int s = Constants.SizeOfCorElementType(CorType);
				return s == -1 ? LoadSize : s;
			}
		}

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		private int LoadSize {
			[ClrSymcall(Symbol = "FieldDesc::LoadSize", FullyQualified = true)]
			get => throw new SigcallException(nameof(LoadSize));
		}

		internal FieldInfo Info => EnclosingType.Module.ResolveField(Token);

		/// <summary>
		///     Name of this field
		/// </summary>
		internal string Name => Info.Name;

		/// <summary>
		///     Enclosing type of this <see cref="FieldDesc" />
		/// </summary>
		internal Type EnclosingType => Runtime.MethodTableToType(EnclosingMethodTable);

		/// <summary>
		///     <see cref="MethodTable" /> of this field's type
		/// </summary>
		internal Pointer<MethodTable> FieldMethodTable => Info.FieldType.GetMethodTable();

		internal Type FieldType => Info.FieldType;

		/// <summary>
		///     The enclosing type's <see cref="MethodTable" />
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal Pointer<MethodTable> EnclosingMethodTable {
			[ClrSymcall(Symbol = "FieldDesc::GetApproxEnclosingMethodTable", FullyQualified = true)]
			get => throw new SigcallException();
		}

		#endregion

		#region Methods

		#region Value

		internal object GetValue<TInstance>(TInstance t)
		{
			return Info.GetValue(t);
		}

		internal void SetValue<TInstance>(TInstance t, object value)
		{
			Info.SetValue(t, value);
		}

		#endregion


		/// <summary>
		///     Returns the address of the field in the specified type.
		///     <remarks>
		///         <para>Sources: /src/vm/field.cpp: 516, 489, 467</para>
		///     </remarks>
		///     <exception cref="Exception">If the field is <c>static</c> </exception>
		/// </summary>
		internal IntPtr GetAddress<TInstance>(ref TInstance t)
		{
			Conditions.Assert(!IsStatic, "You cannot get the address of a static field (yet)");
			Conditions.Assert(Runtime.ReadMethodTable(ref t) == EnclosingMethodTable);
			Conditions.Assert(Offset != FieldOffsetNewEnC);


			var data = Unsafe.AddressOf(ref t).Address;
			if (typeof(TInstance).IsValueType) return data + Offset;

			data =  Marshal.ReadIntPtr(data);
			data += IntPtr.Size + Offset;

			return data;
		}

		//https://github.com/dotnet/coreclr/blob/7b169b9a7ed2e0e1eeb668e9f1c2a049ec34ca66/src/inc/corhdr.h#L1512

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");

			// !NOTE NOTE NOTE!
			// this->ToString() must be used to view this

			table.AddRow("Name", Name);
			table.AddRow("Enclosing MethodTable", Hex.ToHex(EnclosingMethodTable.ToPointer()));
			table.AddRow("Enclosing type", EnclosingType.Name);


			// Unsigned 1
//			table.AddRow("MB", MB);
			table.AddRow("Token", Token);


			table.AddRow("Offset", Offset);
			table.AddRow("CorType", CorType);
			table.AddRow("Size", Size);

			table.AddRow("Static", IsStatic.Prettify());
			table.AddRow("ThreadLocal", IsThreadLocal.Prettify());
			table.AddRow("RVA", IsRVA.Prettify());

			table.AddRow("Protection", Protection);
			table.AddRow("Requires full MB value", RequiresFullMBValue);

			table.AddRow("Attributes", Info.Attributes);

			return table.ToMarkDownString();
		}

		#endregion
	}
}