#region

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RazorSharp.CLR.Meta;
using RazorSharp.CLR.Structures.ILMethods;
using RazorCommon;
using RazorCommon.Utilities;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Exceptions;

// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable MemberCanBeMadeStatic.Global

#endregion


namespace RazorSharp.CLR.Structures
{
	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

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
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct MethodDesc
	{
		// method.hpp: 213
		private const int ALIGNMENT_SHIFT = 3;
		private const int ALIGNMENT       = 1 << ALIGNMENT_SHIFT;
		private const int ALIGNMENT_MASK  = ALIGNMENT - 1;

		static MethodDesc()
		{
			SignatureCall.DynamicBind<MethodDesc>();
		}

		#region Fields

		[FieldOffset(0)] private readonly ushort m_wFlags3AndTokenRemainder;
		[FieldOffset(2)] private readonly byte   m_chunkIndex;
		[FieldOffset(3)] private readonly byte   m_bFlags2;
		[FieldOffset(4)] private readonly ushort m_wSlotNumber;
		[FieldOffset(6)] private readonly ushort m_wFlags;

		/// <summary>
		///     Valid only if the function is non-virtual and non-abstract (<see cref="SizeOf" /> <c>== 16</c>)
		/// </summary>
		[FieldOffset(8)] private void* m_pFunction;

		#endregion

		#region Accessors

		/// <summary>
		///     The enclosing type of this <see cref="MethodDesc" />
		/// </summary>
		internal Type EnclosingType => Runtime.MethodTableToType(EnclosingMethodTable);


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
		/// <exception cref="SigcallException"></exception>
		internal IntPtr NativeCode {
			[ClrSigcall] get => throw new SigcallException();
		}


		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal IntPtr PreImplementedCode {
			[ClrSigcall] get => throw new SigcallException();
		}

		/// <summary>
		///     Name of this method
		/// </summary>
		internal string Name => Info.Name;

		internal byte ChunkIndex => m_chunkIndex;

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
			[ClrSigcall] get => throw new SigcallException();
		}

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal Pointer<MethodTable> EnclosingMethodTable {
			[ClrSigcall] get => throw new SigcallException();
		}


		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal uint RVA {
			[ClrSigcall] get => throw new SigcallException();
		}


		internal int Token {
			[ClrSigcall] get => throw new SigcallException();
		}

		#region bool accessors

		/// <summary>
		///     <para>Whether this method is a constructor</para>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal bool IsConstructor {
			[ClrSigcall] get => throw new SigcallException();
		}

		internal bool IsPreImplemented => PreImplementedCode != IntPtr.Zero;

		/// <summary>
		///     <para>Whether this method is pointing to native code</para>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		internal bool IsPointingToNativeCode {
			[ClrSigcall] get => throw new SigcallException();
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

		internal void Compare(MethodInfo info)
		{
			Conditions.Assert(Token == info.MetadataToken);
			Conditions.Assert(Name == info.Name);
			Conditions.Assert(Info == info);
		}

		#region Equality

		public bool Equals(MethodDesc md)
		{
			bool a = m_wFlags3AndTokenRemainder == md.m_wFlags3AndTokenRemainder;
			bool b = m_chunkIndex == md.m_chunkIndex;
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
				hashCode = (hashCode * 397) ^ m_chunkIndex.GetHashCode();
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
		[ClrSigcall]
		internal Pointer<ILMethod> GetILHeader(int fAllowOverrides = 0)
		{
			throw new SigcallException();
		}

		/// <summary>
		///     <remarks>Address-sensitive</remarks>
		/// </summary>
		[ClrSigcall]
		private long SetStableEntryPointInterlocked(ulong pCode)
		{
			throw new SigcallException();
		}


		internal void SetStableEntryPoint(Pointer<byte> pCode)
		{
			Reset();
			long val = SetStableEntryPointInterlocked((ulong) pCode);
			Debug.Assert(val > 0);
		}

		/// <summary>
		///     <para>Reset the <see cref="MethodDesc" /> to its original state</para>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		[ClrSigcall]
		internal void Reset()
		{
			throw new SigcallException();
		}

		/// <summary>
		///     Use at your own risk!
		/// </summary>
		/// <param name="p">New function pointer</param>
		/// <exception cref="Exception">If this function is <c>virtual</c> or <c>abstract</c></exception>
		[Obsolete("Use SetStableEntryPoint", true)]
		internal void SetFunctionPointer(IntPtr p)
		{
			Conditions.Assert(
				!Attributes.HasFlag(MethodAttributes.Virtual) && !Attributes.HasFlag(MethodAttributes.Abstract),
				"Function is virtual/abstract");


			m_pFunction = p.ToPointer();
		}


		internal TDelegate GetDelegate<TDelegate>() where TDelegate : Delegate
		{
			return Marshal.GetDelegateForFunctionPointer<TDelegate>(Function);
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
			table.AddRow("Enclosing type", EnclosingType.Name);
			table.AddRow("MethodTable", Hex.ToHex(EnclosingMethodTable.Address));
			table.AddRow("Token", Token);
			table.AddRow("Signature", Info);

			table.AddRow("Function", Hex.ToHex(Function));
			table.AddRow("Non-MI Function", Hex.ToHex(m_pFunction));
			table.AddRow("Native code", Hex.ToHex(NativeCode));
			if (HasILHeader) table.AddRow("IL code", Hex.ToHex(GetILHeader().Reference.Code.Address));

//			table.AddRow("Chunk index", m_chunkIndex);
//			table.AddRow("Slot number", m_wSlotNumber);
			table.AddRow("Attributes", Attributes);

			table.AddRow("Is pointing to native code", IsPointingToNativeCode.Prettify());
			table.AddRow("Is constructor", IsConstructor.Prettify());
			table.AddRow("Has this", HasThis.Prettify());
			table.AddRow("Is IL", IsIL.Prettify());
			table.AddRow("MethodDescChunk", MethodDescChunk.ToString("P"));


			table.AddRow("Classification", Classification.Join());
			table.AddRow("Flags", EnumUtil.CreateFlagsString(m_wFlags, Flags));
			table.AddRow("Flags 2", EnumUtil.CreateFlagsString(m_bFlags2, Flags2));
			table.AddRow("Flags 3", EnumUtil.CreateFlagsString(m_wFlags3AndTokenRemainder, Flags3));
			table.AddRow("SizeOf", SizeOf);


			return table.ToMarkDownString();
		}

		#endregion
	}
}