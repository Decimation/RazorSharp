using System;
using JetBrains.Annotations;
using RazorSharp.Utilities.Security.Exceptions;

namespace RazorSharp.Utilities.Security
{
	/// <summary>
	/// Describes the failure message template used by inheriting <see cref="CoreException"/> types in
	/// <see cref="Guard"/>
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Field)]
	internal class FailMessageTemplateAttribute : Attribute
	{
		
	}
}