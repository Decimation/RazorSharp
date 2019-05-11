namespace RazorSharp.CoreClr.Meta
{
	/// <summary>
	/// Represents a structure which provides metadata.
	/// </summary>
	public interface IMeta
	{
		int    Token { get; }
		string Name  { get; }
	}
}