using System;
using RazorSharp.Utilities;

namespace RazorSharp.Analysis
{
	public static class Inspect
	{
		public static ObjectInfo Scan<T>(ref T value, InspectOptions options)
		{
			var info = Scan(value.GetType(), options);

			info.Update(ref value);

			return info;
		}

		public static ObjectInfo Scan(Type t, InspectOptions options)
		{
			var info = new ObjectInfo(t, options);

			info.Update();

			return info;
		}
	}
}