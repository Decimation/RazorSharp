#region

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using RazorCommon;
using RazorSharp.Utilities;

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

		#region Fields

		[FieldOffset(0)] private readonly ushort m_wFlags3AndTokenRemainder;
		[FieldOffset(2)] private readonly byte   m_chunkIndex;
		[FieldOffset(3)] private readonly byte   m_bFlags2;
		[FieldOffset(4)] private readonly ushort m_wSlotNumber;
		[FieldOffset(6)] private readonly ushort m_wFlags;

		/// <summary>
		///     Valid only if the function is non-virtual and non-abstract
		/// </summary>
		[FieldOffset(8)] private void* m_functionPtr;

		#endregion

		#region Accessors

		public IntPtr Function => MethodInfo.MethodHandle.GetFunctionPointer();
		public string Name     => MethodInfo.Name;


		/// <summary>
		///     <remarks>
		///         Address-sensitive
		///     </remarks>
		/// </summary>
		public MethodInfo MethodInfo {
			get {
				IntPtr __this = Unsafe.AddressOf(ref this);
				Assertion.AssertMethodDescAddress(__this);
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
			Trace.Assert(MethodInfo.IsStatic && (Flags2.HasFlag(MethodDescFlags2.HasStableEntryPoint) ||
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

			Trace.Assert((Flags2.HasFlag(MethodDescFlags2.HasStableEntryPoint) ||
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

		public void SetFunctionPointer(IntPtr p)
		{
			if (Attributes.HasFlag(MethodAttributes.Virtual) || Attributes.HasFlag(MethodAttributes.Abstract)) {
				throw new RuntimeException("Function is virtual/abstract");
			}

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

			table.AddRow("m_functionPtr", Hex.ToHex(m_functionPtr));

			table.AddRow("Chunk index", m_chunkIndex);
			table.AddRow("Slot number", m_wSlotNumber);
			table.AddRow("Attributes", Attributes);


			table.AddRow("Classification", Classification.Join());
			table.AddRow("Flags", Runtime.CreateFlagsString(m_wFlags, Flags));
			table.AddRow("Flags 2", Runtime.CreateFlagsString(m_bFlags2, Flags2));
			table.AddRow("Flags 3", Runtime.CreateFlagsString(m_wFlags3AndTokenRemainder, Flags3));


			return table.ToMarkDownString();
		}
	}


}