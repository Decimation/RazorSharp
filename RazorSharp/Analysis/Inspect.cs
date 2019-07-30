using System;
using RazorSharp.Utilities;

namespace RazorSharp.Analysis
{
	public static class Inspect
	{
		public static ObjectInfo Scan<T>(T value, InspectOptions options)
		{
			var info = Scan(value.GetType(), options);

			info.Value = value;

			if (options.HasFlagFast(InspectOptions.MemoryFields)) {
				info.WithMemoryFields();
			}

			return info;
		}

		public static ObjectInfo Scan(Type t, InspectOptions options)
		{
			var info = new ObjectInfo(t, options);

			if (options.HasFlagFast(InspectOptions.Fields)) {
				info.WithFields();
			}

			if (options.HasFlagFast(InspectOptions.Padding)) {
				info.WithPaddingFields();
			}

			return info;
		}
	}
}