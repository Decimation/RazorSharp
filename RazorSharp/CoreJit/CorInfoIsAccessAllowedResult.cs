namespace RazorSharp.CoreJit
{
	internal enum CorInfoIsAccessAllowedResult
	{
		CORINFO_ACCESS_ALLOWED       = 0, // Call allowed
		CORINFO_ACCESS_ILLEGAL       = 1, // Call not allowed
		CORINFO_ACCESS_RUNTIME_CHECK = 2  // Ask at runtime whether to allow the call or not
	}
}