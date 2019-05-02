#region

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Diagnostics;
using RazorCommon.Extensions;
using RazorCommon.Strings;
using RazorCommon.Utilities;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures.ILMethods;
using RazorSharp.Memory;
using RazorSharp.Memory.Extern;
using RazorSharp.Memory.Extern.Symbols;
using RazorSharp.Memory.Extern.Symbols.Attributes;
using RazorSharp.Memory.Pointers;
using Unsafe = RazorSharp.Memory.Unsafe;

// ReSharper disable NonReadonlyMemberInGetHashCode

// ReSharper disable FieldCanBeMadeReadOnly.Local

// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable MemberCanBeMadeStatic.Global

#endregion


namespace RazorSharp.CoreClr.Structures
{
	#region

	#endregion

	// todo: complete

	/// <summary>
	///     <para>
	///         CLR <see cref="MethodDesc" />. Functionality is implemented in this <c>struct</c> and exposed via
	///         <see cref="MetaMethod" />
	///     </para>
	///     <para>Internal representation: <see cref="RuntimeMethodHandle.Value" /></para>
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
	///             <description>/src/vm/method.hpp: 1683</description>
	///         </item>
	///     </list>
	///     <remarks>
	///         This should only be accessed via <see cref="Pointer{T}" />
	///     </remarks>
	/// </summary>
	[ClrSymNamespace]
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct MethodDesc
	{
		private const int ALIGNMENT_SHIFT = 3;
		private const int ALIGNMENT       = 1 << ALIGNMENT_SHIFT;
		private const int ALIGNMENT_MASK  = ALIGNMENT - 1;

		static MethodDesc()
		{
			Symload.Load(typeof(MethodDesc));
		}

		#region Fields

		private ushort m_wFlags3AndTokenRemainder;

		private byte m_bFlags2;

		private ushort m_wSlotNumber;

		private ushort m_wFlags;

		/// <summary>
		///     Valid only if the function is non-virtual and
		///     non-abstract (<see cref="SizeOf" /> <c>== 16</c>)
		/// </summary>
		private void* m_pFunction;

		#endregion

		#region Accessors

		/// <summary>
		///     The enclosing type of this <see cref="MethodDesc" />
		/// </summary>
		internal Type EnclosingType => EnclosingMethodTable.Reference.RuntimeType;

		internal MethodInfo Info => (MethodInfo) EnclosingType.Module.ResolveMethod(Token);

		internal IntPtr Function {
			get => Info.MethodHandle.GetFunctionPointer();
			set => SetStableEntryPoint(value);
		}


		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		/// <exception cref="SymImportException"></exception>
		internal IntPtr NativeCode {
			[Symcall(Symbol = "GetNativeCode")]
			get => throw new SymImportException();
		}


		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal IntPtr PreImplementedCode {
			[Symcall(Symbol = "GetPreImplementedCode")]
			get => throw new SymImportException();
		}

		/// <summary>
		///     Name of this method
		/// </summary>
		internal string Name => Info.Name;

		internal byte ChunkIndex { get; }

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal Pointer<MethodDescChunk> MethodDescChunk {
			get {
				// return
				//PTR_MethodDescChunk(dac_cast<TADDR>(this) -
				//                    (sizeof(MethodDescChunk) + (GetMethodDescIndex() * MethodDesc::ALIGNMENT)));
				Pointer<MethodDescChunk> __this = Unsafe.AddressOf(ref this).Address;
				__this.Subtract(sizeof(MethodDescChunk) + ChunkIndex * ALIGNMENT);
				return __this;
			}
		}


		/// <summary>
		///     Size of the current <see cref="MethodDesc" />
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal int SizeOf {
			[Symcall(Symbol = "SizeOf")]
			get => throw new SymImportException();
		}

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal Pointer<MethodTable> EnclosingMethodTable {
			[Symcall(Symbol = "GetMethodTable")]
			get => throw new SymImportException();
		}


		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal uint RVA {
			[Symcall(Symbol = "GetRVA")]
			get => throw new SymImportException();
		}


		internal int Token {
			[Symcall(Symbol = "GetMemberDef")]
			get => throw new SymImportException();
		}

		#region bool accessors

		/// <summary>
		///     <para>Whether this method is a constructor</para>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal bool IsConstructor {
			[Symcall(Symbol = "IsCtor")]
			get => throw new SymImportException();
		}

		internal bool IsPreImplemented => PreImplementedCode != IntPtr.Zero;

		/// <summary>
		///     <para>Whether this method is pointing to native code</para>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal bool IsPointingToNativeCode {
			[Symcall(Symbol = "IsPointingToNativeCode")]
			get => throw new SymImportException();
		}

		internal bool HasThis => Info.CallingConvention.HasFlag(CallingConventions.HasThis);

		internal bool HasILHeader => IsIL && !IsUnboxingStub && RVA > 0;

		internal bool IsUnboxingStub => (Flags2 & MethodDescFlags2.IsUnboxingStub) != 0;

		internal bool IsIL => MethodClassification.IL == Classification ||
		                      MethodClassification.Instantiated == Classification;

		internal bool IsStatic => Info.IsStatic;

		#endregion

		#region Flags

		internal MethodClassification Classification =>
			(MethodClassification) (m_wFlags & (ushort) MethodDescClassification.Classification);

		internal MethodAttributes         Attributes => Info.Attributes;
		internal MethodDescClassification Flags      => (MethodDescClassification) m_wFlags;
		internal MethodDescFlags2         Flags2     => (MethodDescFlags2) m_bFlags2;
		internal MethodDescFlags3         Flags3     => (MethodDescFlags3) m_wFlags3AndTokenRemainder;

		#endregion

		#endregion


		#region Methods

		#region Equality

		public bool Equals(MethodDesc md)
		{
			bool a = m_wFlags3AndTokenRemainder == md.m_wFlags3AndTokenRemainder;
			bool b = ChunkIndex == md.ChunkIndex;
			bool c = m_bFlags2 == md.m_bFlags2;
			bool d = m_wSlotNumber == md.m_wSlotNumber;
			bool e = m_wFlags == md.m_wFlags;

			return a && b && c && d && e;
		}

		public override bool Equals(object obj)
		{
			if (obj != null && obj.GetType() == GetType()) {
				var md = (MethodDesc) obj;
				return md.Equals(this);
			}

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			unchecked {
				int hashCode = m_wFlags3AndTokenRemainder.GetHashCode();
				hashCode = (hashCode * 397) ^ ChunkIndex.GetHashCode();
				hashCode = (hashCode * 397) ^ m_bFlags2.GetHashCode();
				hashCode = (hashCode * 397) ^ m_wSlotNumber.GetHashCode();
				hashCode = (hashCode * 397) ^ m_wFlags.GetHashCode();
				return hashCode;
			}
		}

		#endregion


		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		[Symcall]
		internal Pointer<ILMethod> GetILHeader(int fAllowOverrides = 0)
		{
			throw new SymImportException();
		}

		/// <summary>
		///     <remarks>Address-sensitive</remarks>
		/// </summary>
		[Symcall]
		private long SetStableEntryPointInterlocked(ulong pCode)
		{
			throw new SymImportException();
		}


		internal void SetStableEntryPoint(Pointer<byte> pCode)
		{
			Reset();
			Conditions.Ensure(SetStableEntryPointInterlocked((ulong) pCode) > 0);
		}

		/// <summary>
		///     <para>Reset the <see cref="MethodDesc" /> to its original state</para>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		[Symcall]
		internal void Reset()
		{
			throw new SymImportException();
		}


		internal TDelegate GetDelegate<TDelegate>() where TDelegate : Delegate
		{
			return Functions.GetDelegateForFunctionPointer<TDelegate>(Function);
		}


		internal void Prepare()
		{
			if (!Flags2.HasFlag(MethodDescFlags2.HasStableEntryPoint) || !Flags2.HasFlag(MethodDescFlags2.HasPrecode))
				RuntimeHelpers.PrepareMethod(Info.MethodHandle);
		}


		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");

			table.AddRow("Name", Name);
			table.AddRow("MethodTable", Hex.ToHex(EnclosingMethodTable.Address));
			table.AddRow("Token", Token);
			table.AddRow("Function", Hex.ToHex(Function));
			table.AddRow("Native code", Hex.ToHex(NativeCode));


			if (HasILHeader)
				table.AddRow("IL code", Hex.ToHex(GetILHeader().Reference.Code.Address));

			table.AddRow("Attributes", Attributes);
			table.AddRow("Classification", Classification.JoinFlags());

			table.AddRow("Flags", EnumUtil.CreateFlagsString(m_wFlags, Flags));
			table.AddRow("Flags 2", EnumUtil.CreateFlagsString(m_bFlags2, Flags2));
			table.AddRow("Flags 3", EnumUtil.CreateFlagsString(m_wFlags3AndTokenRemainder, Flags3));

			table.AddRow("SizeOf", SizeOf);

			return table.ToString();
		}

		#endregion

		/*public static void InjectJmp(Pointer<byte> addr, MethodInfo methodInfo)
		{
			var mm = new MetaMethod(methodInfo.GetMethodDesc());
			mm.PrepareOverride();
			Pointer<byte> targetAddr = mm.Function;

			// Opcode: E9 cd
			// Mnemonic: JMP rel32
			// Description: Jump near, relative, displacement relative to next instruction.
			addr.Write(0xE9);
			addr++; // Move over jmp opcode
			Pointer<byte> rel32 = targetAddr - addr;
			rel32 += sizeof(int); // Add size of rel32 arg

			addr.WriteAny(rel32.ToInt32());
			Console.WriteLine("done inject");
		}*/
	}
}