// ReSharper disable InconsistentNaming
namespace RazorSharp.CoreClr.Enums.EEClass
{
	internal enum EEClassFieldId : uint
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