using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RazorSharp.Import.Enums;

namespace RazorSharp.Import.Attributes
{
	/// <summary>
	/// Designates the <see cref="Dictionary{TKey,TValue}"/> to use for members specified with
	/// <see cref="ImportCallOptions.Map"/>
	/// </summary>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Field)]
	public class ImportMapAttribute : Attribute
	{
		
	}
}