using System;
using RazorSharp.Analysis;
using RazorSharp.CoreClr.Metadata.Enums;
using RazorSharp.Import.Enums;

namespace RazorSharp.Utilities
{
	/// <summary>
	/// Contains optimized versions of the <see cref="Enum.HasFlag"/> function.
	/// </summary>
	public static class EnumHelper
	{
		// ((uThis & uFlag) == uFlag)

		public static bool HasFlagFast(this InspectOptions value, InspectOptions flag)
		{
			return (value & flag) == flag;
		}
		
		public static bool HasFlagFast(this MethodDescFlags2 value, MethodDescFlags2 flag)
		{
			return (value & flag) == flag;
		}

		public static bool HasFlagFast(this MethodTableFlags value, MethodTableFlags flag)
		{
			return (value & flag) == flag;
		}

		public static bool HasFlagFast(this LayoutFlags value, LayoutFlags flag)
		{
			return (value & flag) == flag;
		}

		public static bool HasFlagFast(this VMFlags value, VMFlags flag)
		{
			return (value & flag) == flag;
		}

		public static bool HasFlagFast(this IdentifierOptions value, IdentifierOptions flag)
		{
			return (value & flag) == flag;
		}
		
		public static bool HasFlagFast(this ImportCallOptions value, ImportCallOptions flag)
		{
			return (value & flag) == flag;
		}
	}
}