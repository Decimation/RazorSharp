#region

using System.Reflection;
using NativeSharp.Kernel;
using NativeSharp.Kernel.Enums;
using RazorSharp.CoreClr.Meta.Base;
using RazorSharp.CoreClr.Metadata.JitIL;
using RazorSharp.Memory.Pointers;
using SimpleSharp;
using SimpleSharp.Diagnostics;

// ReSharper disable ReturnTypeCanBeEnumerable.Global

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.CoreClr.Meta
{
	/// <summary>
	///     <list type="bullet">
	///         <item><description>CLR structure: <see cref="ILMethod"/></description></item>
	///         <item><description>Reflection structure: <see cref="MethodBody"/></description></item>
	///     </list>
	/// </summary>
	public class MetaIL : PseudoClrStructure<ILMethod>
	{
		#region Constructor

		internal MetaIL(Pointer<ILMethod> ptr) : base(ptr) { }

		#endregion

		#region Accessors

		/// <summary>
		///     Whether this type is <see cref="TinyILMethod" />
		/// </summary>
		public bool IsTiny => Value.Reference.IsTiny;

		/// <summary>
		///     Whether this type is <see cref="FatILMethod" />
		/// </summary>
		public bool IsFat => Value.Reference.IsFat;

		/// <summary>
		///     Points to the JIT IL code
		/// </summary>
		public Pointer<byte> Code => Value.Reference.Code;

		/// <summary>
		///     Length/size of the IL code (<see cref="Code" />)
		/// </summary>
		public int CodeSize => Value.Reference.CodeSize;

		/// <summary>
		///     <remarks>
		///         Equals <see cref="System.Reflection.MethodBody.MaxStackSize" />
		///     </remarks>
		/// </summary>
		public int MaxStackSize => Value.Reference.MaxStackSize;

		/// <summary>
		///     <remarks>
		///         Equals <see cref="System.Reflection.MethodBody.LocalSignatureMetadataToken" />
		///     </remarks>
		/// </summary>
		public override int Token => Value.Reference.Token;

		/// <summary>
		///     <remarks>
		///         <see cref="IsFat" /> must be <c>true</c>
		///     </remarks>
		/// </summary>
		public CorILMethodFlags Flags => Value.Reference.Flags;

		/// <summary>
		///     <remarks>
		///         Equals <see cref="System.Reflection.MethodBody.GetILAsByteArray()" />
		///     </remarks>
		/// </summary>
		/// <returns></returns>
		public byte[] RawIL => Value.Reference.RawIL;

		public Instruction[] Instructions => InspectIL.GetInstructions(RawIL);

		#endregion

		internal void SetCode(byte[] code) //todo: WIP
		{
			Conditions.Require(code.Length <= CodeSize, nameof(code));

			int ul  = code.Length;
			var  ptr = Code.Address;

			Kernel32.VirtualProtect(ptr, ul, MemoryProtection.ExecuteReadWrite, out var oldProtect);

			Code.WriteAll(code);

			Kernel32.VirtualProtect(ptr, ul, oldProtect, out oldProtect);
		}

		public override ConsoleTable Debug {
			get {
				var table = base.Debug;

				table.AddRow(nameof(Code), Code);
				table.AddRow(nameof(CodeSize), CodeSize);
				table.AddRow(nameof(MaxStackSize), MaxStackSize);

				if (IsFat) {
					table.AddRow(nameof(Flags), Flags);
				}
				
				table.AddRow(nameof(IsTiny), IsTiny);
				table.AddRow(nameof(IsFat), IsFat);

				return table;
			}
		}
	}
}