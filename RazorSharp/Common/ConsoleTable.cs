using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RazorSharp.Common
{

	/// <summary>
	/// Source: https://github.com/khalidabuhakmeh/ConsoleTables
	/// </summary>
	public class ConsoleTable
	{
		public IList<object>   Columns { get; set; }
		public IList<object[]> Rows    { get; protected set; }

		public ConsoleTableOptions Options { get; protected set; }

		public ConsoleTable(params string[] columns)
			: this(new ConsoleTableOptions {Columns = new List<string>(columns)}) { }

		public ConsoleTable(ConsoleTableOptions options)
		{
			Options = options ?? throw new ArgumentNullException(nameof(options));
			Rows    = new List<object[]>();
			Columns = new List<object>(options.Columns);
		}

		public ConsoleTable AddColumn(IEnumerable<string> names)
		{
			foreach (var name in names)
				Columns.Add(name);
			return this;
		}

		public ConsoleTable AddRow(params object[] values)
		{
			if (values == null)
				throw new ArgumentNullException(nameof(values));

			if (!Columns.Any())
				throw new Exception("Please set the columns first");

			if (Columns.Count != values.Length)
				throw new Exception(
					$"The number columns in the row ({Columns.Count}) does not match the values ({values.Length}");

			Rows.Add(values);
			return this;
		}

		/// <summary>
		/// <para>Custom method</para>
		///
		/// Removes the entire row if one cell in the row contains one of the args
		/// </summary>
		public ConsoleTable RemoveFromRows(params object[] args)
		{
			// Before:
			// | Field | Value |
			// |-------|-------|
			// | -1    | bar   |

			// RemoveFromRows(-1):
			// | Field | Value |
			// |-------|-------|

			var matching = from a in Rows from b in a from c in args where b.Equals(c) select a;
			Rows.RemoveAtRange(matching.Select(v => Rows.IndexOf(v)).ToArray());

			/*for (int i = Rows.Count - 1; i >= 0; i--) {
				for (int j = Rows[i].Length - 1; j >= 0; j--) {
					foreach (object t in args) {
						if (Rows[i][j] == t || Rows[i][j].ToString() == t.ToString()) {
							Rows.RemoveAt(i);
						}
					}
				}
			}*/

			return this;
		}

		/// <summary>
		/// Custom method<para></para>
		///
		/// Similar to RemoveFromRows, but for columns.<para></para>
		/// Removes the corresponding column if one of the rows in the columns contains one of the args<para></para>
		/// </summary>
		public ConsoleTable DetachFromColumns(params object[] args)
		{
			// Before:
			//              | Size | Heap size | Base instance size | Base fields size |
			// |------------|------|-----------|--------------------|------------------|
			// | Size value | 8    | 36        | -1                 | -1               |

			// DetachFromColumns(-1):
			//              | Size | Heap size |
			// |------------|------|-----------|
			// | Size value | 8    | 36        |


			for (int i = Rows.Count - 1; i >= 0; i--) {
				for (int j = Rows[i].Length - 1; j >= 0; j--) {
					for (int k = 0; k < args.Length; k++) {
						if (Rows[i][j].Equals(args[k])) {
							Rows[i] = Collections.RemoveAt(Rows[i], j);
							Columns.RemoveAt(j);
						}
					}
				}
			}


			return this;
		}


		public ConsoleTable RemoveColumn(int index)
		{
			Columns.RemoveAt(index);
			for (int i = Rows.Count - 1; i >= 0; i--) {
				Rows[i] = Collections.RemoveAt(Rows[i], index);
			}

			return this;
		}


		/// <summary>
		/// Custom method
		/// </summary>
		public ConsoleTable AttachColumn(string col, object rowval)
		{
			return AttachColumn(col, new[] {rowval});
		}

		/// <summary>
		/// Custom method
		/// </summary>
		public ConsoleTable AttachColumn(string col, object[] rowval)
		{
			AddColumn(new[] {col});

			if (rowval.Length != Rows.Count) {
				throw new Exception();
			}

			var ls = Rows[0].ToList();
			ls.AddRange(rowval);
			Rows.Clear();
			Rows.Add(ls.ToArray());

			return this;
		}

		public static ConsoleTable From<T>(IEnumerable<T> values)
		{
			var table = new ConsoleTable();

			var columns = GetColumns<T>();

			table.AddColumn(columns);

			foreach (var propertyValues in values.Select(value =>
				columns.Select(column => GetColumnValue<T>(value, column))))
				table.AddRow(propertyValues.ToArray());

			return table;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();

			// find the longest column by searching each row
			var columnLengths = ColumnLengths();

			// create the string format with padding
			var format = Enumerable.Range(0, Columns.Count)
				             .Select(i => " | {" + i + ",-" + columnLengths[i] + "}")
				             .Aggregate((s, a) => s + a) + " |";

			// find the longest formatted line
			var maxRowLength  = Math.Max(0, Rows.Any() ? Rows.Max(row => string.Format(format, row).Length) : 0);
			var columnHeaders = string.Format(format, Columns.ToArray());

			// longest line is greater of formatted columnHeader and longest row
			var longestLine = Math.Max(maxRowLength, columnHeaders.Length);

			// add each row
			var results = Rows.Select(row => string.Format(format, row)).ToList();

			// create the divider
			var divider = " " + string.Join("", Enumerable.Repeat("-", longestLine - 1)) + " ";

			builder.AppendLine(divider);
			builder.AppendLine(columnHeaders);

			foreach (var row in results) {
				builder.AppendLine(divider);
				builder.AppendLine(row);
			}

			builder.AppendLine(divider);

			if (Options.EnableCount) {
				builder.AppendLine();
				builder.AppendFormat(" Count: {0}", Rows.Count);
			}

			return builder.ToString();
		}

		public string ToMarkDownString()
		{
			return ToMarkDownString('|');
		}

		private string ToMarkDownString(char delimiter)
		{
			var builder = new StringBuilder();

			// find the longest column by searching each row
			var columnLengths = ColumnLengths();

			// create the string format with padding
			var format = Format(columnLengths, delimiter);

			// find the longest formatted line
			var columnHeaders = string.Format(format, Columns.ToArray());


			// add each row
			var results = Rows.Select(row => string.Format(format, row)).ToList();

			// create the divider
			var divider = Regex.Replace(columnHeaders, @"[^|]", "-");

			// custom subroutine:
			// remove the first delimiter if the first column is empty
			if (Columns[0].ToString() == string.Empty) {
				columnHeaders = ' ' + columnHeaders.Substring(1);
			}

			builder.AppendLine(columnHeaders);
			builder.AppendLine(divider);
			results.ForEach(row => builder.AppendLine(row));

			return builder.ToString();
		}

		public string ToMinimalString()
		{
			return ToMarkDownString(char.MinValue);
		}

		public string ToStringAlternative()
		{
			var builder = new StringBuilder();

			// find the longest column by searching each row
			var columnLengths = ColumnLengths();

			// create the string format with padding
			var format = Format(columnLengths);

			// find the longest formatted line
			var columnHeaders = string.Format(format, Columns.ToArray());

			// add each row
			var results = Rows.Select(row => string.Format(format, row)).ToList();

			// create the divider
			var divider     = Regex.Replace(columnHeaders, @"[^|]", "-");
			var dividerPlus = divider.Replace("|", "+");

			builder.AppendLine(dividerPlus);
			builder.AppendLine(columnHeaders);

			foreach (var row in results) {
				builder.AppendLine(dividerPlus);
				builder.AppendLine(row);
			}

			builder.AppendLine(dividerPlus);

			return builder.ToString();
		}

		private string Format(List<int> columnLengths, char delimiter = '|')
		{
			var delimiterStr = delimiter == char.MinValue ? string.Empty : delimiter.ToString();
			var format = (Enumerable.Range(0, Columns.Count)
				              .Select(i => " " + delimiterStr + " {" + i + ",-" + columnLengths[i] + "}")
				              .Aggregate((s, a) => s + a) + " " + delimiterStr).Trim();
			return format;
		}

		private List<int> ColumnLengths()
		{
			var columnLengths = Columns
				.Select((t, i) => Rows.Select(x => x[i])
					.Union(new[] {Columns[i]})
					.Where(x => x != null)
					.Select(x => x.ToString().Length).Max())
				.ToList();
			return columnLengths;
		}

		public void Write(RFormat format = RFormat.Default)
		{
			switch (format) {
				case RFormat.Default:
					System.Console.WriteLine(ToString());
					break;
				case RFormat.MarkDown:
					System.Console.WriteLine(ToMarkDownString());
					break;
				case RFormat.Alternative:
					System.Console.WriteLine(ToStringAlternative());
					break;
				case RFormat.Minimal:
					System.Console.WriteLine(ToMinimalString());
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		private static IEnumerable<string> GetColumns<T>()
		{
			return typeof(T).GetProperties().Select(x => x.Name).ToArray();
		}

		private static object GetColumnValue<T>(object target, string column)
		{
			return typeof(T).GetProperty(column).GetValue(target, null);
		}
	}

	public class ConsoleTableOptions
	{
		public IEnumerable<string> Columns     { get; set; } = new List<string>();
		public bool                EnableCount { get; set; } = false;
	}

	public enum RFormat
	{
		Default     = 0,
		MarkDown    = 1,
		Alternative = 2,
		Minimal     = 3
	}

}