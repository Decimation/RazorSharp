#region

using System.Reflection;

#endregion

namespace RazorSharp.CoreClr.Meta
{
	internal interface IMetaMember : IMeta
	{
		MemberInfo Info { get; }
	}
}