using System;
using System.Runtime.InteropServices;

namespace RazorSharp.CoreJit {
	//CORINFO_RESOLVED_TOKEN
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct CorInfoResolvedToken
	{
		//
		// [In] arguments of resolveToken
		//
		internal IntPtr           tokenContext; //Context for resolution of generic arguments
		internal IntPtr           tokenScope;
		internal UInt32           token; //The source token
		internal CorInfoTokenKind tokenType;

		//
		// [Out] arguments of resolveToken. 
		// - Type handle is always non-NULL.
		// - At most one of method and field handles is non-NULL (according to the token type).
		// - Method handle is an instantiating stub only for generic methods. Type handle 
		//   is required to provide the full context for methods in generic types.
		//
		internal IntPtr hClass;
		internal IntPtr hMethod;
		internal IntPtr hField;

		//
		// [Out] TypeSpec and MethodSpec signatures for generics. NULL otherwise.
		//
		internal Byte   pTypeSpec;
		internal UInt32 cbTypeSpec;
		internal Byte   pMethodSpec;
		internal UInt32 cbMethodSpec;
	}
}