using System;
using static RazorSharp.CorJit.JitConstants;

namespace RazorSharp.CorJit
{
	public enum CorJitResult : Int32
	{
		CORJIT_OK = 0,

		CORJIT_BADCODE =
			unchecked((Int32) (((UInt32) (SEVERITY_ERROR) << 31) | ((UInt32) (FACILITY_NULL) << 16) |
			                   ((UInt32) (1)))),

		CORJIT_OUTOFMEM =
			unchecked((Int32) (((UInt32) (SEVERITY_ERROR) << 31) | ((UInt32) (FACILITY_NULL) << 16) |
			                   ((UInt32) (2)))),

		CORJIT_INTERNALERROR =
			unchecked((Int32) (((UInt32) (SEVERITY_ERROR) << 31) | ((UInt32) (FACILITY_NULL) << 16) |
			                   ((UInt32) (3)))),

		CORJIT_SKIPPED =
			unchecked((Int32) (((UInt32) (SEVERITY_ERROR) << 31) | ((UInt32) (FACILITY_NULL) << 16) |
			                   ((UInt32) (4)))),

		CORJIT_RECOVERABLEERROR =
			unchecked((Int32) (((UInt32) (SEVERITY_ERROR) << 31) | ((UInt32) (FACILITY_NULL) << 16) |
			                   ((UInt32) (5)))),
	};
}