#region

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Exceptions;
using static RazorSharp.Memory.Memory;

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
			SignatureCall.Transpile<FieldDesc>();
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
		///     MemberDef
		/// </summary>
		private int MB => (int) (m_dword1 & 0xFFFFFF);

		/// <summary>
		///     Field token
		///     <remarks>
		///         Equal to <see cref="FieldInfo.MetadataToken" />
		///     </remarks>
		/// </summary>
		public int MemberDef {
			[CLRSigcall(OffsetGuess = 0x33AC0)] get => throw new NotTranspiledException();
		}


		/// <summary>
		///     Offset in memory
		/// </summary>
		public int Offset => (int) (m_dword2 & 0x7FFFFFF);


		private int Type => (int) ((m_dword2 >> 27) & 0x7FFFFFF);

		/// <summary>
		///     Field type
		/// </summary>
		public CorElementType CorType => (CorElementType) Type;


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
		///     Access level
		/// </summary>
		private int ProtectionInt => (int) ((m_dword1 >> 26) & 0x3FFFFFF);

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


		private int LoadSize {
			[CLRSigcall(OffsetGuess = 0x102278)] get => throw new NotTranspiledException();
		}

		public FieldInfo Info        => RuntimeType.Module.ResolveField(MemberDef);
		public string    Name        => Info.Name;
		public Type      RuntimeType => CLRFunctions.JIT_GetRuntimeType(MethodTableOfEnclosingClass.ToPointer());


		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		public Pointer<MethodTable> MethodTableOfEnclosingClass {
			[CLRSigcall(OffsetGuess = 0x21214)] get => throw new NotTranspiledException();
		}


		public bool RequiresFullMBValue => ReadBit(m_dword1, 31);

		#endregion

		[CLRSigcall(OffsetGuess = 0x4109D4)]
		public void* GetModule()
		{
			throw new NotTranspiledException();
		}

		// RuntimeFieldInfoStub
		// ReflectFieldObject
		[CLRSigcall(OffsetGuess = 0x1025A0)]
		public void* GetStubFieldInfo()
		{
			throw new NotTranspiledException();
		}

		#region Value

		public object GetValue<TInstance>(TInstance t)
		{
			return Info.GetValue(t);
		}

		public object GetValue()
		{
			return Info.GetValue(null);
		}

		public void SetValue(object value)
		{
			Info.SetValue(null, value);
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
			RazorContract.Assert(Runtime.ReadMethodTable(ref t) == MethodTableOfEnclosingClass);
			RazorContract.Assert(Offset != FieldOffsetNewEnC);


			IntPtr data = Unsafe.AddressOf(ref t);
			if (typeof(TInstance).IsValueType) {
				return data + Offset;
			}

			data =  Marshal.ReadIntPtr(data);
			data += IntPtr.Size + Offset;

			return data;
		}


		//https://github.com/dotnet/coreclr/blob/7b169b9a7ed2e0e1eeb668e9f1c2a049ec34ca66/src/inc/corhdr.h#L1512

		public override string ToString()
		{
			ConsoleTable table = new ConsoleTable("Field", "Value");

			// !NOTE NOTE NOTE!
			// this->ToString() must be used to view this

			table.AddRow("Name", Name);
			table.AddRow("Enclosing MethodTable", Hex.ToHex(MethodTableOfEnclosingClass.ToPointer()));
			table.AddRow("Enclosing type", RuntimeType.Name);


			// Unsigned 1
			table.AddRow("MB", MB);
			table.AddRow("MemberDef", MemberDef);


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