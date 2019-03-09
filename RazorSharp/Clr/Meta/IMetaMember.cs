#region

using System.Reflection;

#endregion

namespace RazorSharp.Clr.Meta
{
	internal interface IMetaMember : IMeta
	{
		MemberInfo Info { get; }
	}
}