#region

using System.Runtime.InteropServices;
using RazorSharp.Memory.Extern;
using RazorSharp.Memory.Extern.Symbols;
using RazorSharp.Memory.Extern.Symbols.Attributes;
using TADDR = System.UInt64;

// ReSharper disable BuiltInTypeReferenceStyle

#endregion

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable MemberCanBeMadeStatic.Global

namespace RazorSharp.CoreClr.Structures
{
	/*************************************************************************/
	// A TypeHandle is the FUNDAMENTAL concept of type identity in the CLR.
	// That is two types are equal if and only if their type handles
	// are equal.  A TypeHandle, is a pointer sized struture that encodes 
	// everything you need to know to figure out what kind of type you are
	// actually dealing with.  

	// At the present time a TypeHandle can point at two possible things
	//
	//      1) A MethodTable    (Intrinsics, Classes, Value Types and their instantiations)
	//      2) A TypeDesc       (all other cases: arrays, byrefs, pointer types, function pointers, generic type variables)  
	//
	// or with IL stubs, a third thing:
	//
	//      3) A MethodTable for a native value type.
	//
	// Array MTs are not valid TypeHandles: for example no allocated object will
	// ever return such a type handle from Object::GetTypeHandle(), and
	// these type handles should not be passed across the JIT Interface
	// as CORINFO_CLASS_HANDLEs.  However some code in the EE does create 
	// temporary TypeHandles out of these MTs, so we can't yet assert 
	// !pMT->IsArray() in the TypeHandle constructor.
	//
	// Wherever possible, you should be using TypeHandles or MethodTables.
	// Code that is known to work over Class/ValueClass types (including their
	// instantiations) is currently written to use MethodTables.
	//
	// TypeDescs in turn break down into several variants and are
	// for special cases around the edges
	//    - array types whose method tables get share
	//    - types for function pointers for verification and reflection
	//    - types for generic parameters for verification and reflection
	//
	// Generic type instantiations (in C# syntax: C<ty_1,...,ty_n>) are represented by
	// MethodTables, i.e. a new MethodTable gets allocated for each such instantiation.
	// The entries in these tables (i.e. the code) are, however, often shared.
	// Clients of TypeHandle don't need to know any of this detail; just use the
	// GetInstantiation and HasInstantiation methods.
	[ClrSymNamespace]
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct TypeHandle
	{
		// https://github.com/dotnet/coreclr/blob/master/src/vm/typehandle.h

		#region Fields

//			union 
//			{
//				TADDR m_asTAddr; // we look at the low order bits
//#ifndef DACCESS_COMPILE
//				void *              m_asPtr;
//				PTR_MethodTable     m_asMT;
//				PTR_TypeDesc        m_asTypeDesc;
//				PTR_ArrayTypeDesc   m_asArrayTypeDesc;
//				PTR_ParamTypeDesc   m_asParamTypeDesc;
//				PTR_TypeVarTypeDesc m_asTypeVarTypeDesc;
//				PTR_FnPtrTypeDesc   m_asFnPtrTypeDesc;
//#endif
//			};

		#region Fields

		[FieldOffset(0)]
		private TADDR m_asTAddr;

		[FieldOffset(0)]
		private void* m_asPtr;

		[FieldOffset(0)]
		private MethodTable* m_asMT;

		[FieldOffset(0)]
		private TypeDesc* m_asTypeDesc;

		// etc

		#endregion

		#endregion

		[Symcall]
		internal MethodTable* GetMethodTable()
		{
			throw new SymImportException();
		}

		static TypeHandle()
		{
			Symload.Load(typeof(TypeHandle));
		}
	}
}