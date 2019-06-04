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

// ReSharper disable ParameterTypeCanBeEnumerable.Global

namespace RazorSharp.Analysis
{
	/// <summary>
	/// Provides utilities for inspecting objects.
	/// </summary>
	public static class Inspect
	{
		// Old: https://github.com/Decimation/RazorSharp/blob/3a3573f368cf021002cc15b466ca653129faf92c/RazorSharp/Analysis/ObjectLayout.cs
		// See also: https://github.com/SergeyTeplyakov/ObjectLayoutInspector

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
			if (RtInfo.IsStruct(value) && options.HasFlagFast(InspectOptions.Addresses)) {
				throw new InvalidOperationException(
					$"Use the ref-qualified method when using {InspectOptions.Addresses} with value types");
			}

			return Layout(ref value, options);
		}

		public static ConsoleTable DumpFields<T>(T[] values)
		{
			var type   = new MetaType(typeof(T));
			var fields = type.InstanceFields.ToArray();
			var table  = new ConsoleTable();

			table.AddColumn('#');

			foreach (var field in fields) {
				table.AddColumn(field.CleanName);
			}

			int i = 0;
			foreach (var value in values) {
				var fieldValues = new List<object>(fields.Length + 1)
				{
					i++
				};

				fieldValues.AddRange(fields.Select(field => field.ToValueString(value)));

				table.Rows.Add(fieldValues.ToArray());
			}

			return table;
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