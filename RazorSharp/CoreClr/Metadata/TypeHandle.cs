using System.Collections.Generic;
using System.Runtime.InteropServices;
using RazorSharp.Import;
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
			Imports = new ImportMap();
		}

		#region Fields

//		[FieldOffset(default)]
//		private TAddr m_asTAddr;

		[FieldOffset(default)]
		private void* m_asPtr;

		[FieldOffset(default)]
		private MethodTable* m_asMT;

		#endregion

		[ImportMapDesignation]
		private static readonly ImportMap Imports;
		
		internal Pointer<MethodTable> MethodTable {
			[ImportCall(IdentifierOptions.UseAccessorName, ImportCallOptions.Map)]
			get {
				fixed (TypeHandle* value = &this) {
					return Functions.Native.CallReturnPointer((void*) Imports[nameof(MethodTable)], value);
				}
			}
		}
	}
}