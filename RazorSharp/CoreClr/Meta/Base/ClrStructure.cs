using System;
using System.Globalization;
using System.Reflection;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities.Security;
using SimpleSharp;
using SimpleSharp.Strings.Formatting;

namespace RazorSharp.CoreClr.Meta.Base
{
	/// <summary>
	/// Describes a CLR structure that has metadata information.
	/// </summary>
	/// <typeparam name="TClr">CLR structure type</typeparam>
	public abstract unsafe class ClrStructure<TClr> : IFormattable where TClr : unmanaged
	{
		#region Fields

		/// <summary>
		/// Points to the internal CLR structure representing this instance
		/// </summary>
		public Pointer<TClr> Value { get; }

		/// <summary>
		/// The native, built-in form of <see cref="Value"/>
		/// </summary>
		protected internal TClr* NativePointer => Value.ToPointer<TClr>();

		public abstract MemberInfo Info { get; }

		public string Name => Info.Name;

		protected virtual ConsoleTable InfoTable => IdTable;

		private ConsoleTable IdTable {
			get {
				var table = new ConsoleTable("Property", "Value");

				table.AddRow("Handle", Value);
				table.AddRow(nameof(Token), Hex.ToHex(Token));

				return table;
			}
		}

		public abstract int Token { get; }

		#endregion

		#region Constructors

		// Root constructor
		protected ClrStructure(Pointer<TClr> ptr)
		{
			Value = ptr;
		}

		internal ClrStructure(MemberInfo member) : this(Runtime.ResolveHandle(member)) { }

		#endregion


		#region ToString

		public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);

		public override string ToString() => ToString(ClrStructureSettings.DefaultFormat);

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return format switch
			{
				ClrStructureSettings.FORMAT_ALL => InfoTable.ToString(),
				ClrStructureSettings.FORMAT_MIN => IdTable.ToString(),
				_ => IdTable.ToString(),
			};
		}

		#endregion

		#region Equality

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			if (obj.GetType() != this.GetType())
				return false;

			return Equals((ClrStructure<TClr>) obj);
		}

		public bool Equals(ClrStructure<TClr> other)
		{
			return this.Value == other.Value;
		}

		public override int GetHashCode()
		{
			throw Guard.NotImplementedFail(nameof(GetHashCode));
		}

		public static bool operator ==(ClrStructure<TClr> left, ClrStructure<TClr> right) => Equals(left, right);

		public static bool operator !=(ClrStructure<TClr> left, ClrStructure<TClr> right) => !Equals(left, right);

		#endregion
	}
}