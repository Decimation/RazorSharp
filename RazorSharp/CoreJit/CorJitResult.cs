namespace RazorSharp.CoreJit
{
	internal enum CorJitResult
	{
		CORJIT_OK = 0,

		CORJIT_BADCODE = unchecked((int) (((uint) JitConstants.SEVERITY_ERROR << 31) |
		                                  ((uint) JitConstants.FACILITY_NULL << 16) | 1)),

		CORJIT_OUTOFMEM =
			unchecked((int) (((uint) JitConstants.SEVERITY_ERROR << 31) | ((uint) JitConstants.FACILITY_NULL << 16) |
			                 2)),

		CORJIT_INTERNALERROR =
			unchecked((int) (((uint) JitConstants.SEVERITY_ERROR << 31) | ((uint) JitConstants.FACILITY_NULL << 16) |
			                 3)),

		CORJIT_SKIPPED = unchecked((int) (((uint) JitConstants.SEVERITY_ERROR << 31) |
		                                  ((uint) JitConstants.FACILITY_NULL << 16) | 4)),

		CORJIT_RECOVERABLEERROR =
			unchecked((int) (((uint) JitConstants.SEVERITY_ERROR << 31) | ((uint) JitConstants.FACILITY_NULL << 16) |
			                 5))
	}
}