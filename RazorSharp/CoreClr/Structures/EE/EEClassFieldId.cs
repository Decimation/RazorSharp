namespace RazorSharp.CoreClr.Structures.EE
{
	public enum EEClassFieldId : uint
	{
		NumInstanceFields = 0,
		NumMethods,
		NumStaticFields,
		NumHandleStatics,
		NumBoxedStatics,
		NonGCStaticFieldBytes,
		NumThreadStaticFields,
		NumHandleThreadStatics,
		NumBoxedThreadStatics,
		NonGCThreadStaticFieldBytes,
		NumNonVirtualSlots,
		COUNT
	}
}