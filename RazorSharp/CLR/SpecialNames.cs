#region

using System;
using RazorCommon.Extensions;

#endregion

namespace RazorSharp.CLR
{

	internal static class SpecialNames
	{
		internal const string FIXED_BUFFER_NAME         = "<{0}>e__FixedBuffer";
		internal const string BACKING_FIELD_NAME        = "<{0}>" + BACKING_FIELD_NAME_SUFFIX;
		private const  string BACKING_FIELD_NAME_SUFFIX = "k__BackingField";


		internal static string NameOfFixedBuffer(string fieldName)
		{
			return String.Format(FIXED_BUFFER_NAME, fieldName);
		}

		internal static string DemangledAutoPropertyName(string fieldName)
		{
			if (fieldName.Contains(BACKING_FIELD_NAME_SUFFIX)) {
				string x = fieldName.JSubstring(fieldName.IndexOf('<') + 1, fieldName.IndexOf('>'));

				return x;
			}

			return null;
		}

		/// <summary>
		///     Gets the internal name of an auto-property's backing field.
		///     <example>If the auto-property's name is X, the backing field name is &lt;X&gt;k__BackingField.</example>
		/// </summary>
		/// <param name="propname">Auto-property's name</param>
		/// <returns>Internal name of the auto-property's backing field</returns>
		internal static string NameOfAutoPropertyBackingField(string propname)
		{
			return String.Format(BACKING_FIELD_NAME, propname);
		}

		internal static string NameOfGetPropertyMethod(string propname)
		{
			const string getPrefix = "get_";
			return getPrefix + propname;
		}
	}

}