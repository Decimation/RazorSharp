using System;
using System.Collections.Generic;
using System.Linq;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities;
using SimpleSharp;

namespace RazorSharp.Analysis
{
	public static class Inspect
	{
		// Old: https://github.com/Decimation/RazorSharp/blob/3a3573f368cf021002cc15b466ca653129faf92c/RazorSharp/Analysis/ObjectLayout.cs
		// Also: https://github.com/SergeyTeplyakov/ObjectLayoutInspector
		
		#region LayoutInfo

		/// <summary>
		/// Default <see cref="InspectOptions"/> when only a type is provided
		/// </summary>
		private const InspectOptions DEFAULT_TYPE_ONLY =
			InspectOptions.Sizes | InspectOptions.Types | InspectOptions.FieldOffsets;

		/// <summary>
		/// Default <see cref="InspectOptions"/> when a value is provided
		/// </summary>
		private const InspectOptions DEFAULT_VALUE_PROVIDED =
			DEFAULT_TYPE_ONLY | InspectOptions.Addresses | InspectOptions.Values;

		public static LayoutInfo Layout(Type type, InspectOptions options = DEFAULT_TYPE_ONLY)
		{
			var info = new LayoutInfo(type, options);
			info.Populate();

			return info;
		}

		public static LayoutInfo Layout<T>(InspectOptions options = DEFAULT_TYPE_ONLY) 
			=> Layout(typeof(T), options);

		public static LayoutInfo Layout<T>(ref T value, InspectOptions options = DEFAULT_VALUE_PROVIDED)
		{
			var info = Layout(value.GetType(), options);
			info.Populate(ref value);

			return info;
		}

		public static LayoutInfo Layout<T>(T value, InspectOptions options = DEFAULT_VALUE_PROVIDED)
		{
			if (Runtime.IsStruct(value) && options.HasFlagFast(InspectOptions.Addresses)) {
				throw new InvalidOperationException(
					$"Use the ref-qualified method when using {InspectOptions.Addresses} with value types");
			}

			return Layout(ref value, options);
		}

		#endregion

		#region String

		public static string LayoutString(Type type, InspectOptions options = DEFAULT_TYPE_ONLY)
			=> Layout(type, options).ToString();

		public static string LayoutString<T>(InspectOptions options = DEFAULT_TYPE_ONLY)
			=> Layout<T>(options).ToString();

		public static string LayoutString<T>(ref T value, InspectOptions options = DEFAULT_VALUE_PROVIDED)
			=> Layout(ref value, options).ToString();

		public static string LayoutString<T>(T value, InspectOptions options = DEFAULT_VALUE_PROVIDED)
			=> Layout(value, options).ToString();

		#endregion
	}
}