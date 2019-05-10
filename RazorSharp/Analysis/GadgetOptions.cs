using System;

namespace RazorSharp.Analysis
{
	[Flags]
	public enum GadgetOptions
	{
//		FieldNames = 0,

		None = 0,

		FieldSizes = 1,

		FieldAddresses = 2,

		FieldOffsets = 4,

		FieldTypes = 8,

		FieldValues = 16,

		InternalStructures = 32,
	}

	internal static class GadgetOptionsExtensions
	{
		internal static bool HasFlagFast(this GadgetOptions value, GadgetOptions flag)
		{
			return (value & flag) == flag;
		}
	}
}