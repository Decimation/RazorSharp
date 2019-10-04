namespace RazorSharp.Memory.Components
{
	/// <summary>
	/// Represents padding in memory.
	/// </summary>
	internal class PaddingField : MemoryField
	{
		internal PaddingField(int offset, int size = sizeof(int)) : base("(Padding)", offset, size) { }
		
		public override object GetValue(object value)
		{
			// Padding is always null
			return default(int);
		}
	}
}