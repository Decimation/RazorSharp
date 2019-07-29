using System;
using RazorSharp.Import.Enums;

namespace RazorSharp.CoreClr.Metadata.Enums
{
	/// <summary>
	/// Contains optimized versions of the <see cref="Enum.HasFlag"/> function.
	/// </summary>
	public static class EnumUtil
	{
		// ((uThis & uFlag) == uFlag)

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