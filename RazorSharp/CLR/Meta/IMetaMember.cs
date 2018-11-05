#region

using System.Reflection;

#endregion

namespace RazorSharp.CLR.Meta
{

	internal interface IMetaMember : IMeta
	{
		MemberInfo Info { get; }
	}

}