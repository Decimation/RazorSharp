using System;

namespace RazorSharp.Utilities
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	public class BitfieldAttribute : Attribute
	{
		public string Name  { get; set; }
		
		/// <summary>
		/// Bit count
		/// </summary>
		public int    Count { get; set; }

		public BitfieldAttribute(string name, int count)
		{
			Name  = name;
			Count = count;
		}
	}
}