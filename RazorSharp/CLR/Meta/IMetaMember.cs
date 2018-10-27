using System.Reflection;

namespace RazorSharp.CLR.Meta
{

	internal interface IMetaMember : IMeta
	{
		MemberInfo Info { get; }
	}

}