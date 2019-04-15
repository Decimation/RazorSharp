namespace RazorSharp.CoreJit
{
	internal enum CorInfoRegionKind : uint
	{
		CORINFO_REGION_NONE,
		CORINFO_REGION_HOT,
		CORINFO_REGION_COLD,
		CORINFO_REGION_JIT
	}
}