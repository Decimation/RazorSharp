#region

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using RazorCommon;
using RazorSharp.Memory;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Exceptions;

#endregion

namespace RazorSharp.CLR.Structures
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion

	//todo: complete

	/// <summary>
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
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct MethodDesc
	{

		// method.hpp: 213
		private const int ALIGNMENT_SHIFT = 3;
		private const int ALIGNMENT       = 1 << ALIGNMENT_SHIFT;
		private const int ALIGNMENT_MASK  = ALIGNMENT - 1;

		#region Fields

		[FieldOffset(0)] private readonly ushort m_wFlags3AndTokenRemainder;
		[FieldOffset(2)] private readonly byte   m_chunkIndex;
		[FieldOffset(3)] private readonly byte   m_bFlags2;
		[FieldOffset(4)] private readonly ushort m_wSlotNumber;
		[FieldOffset(6)] private readonly ushort m_wFlags;

		/// <summary>
		///     Valid only if the function is non-virtual and non-abstract (<see cref="SizeOf" /> <c>== 16</c>)
		/// </summary>
		[FieldOffset(8)] private void* m_functionPtr;

		#endregion

		#region Accessors

		static MethodDesc()
		{
			SignatureCall.Transpile<MethodDesc>();
		}

		public IntPtr Function => MethodInfo.MethodHandle.GetFunctionPointer();
		public string Name     => MethodInfo.Name;

		public bool IsCtor {
			[Sigcall("clr.dll",
				"48 89 5C 24 08 57 48 83 EC 20 48 8B F9 E8 4E 32 F6 FF 33 DB 0F BA E0 0C 0F 82 39 BA 0F 00")]
			get => throw new NotTranspiledException();
		}

		public bool IsPointingToNativeCode {
			[Sigcall("clr.dll",
				"48 89 5C 24 08 57 48 83 EC 20 8A 41 03 48 8B F9 A8 01 0F 85 FC C5 F6 FF 33 C0 EB 01 CC")]
			get => throw new NotTranspiledException();
		}

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		public MethodInfo MethodInfo {
			get {
				IntPtr __this = Unsafe.AddressOf(ref this);
				RazorContract.RequiresMethodDescAddress(__this);
				MethodInfo m = Runtime.MethodAddrMap[__this];
				return m;
			}
		}

		#region Flags

		public MethodClassification Classification =>
			(MethodClassification) (m_wFlags & (ushort) MethodDescClassification.mdcClassification);

		public MethodAttributes         Attributes => MethodInfo.Attributes;
		public MethodDescClassification Flags      => (MethodDescClassification) m_wFlags;
		public MethodDescFlags2         Flags2     => (MethodDescFlags2) m_bFlags2;
		public MethodDescFlags3         Flags3     => (MethodDescFlags3) m_wFlags3AndTokenRemainder;

		#endregion

		// method.hpp: 2188
		/*public MethodDescChunk* MethodDescChunk {
			get {
				fixed (MethodDesc* __this = &this) {
					long taddr = (long) __this;
					return (MethodDescChunk*) (taddr - sizeof(MethodDescChunk) + (m_chunkIndex * ALIGNMENT));
				}
			}
		}*/

		#endregion

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
				MethodDesc md = (MethodDesc) obj;
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


/*		// Note: This doesn't actually seem to be in the source code, but it matches
		// MethodHandle.GetFunctionPointer for non-virtual functions
//		[FieldOffset(8)] private readonly void*  m_function;*/


		/*public object Invoke<TDelegate>(params object[] args) where TDelegate : Delegate
		{
			TDelegate d = GetDelegate<TDelegate>();
			RazorContract.Assert(MethodInfo.IsStatic && (Flags2.HasFlag(MethodDescFlags2.HasStableEntryPoint) ||
			                                     Flags2.HasFlag(MethodDescFlags2.HasPrecode)));
			return d.DynamicInvoke(args);
		}

		/// <summary>
		/// Invokes the target method.
		/// </summary>
		/// <param name="instance">Instance to invoke the method upon. If the method is static, the parameter is
		/// ignored</param>
		/// <param name="args">Parameters for the method, if any</param>
		public object Invoke<TInstance, TDelegate>(ref TInstance instance, params object[] args)
			where TDelegate : Delegate
		{
			TDelegate d = GetDelegate<TDelegate>();

			RazorContract.Assert((Flags2.HasFlag(MethodDescFlags2.HasStableEntryPoint) ||
			              Flags2.HasFlag(MethodDescFlags2.HasPrecode)));

			if (MethodInfo.IsStatic) {
				return d.DynamicInvoke(args);
			}


			var __this = Unsafe.AddressOf(ref instance);
			if (!typeof(TInstance).IsValueType) {
				__this = Marshal.ReadIntPtr(__this);
			}

			object o = args.Length == 0 ? d.DynamicInvoke(__this) : d.DynamicInvoke(__this, args);

			if (MethodInfo.ReturnType.IsValueType) {
				return o;
			}
			else {
				IntPtr heapPtr = (IntPtr) Int64.Parse(o.ToString());

				return Misc.InvokeGenericMethod(typeof(CSUnsafe), "Read", MethodInfo.ReturnType, instance,new IntPtr(&heapPtr));

			}


			return o;
		}

		public object IntrinsicInvoke<TDelegate>(params object[] args) where TDelegate : Delegate
		{
			TDelegate d = GetDelegate<TDelegate>();
			return d.DynamicInvoke(args);
		}*/

		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		public int SizeOf {
			get {
				fixed (MethodDesc* __this = &this) {
					RazorContract.RequiresMethodDescAddress((IntPtr) __this);
					return (int) CLRFunctions.MethodDescFunctions.SizeOf(__this);
				}
			}
		}

		/// <summary>
		///     Use at your own risk!
		/// </summary>
		/// <param name="p">New function pointer</param>
		/// <exception cref="MethodDescException">If this function is <c>virtual</c> or <c>abstract</c></exception>
		public void SetFunctionPointer(IntPtr p)
		{
			RazorContract.Requires<MethodDescException>(
				!(Attributes.HasFlag(MethodAttributes.Virtual) || Attributes.HasFlag(MethodAttributes.Abstract)),
				"Function is virtual/abstract");


			m_functionPtr = p.ToPointer();
		}


		public TDelegate GetDelegate<TDelegate>() where TDelegate : Delegate
		{
			return Marshal.GetDelegateForFunctionPointer<TDelegate>(Function);
		}

		public void Prepare()
		{
			if (!Flags2.HasFlag(MethodDescFlags2.HasStableEntryPoint) || !Flags2.HasFlag(MethodDescFlags2.HasPrecode)) {
				RuntimeHelpers.PrepareMethod(MethodInfo.MethodHandle);
			}
		}

		private string CreateSignatureString()
		{
			ParameterInfo[] param = MethodInfo.GetParameters();

			if (param.Length == 0) {
				return String.Format("{0}()", Name);
			}

			StringBuilder sb = new StringBuilder();

			sb.AppendFormat("{0}(", Name);
			foreach (ParameterInfo v in param) {
				sb.AppendFormat("{0} {1}, ", v.ParameterType.Name, v.Name);
			}

			sb[sb.Length - 2] = ')';

			return sb.ToString();
		}

		public override string ToString()
		{
			ConsoleTable table = new ConsoleTable("Field", "Value");
			table.AddRow("Name", Name);
			table.AddRow("Signature", CreateSignatureString());
			table.AddRow("Function", Hex.ToHex(Function));

			table.AddRow("Non-MI FuncPtr", Hex.ToHex(m_functionPtr));

			table.AddRow("Chunk index", m_chunkIndex);
			table.AddRow("Slot number", m_wSlotNumber);
			table.AddRow("Attributes", Attributes);


			table.AddRow("Classification", Classification.Join());
			table.AddRow("Flags", Runtime.CreateFlagsString(m_wFlags, Flags));
			table.AddRow("Flags 2", Runtime.CreateFlagsString(m_bFlags2, Flags2));
			table.AddRow("Flags 3", Runtime.CreateFlagsString(m_wFlags3AndTokenRemainder, Flags3));
			table.AddRow("SizeOf", SizeOf);


			return table.ToMarkDownString();
		}
	}


}