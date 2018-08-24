#region

using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using RazorCommon;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Exceptions;
using static System.Type;
using static RazorSharp.Memory.Memory;

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

			var t = System.Type.GetType("System.IRuntimeFieldInfo", true);
			FieldHandleConstructor = typeof(RuntimeFieldHandle).GetConstructor(
				BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null,
				new[] {t}, null);
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
		/// Field token
		/// </summary>
		public int MemberDef {
			get {
				if (RequiresFullMBValue) {
					return Constants.TokenFromRid(MB & (int) MbMask.PackedMbLayoutMbMask, CorTokenType.mdtFieldDef);
				}

				return Constants.TokenFromRid(MB, CorTokenType.mdtFieldDef);
			}
		}

		[Sigcall("clr.dll", "48 83 EC 28 E8 37 08 C1 FF 48 8B C8 48 83 C4 28 E9 47 EB BF FF CC 90 90")]
		public void* GetModule()
		{
			throw new NotTranspiledException();
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

				if (s == -1) {
					fixed (FieldDesc* __this = &this) {
						RazorContract.RequiresFieldDescAddress((IntPtr) __this);
						return CLRFunctions.FieldDescFunctions.LoadSize(__this);
					}
				}

				return s;
			}
		}

		public Type RuntimeType => CLRFunctions.JIT_GetRuntimeType(MethodTableOfEnclosingClass);


		// ReflectFieldObject
		[Sigcall("clr.dll",
			"48 89 5C 24 10 57 48 83 EC 60 48 8B 05 07 0F 84 00 33 DB 48 8B F9 48 8B 80 78 03 00 00 48 85 C0 0F 84 72 4C 08 00")]
		public RuntimeFieldInfoStub GetStubFieldInfo()
		{
			// RuntimeFieldInfoStub
			throw new NotTranspiledException();
		}

		private static readonly ConstructorInfo FieldHandleConstructor;

		public RuntimeFieldHandle getFieldHandle()
		{
			return (RuntimeFieldHandle) FieldHandleConstructor.Invoke(new object[] {GetStubFieldInfo()});


			/*return (RuntimeFieldHandle) Activator.CreateInstance(typeof(RuntimeFieldHandle),
				BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null,
				new object[] {GetStubFieldInfo()}, CultureInfo.CurrentCulture);*/
		}


		public FieldInfo getFieldInfo()
		{
			return FieldInfo.GetFieldFromHandle(getFieldHandle());
		}

		// This type is used to remove the expense of having a managed reference object that is dynamically
		// created when we can prove that we don't need that object. Use of this type requires code to ensure
		// that the underlying native resource is not freed.
		// Cases in which this may be used:
		//  1. When native code calls managed code passing one of these as a parameter
		//  2. When managed code acquires one of these from an RtFieldInfo, and ensure that the RtFieldInfo is preserved
		//     across the lifetime of the RuntimeFieldHandleInternal instance
		//  3. When another object is used to keep the RuntimeFieldHandleInternal alive.
		// When in doubt, do not use.
		public struct RuntimeFieldHandleInternal
		{
			internal static RuntimeFieldHandleInternal EmptyHandle => new RuntimeFieldHandleInternal();

			internal bool IsNullHandle()
			{
				return m_handle != IntPtr.Zero;
			}

			internal IntPtr Value => m_handle;


			internal RuntimeFieldHandleInternal(IntPtr value)
			{
				m_handle = value;
			}

			internal IntPtr m_handle;
		}

		private interface IRuntimeFieldInfo
		{
			RuntimeFieldHandleInternal Value { get; }
		}

		[StructLayout(LayoutKind.Sequential)]
		public class RuntimeFieldInfoStub : IRuntimeFieldInfo
		{
//			[SecuritySafeCritical]
			public RuntimeFieldInfoStub(IntPtr methodHandleValue, object keepalive)
			{
				m_keepalive   = keepalive;
				m_fieldHandle = new RuntimeFieldHandleInternal(methodHandleValue);
			}

			// These unused variables are used to ensure that this class has the same layout as RuntimeFieldInfo
#pragma warning disable 169
			object         m_keepalive;
			private object m_c;
			object         m_d;
			int            m_b;
			object         m_e;
#if FEATURE_REMOTING
        object m_f;
#endif
			RuntimeFieldHandleInternal m_fieldHandle;
#pragma warning restore 169


			public RuntimeFieldHandleInternal Value => m_fieldHandle;
		}


		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		public FieldInfo FieldInfo {
			get {
				IntPtr __this = Unsafe.AddressOf(ref this);
				RazorContract.RequiresFieldDescAddress(__this);
				return Runtime.FieldAddrMap[__this];
			}
		}

		public string Name => FieldInfo.Name;

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		public MethodTable* MethodTableOfEnclosingClass {
			get {
				IntPtr __this = Unsafe.AddressOf(ref this);
				RazorContract.RequiresFieldDescAddress(__this);
				return (MethodTable*) PointerUtils.Add(__this.ToPointer(), m_pMTOfEnclosingClass);
			}
		}


		public bool RequiresFullMBValue => ReadBit(m_dword1, 31);

		#endregion

		#region Value

		public object GetValue<TInstance>(TInstance t)
		{
			return FieldInfo.GetValue(t);
		}

		public object GetValue()
		{
			return FieldInfo.GetValue(null);
		}

		public void SetValue(object value)
		{
			FieldInfo.SetValue(null, value);
		}

		public void SetValue<TInstance>(TInstance t, object value)
		{
			FieldInfo.SetValue(t, value);
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
			table.AddRow("Enclosing MethodTable", Hex.ToHex(MethodTableOfEnclosingClass));


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

			table.AddRow("Attributes", FieldInfo.Attributes);

			return table.ToMarkDownString();
		}
	}


	internal enum MbMask
	{
		PackedMbLayoutMbMask       = 0x01FFFF,
		PackedMbLayoutNameHashMask = 0xFE0000
	}

}