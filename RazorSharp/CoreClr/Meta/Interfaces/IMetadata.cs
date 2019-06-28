using System.Reflection;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.CoreClr.Meta.Interfaces
{
	public interface IMetadata<T> : IToken
	{
		string Name { get; }

		MemberInfo Info { get; }

		Pointer<T> Value { get; }

		MetaInfoType MetaInfoType { get; }
	}
}