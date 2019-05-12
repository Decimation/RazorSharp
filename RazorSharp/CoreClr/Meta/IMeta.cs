namespace RazorSharp.CoreClr.Meta
{
	/// <summary>
	/// Represents a structure which provides a name and metadata token.
	/// </summary>
	public interface IMeta
	{
		int    Token { get; }
		string Name  { get; }
	}
}