using System.Collections.Generic;
using System.Runtime.InteropServices;
using RazorSharp.Import.Attributes;
using RazorSharp.Import.Enums;
using RazorSharp.Interop;
using RazorSharp.Memory.Pointers;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable MemberCanBeMadeStatic.Global

namespace RazorSharp.CoreClr.Metadata
{
	[ImportNamespace]
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct TypeHandle
	{
		static TypeHandle()
		{
			ImportMap = new Dictionary<string, Pointer<byte>>();
		}

		#region Fields

//		[FieldOffset(default)]
//		private TAddr m_asTAddr;

		[FieldOffset(default)]
		private void* m_asPtr;

		[FieldOffset(default)]
		private MethodTable* m_asMT;

		#endregion

		[ImportMap]
		private static readonly Dictionary<string, Pointer<byte>> ImportMap;
		
		internal Pointer<MethodTable> MethodTable {
			[ImportCall(IdentifierOptions.UseAccessorName, ImportCallOptions.Map)]
			get {
				fixed (TypeHandle* value = &this) {
					return Functions.Native.CallReturnPointer((void*) ImportMap[nameof(MethodTable)], value);
				}
			}
		}
	}
}