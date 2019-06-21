using System.Reflection;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.CoreClr.Meta.Interfaces
{
	public interface IMetadata : IToken
	{
		string     Name  { get; }
		MemberInfo Info  { get; }
		Pointer<byte> Value { get; }
	}
}