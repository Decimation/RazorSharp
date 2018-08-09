#region

using System;
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
		/// </summary>
		public void* Function {
			get {
				fixed (MethodDesc* __this = &this) {
					return CLRFunctions.MethodDescFunctions.GetMultiCallableAddrOfCode(__this);
				}
			}
		}

		/// <summary>
		/// Slower than using Reflection
		/// </summary>
		public string Name {
			get {
				fixed (MethodDesc* __this = &this) {
					byte* lpcutf8 = CLRFunctions.MethodDescFunctions.GetName(__this);
					return CLRFunctions.StringFunctions.NewString(lpcutf8);
				}
			}
		}

		private MethodDescFlags2 Flags2 => (MethodDescFlags2) m_bFlags2;
		private MethodDescFlags3 Flags3 => (MethodDescFlags3) m_wFlags3AndTokenRemainder;

		public override string ToString()
		{
			var flags3 = String.Join(", ", Flags3.GetFlags());
			var flags2 = String.Join(", ", Flags2.GetFlags());

			var table = new ConsoleTable("Field", "Value");
			table.AddRow(nameof(m_wFlags3AndTokenRemainder), m_wFlags3AndTokenRemainder);
			table.AddRow(nameof(m_chunkIndex), m_chunkIndex);
			table.AddRow(nameof(m_bFlags2), m_bFlags2);
			table.AddRow(nameof(m_wSlotNumber), m_wSlotNumber);
			table.AddRow(nameof(m_wFlags), m_wFlags);


			table.AddRow("Flags2", flags2);
			table.AddRow("Flags3", flags3);

//			table.AddRow("Function", Hex.ToHex(Function));


			return table.ToMarkDownString();
		}
	}

	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/master/src/vm/method.hpp#L1701
	/// Use with: MethodDesc::m_bFlags2
	/// </summary>
	[Flags]
	internal enum MethodDescFlags2 : byte
	{
		/// <summary>
		/// The method entrypoint is stable (either precode or actual code)
		/// </summary>
		HasStableEntryPoint = 0x01,

		/// <summary>
		/// implies that HasStableEntryPoint is set.
		/// Precode has been allocated for this method
		/// </summary>
		HasPrecode = 0x02,

		IsUnboxingStub = 0x04,

		/// <summary>
		/// Has slot for native code
		/// </summary>
		HasNativeCodeSlot = 0x08,

		/// <summary>
		/// Jit may expand method as an intrinsic
		/// </summary>
		IsJitIntrinsic = 0x10,
	}

	/// <summary>
	/// Source: https://github.com/dotnet/coreclr/blob/master/src/vm/method.hpp#L1686
	/// Use with: MethodDesc::m_wFlags3AndTokenRemainder
	/// </summary>
	[Flags]
	internal enum MethodDescFlags3 : ushort
	{

		TokenRemainderMask = 0x3FFF,

		// These are separate to allow the flags space available and used to be obvious here
		// and for the logic that splits the token to be algorithmically generated based on the
		// #define

		/// <summary>
		/// Indicates that a type-forwarded type is used as a valuetype parameter (this flag is only valid for ngenned items)
		/// </summary>
		HasForwardedValuetypeParameter = 0x4000,

		/// <summary>
		/// Indicates that all typeref's in the signature of the method have been resolved to typedefs (or that process failed) (this flag is only valid for non-ngenned methods)
		/// </summary>
		ValueTypeParametersWalked = 0x4000,

		/// <summary>
		/// Indicates that we have verified that there are no equivalent valuetype parameters for this method
		/// </summary>
		DoesNotHaveEquivalentValuetypeParameters = 0x8000,
	}

}