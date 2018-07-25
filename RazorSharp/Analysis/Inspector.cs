using System;
using RazorCommon;
using RazorSharp.Runtime;

namespace RazorSharp.Analysis
{
	[Flags]
	public enum InspectorMode
	{
		None = 0,
		Meta = 1,
		Address = 2,
		Size = 4,
		All = Meta | Address | Size,
	}
	public unsafe class Inspector<T>
	{
		public MetadataInfo Metadata  { get; protected set; }
		public AddressInfo  Addresses { get; protected set; }
		public SizeInfo     Sizes     { get; protected set; }

		protected readonly InspectorMode _mode;

		public Inspector(ref T t, InspectorMode mode = InspectorMode.All)
		{
			_mode = mode;
			Metadata  = new MetadataInfo(ref t);
			Addresses = new AddressInfo(ref t);
			Sizes     = new SizeInfo();
		}


		public class MetadataInfo
		{
			public T    Value       { get; }
			public bool IsBlittable { get; }
			public bool IsValueType { get; }

			// Value types have a MethodTable, but not a
			// MethodTable*.
			public MethodTable* MethodTable { get; }

			protected internal MetadataInfo(ref T t)
			{
				Value       = t;
				IsBlittable = Unsafe.IsBlittable<T>();
				IsValueType = typeof(T).IsValueType;
				MethodTable = Runtime.Runtime.ReadMethodTable(ref t);
			}

			protected internal virtual ConsoleTable ToTable()
			{
				var table = new ConsoleTable("Field", "Value");
				table.AddRow("Value", Value);
				table.AddRow("Blittable", IsBlittable);
				table.AddRow("Value type", IsValueType);
				table.AddRow("Method Table", Hex.ToHex(MethodTable));
				return table;
			}
		}

		public class AddressInfo
		{
			public IntPtr Address { get; }

			protected internal AddressInfo(ref T t)
			{
				Address = Unsafe.AddressOf(ref t);
			}

			protected internal virtual ConsoleTable ToTable()
			{
				var table = new ConsoleTable("Address type", "Value");
				table.AddRow("Address", Hex.ToHex(Address));
				return table;
			}
		}

		public class SizeInfo
		{
			public int Size { get; }

			protected internal SizeInfo()
			{
				Size = Unsafe.SizeOf<T>();
			}

			protected internal virtual ConsoleTable ToTable()
			{
				var table = new ConsoleTable("Size type", "Value");
				table.AddRow("Size", Size);
				return table;
			}
		}

		public override string ToString()
		{
			var table = new ConsoleTable("Property", "Value");

			switch (_mode) {
				case InspectorMode.None:
					break;
				case InspectorMode.Meta:
					table.AddAllRows(Metadata.ToTable().Rows);
					break;
				case InspectorMode.Address:
					table.AddAllRows(Addresses.ToTable().Rows);
					break;
				case InspectorMode.Size:
					table.AddAllRows(Sizes.ToTable().Rows);
					break;
				case InspectorMode.All:
					table.AddAllRows(Metadata.ToTable().Rows);
					table.AddAllRows(Addresses.ToTable().Rows);
					table.AddAllRows(Sizes.ToTable().Rows);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}



			return table.ToMarkDownString();
		}
	}

}