#region

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using RazorCommon;

#endregion

namespace RazorSharp.Runtime.CLRTypes
{

	#region

	using DWORD = UInt32;
	using WORD = UInt16;
	using unsigned = UInt32;

	#endregion

	//todo: complete
	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/master/src/vm/method.hpp#L1683
	///
	/// Internal representation: MethodHandle.Value
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct MethodDesc
	{

		[FieldOffset(0)] private readonly UInt16 m_wFlags3AndTokenRemainder;
		[FieldOffset(2)] private readonly byte   m_chunkIndex;
		[FieldOffset(3)] private readonly byte   m_bFlags2;
		[FieldOffset(4)] private readonly WORD   m_wSlotNumber;
		[FieldOffset(6)] private readonly WORD   m_wFlags;

		public MethodInfo MethodInfo {
			get { return Runtime.MethodMap[this]; }
		}

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
			if (obj.GetType() == GetType()) {
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


/*		// Note: This doesn't actually seem to be in the source code, but it matches
		// MethodHandle.GetFunctionPointer for non-virtual functions
//		[FieldOffset(8)] private readonly void*  m_function;


		public TDelegate GetDelegate<TDelegate>() where TDelegate : Delegate
		{
			var del = Marshal.GetDelegateForFunctionPointer<TDelegate>((IntPtr) m_function);

			RuntimeHelpers.PrepareDelegate(del);

			return del;
		}

		/// <summary>
		/// Invokes a non-virtual method that uses the "this" pointer. That is, the method uses instance fields.
		/// </summary>
		/// <param name="instance">Instance to invoke the method upon</param>
		/// <param name="args">Parameters for the method, if any</param>
		public void Invoke<TInstance, TDelegate>(ref TInstance instance, params object[] args) where TDelegate : Delegate
		{
			TDelegate d = GetDelegate<TDelegate>();

			var __this = Unsafe.AddressOf(ref instance);
			if (!typeof(TInstance).IsValueType) {
				__this = Marshal.ReadIntPtr(__this);
			}

			if (args.Length == 0)
				d.DynamicInvoke(__this);
			else
				d.DynamicInvoke(__this, args);
		}*/

		/// <summary>
		/// Slightly slower than using MethodHandle.GetFunctionPointer
		///
		/// <remarks>
		/// Address-sensitive
		/// </remarks>
		/// </summary>
		public IntPtr Function {
			get {
#if SIGSCAN
				fixed (MethodDesc* __this = &this) {
					return CLRFunctions.MethodDescFunctions.GetMultiCallableAddrOfCode(__this);
				}
#endif

				return MethodInfo.MethodHandle.GetFunctionPointer();
			}
		}

		/// <summary>
		/// Slower than using Reflection
		///
		/// <remarks>
		/// Address-sensitive
		/// </remarks>
		/// </summary>
		public string Name {
			get {
#if SIGSCAN
				fixed (MethodDesc* __this = &this) {
					byte* lpcutf8 = CLRFunctions.MethodDescFunctions.GetName(__this);
					return CLRFunctions.StringFunctions.NewString(lpcutf8);
				}
#endif
				return MethodInfo.Name;
			}
		}


		private MethodDescFlags2 Flags2 => (MethodDescFlags2) m_bFlags2;
		private MethodDescFlags3 Flags3 => (MethodDescFlags3) m_wFlags3AndTokenRemainder;

		public override string ToString()
		{
			var flags3 = String.Join(", ", Flags3.GetFlags());
			var flags2 = String.Join(", ", Flags2.GetFlags());

			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Name", Name);
			table.AddRow("Function", Hex.ToHex(Function));
			table.AddRow(nameof(m_wFlags3AndTokenRemainder), m_wFlags3AndTokenRemainder);
			table.AddRow(nameof(m_chunkIndex), m_chunkIndex);
			table.AddRow(nameof(m_bFlags2), m_bFlags2);
			table.AddRow(nameof(m_wSlotNumber), m_wSlotNumber);
			table.AddRow(nameof(m_wFlags), m_wFlags);


			table.AddRow("Flags2", flags2);
			table.AddRow("Flags3", flags3);


			return table.ToMarkDownString();
		}
	}


}