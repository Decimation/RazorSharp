using System;

namespace RazorSharp.Analysis
{
	[Flags]
	public enum InspectOptions
	{
		None = 0,
		Fields = 1 << 0,
		Values = 1 << 1,
		MemoryFields = (1 << 2) | Fields,
		Addresses = 1 << 3,
		Padding = 1 << 4,
	}
}