#region

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Exceptions;
using static RazorSharp.Memory.Mem;

// ReSharper disable MemberCanBeMadeStatic.Local

// ReSharper disable MemberCanBeMadeStatic.Global

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.CLR.Structures
{

	#region

	#endregion


	/// <summary>
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
	///         Do not dereference.
	///     </remarks>
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct FieldDesc
	{
		static FieldDesc()
		{
			SignatureCall.DynamicBind<FieldDesc>();
		}

		private const int FieldOffsetMax    = (1 << 27) - 1;
		private const int FieldOffsetNewEnC = FieldOffsetMax - 4;

		#region Fields

		[FieldOffset(0)] private readonly MethodTable* m_pMTOfEnclosingClass;

		// unsigned m_mb                  	: 24;
		// unsigned m_isStatic            	: 1;
		// unsigned m_isThreadLocal       	: 1;
		// unsigned m_isRVA               	: 1;
		// unsigned m_prot                	: 3;
		// unsigned m_requiresFullMbValue 	: 1;
		[FieldOffset(8)] private readonly uint m_dword1;

		// unsigned m_dwOffset         		: 27;
		// unsigned m_type             		: 5;
		[FieldOffset(12)] private readonly uint m_dword2;

		#endregion

		#region Accessors

		/// <summary>
		///     Unprocessed <see cref="Token" />
		/// </summary>
		private int MB => (int) (m_dword1 & 0xFFFFFF);

		/// <summary>
		///     Field metadata token
		///     <remarks>
		///         <para>Equal to <see cref="System.Reflection.FieldInfo.MetadataToken" /></para>
		///         <para>Equal to WinDbg's <c>!DumpObj</c> <c>"Field"</c> column in hexadecimal format.</para>
		///     </remarks>
		/// </summary>
		public int Token {
			get {
				// Check if this FieldDesc is using the packed mb layout
				if (!RequiresFullMBValue) {
					return Constants.TokenFromRid(MB & (int) MbMask.PackedMbLayoutMbMask, CorTokenType.mdtFieldDef);
				}

				return Constants.TokenFromRid(MB, CorTokenType.mdtFieldDef);
			}
		}

		/// <summary>
		///     Offset in memory
		///     <remarks>
		///         <para>Equal to WinDbg's <c>!DumpObj</c> <c>"Offset"</c> column in hexadecimal format.</para>
		///     </remarks>
		/// </summary>
		public int Offset => (int) (m_dword2 & 0x7FFFFFF);

		public bool IsPublic            => Protection.HasFlag(ProtectionLevel.Public);
		public bool IsPrivate           => Protection.HasFlag(ProtectionLevel.Private);
		public bool IsInternal          => Protection.HasFlag(ProtectionLevel.Internal);
		public bool IsPrivateProtected  => Protection.HasFlag(ProtectionLevel.PrivateProtected);
		public bool IsProtectedInternal => Protection.HasFlag(ProtectionLevel.ProtectedInternal);

		private int TypeInt       => (int) ((m_dword2 >> 27) & 0x7FFFFFF);
		private int ProtectionInt => (int) ((m_dword1 >> 26) & 0x3FFFFFF);


		/// <summary>
		///     Field type
		/// </summary>
		public CorElementType CorType => (CorElementType) TypeInt;

		/// <summary>
		///     Whether the field is <c>static</c>
		/// </summary>
		public bool IsStatic => ReadBit(m_dword1, 24);

		/// <summary>
		///     Whether the field is decorated with a <see cref="ThreadStaticAttribute" /> attribute
		/// </summary>
		public bool IsThreadLocal => ReadBit(m_dword1, 25);

		/// <summary>
		///     Unknown (Relative Virtual Address) ?
		/// </summary>
		public bool IsRVA => ReadBit(m_dword1, 26);


		/// <summary>
		///     Access level of the field
		/// </summary>
		public ProtectionLevel Protection => (ProtectionLevel) ProtectionInt;

		/// <summary>
		///     <para>Size of the field</para>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		public int Size {
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
			[CLRSigcall] get => throw new SigcallException();
		}

		/// <summary>
		///     The corresponding <see cref="FieldInfo" /> of this <see cref="FieldDesc" />
		/// </summary>
		public FieldInfo Info => EnclosingType.Module.ResolveField(Token);

		/// <summary>
		///     Name of this field
		/// </summary>
		public string Name => Info.Name;


		public bool IsFixedBuffer => SpecialNames.TypeNameOfFixedBuffer(Name) == Info.FieldType.Name;

		public bool IsAutoProperty {
			get {
				string demangled = SpecialNames.DemangledAutoPropertyName(Name);
				if (demangled != null) {
					return SpecialNames.NameOfAutoPropertyBackingField(demangled) == Name;
				}

				return false;
			}
		}

		/// <summary>
		///     Enclosing type of this <see cref="FieldDesc" />
		/// </summary>
		public Type EnclosingType => Runtime.MethodTableToType(EnclosingMethodTable);

		/// <summary>
		///     <see cref="MethodTable" /> of this field's type
		/// </summary>
		public Pointer<MethodTable> TypeMethodTable => Runtime.MethodTableOf(Info.FieldType);


		/// <summary>
		///     The enclosing type's <see cref="EnclosingMethodTable" />
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		public Pointer<MethodTable> EnclosingMethodTable {
			[CLRSigcall] get => throw new SigcallException();
		}


		private bool RequiresFullMBValue => ReadBit(m_dword1, 31);

		#endregion

		#region Methods

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		[CLRSigcall]
		internal void* GetModule()
		{
			throw new SigcallException();
		}

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		[CLRSigcall]
		internal void* GetStubFieldInfo()
		{
			// RuntimeFieldInfoStub
			// ReflectFieldObject
			throw new SigcallException();
		}

		#region Value

		public object GetValue<TInstance>(TInstance t)
		{
			return Info.GetValue(t);
		}


		public void SetValue<TInstance>(TInstance t, object value)
		{
			Info.SetValue(t, value);
		}

		#endregion


		/// <summary>
		///     Returns the address of the field in the specified type.
		///     <remarks>
		///         <para>Sources: /src/vm/field.cpp: 516, 489, 467</para>
		///     </remarks>
		///     <exception cref="FieldDescException">If the field is <c>static</c> </exception>
		/// </summary>
		public IntPtr GetAddress<TInstance>(ref TInstance t)
		{
			RazorContract.Requires<FieldDescException>(!IsStatic, "You cannot get the address of a static field (yet)");
			RazorContract.Assert(Runtime.ReadMethodTable(ref t) == EnclosingMethodTable);
			RazorContract.Assert(Offset != FieldOffsetNewEnC);


			IntPtr data = Unsafe.AddressOf(ref t);
			if (typeof(TInstance).IsValueType) {
				return data + Offset;
			}

			data =  Marshal.ReadIntPtr(data);
			data += IntPtr.Size + Offset;

			return data;
		}

		#endregion


		//https://github.com/dotnet/coreclr/blob/7b169b9a7ed2e0e1eeb668e9f1c2a049ec34ca66/src/inc/corhdr.h#L1512

		public override string ToString()
		{
			ConsoleTable table = new ConsoleTable("Field", "Value");

			// !NOTE NOTE NOTE!
			// this->ToString() must be used to view this

			table.AddRow("Name", Name);
			table.AddRow("Enclosing MethodTable", Hex.ToHex(EnclosingMethodTable.ToPointer()));
			table.AddRow("Enclosing type", EnclosingType.Name);


			// Unsigned 1
			table.AddRow("MB", MB);
			table.AddRow("MemberDef", Token);


			table.AddRow("Offset", Offset);
			table.AddRow("CorType", CorType);
			table.AddRow("Size", Size);

			table.AddRow("Static", IsStatic);
			table.AddRow("ThreadLocal", IsThreadLocal);
			table.AddRow("RVA", IsRVA);

			table.AddRow("Protection", Protection);
			table.AddRow("Requires full MB value", RequiresFullMBValue);

			table.AddRow("Attributes", Info.Attributes);

			return table.ToMarkDownString();
		}
	}


	internal enum MbMask
	{
		PackedMbLayoutMbMask       = 0x01FFFF,
		PackedMbLayoutNameHashMask = 0xFE0000
	}

}