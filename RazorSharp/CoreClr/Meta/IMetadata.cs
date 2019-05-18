using System.Reflection;

namespace RazorSharp.CoreClr.Meta
{
	public interface IMetadata
	{
		int        Token { get; }
		string     Name  { get; }
		MemberInfo Info  { get; }
	}
}