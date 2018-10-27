using System.Reflection;

namespace RazorSharp.CLR.Meta
{

	internal interface IMeta
	{
		int    Token { get; }
		string Name  { get; }
	}

}