#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace RazorSharp.Common
{

	public static class Enums
	{
		private const string JoinEnumStr = ", ";

		public static string Join(this Enum e, string joinStr = JoinEnumStr)
		{
			return String.Join(joinStr, e.GetFlags());
		}

		public static IEnumerable<Enum> GetFlags(this Enum e)
		{
			return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag).Distinct();
		}
	}

}