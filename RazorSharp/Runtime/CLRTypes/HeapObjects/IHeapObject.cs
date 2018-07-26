namespace RazorSharp.Runtime.CLRTypes.HeapObjects
{

	public unsafe interface IHeapObject
	{
		// The object header is at a negative offset of
		// -IntPtr.Size, so we can't represent it in fixed structs
		ObjHeader*   Header      { get; }
		MethodTable* MethodTable { get; }
	}

}