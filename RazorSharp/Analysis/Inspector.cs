using System;
using System.Collections;
using System.Linq;
using System.Text;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp.Pointers;
using RazorSharp.Runtime;
using RazorSharp.Runtime.CLRTypes;

// ReSharper disable InconsistentNaming

namespace RazorSharp.Analysis
{

	[Flags]
	public enum InspectorMode
	{
		None       = 0,
		Meta       = 1,
		Address    = 2,
		Size       = 4,
		Internal   = 8,
		FieldDescs = 16,
		Layout     = 32,
		All        = Meta | Address | Size | Internal | Layout | FieldDescs,
	}

	public unsafe class Inspector<T>
	{
		public MetadataInfo    Metadata  { get; protected set; }
		public AddressInfo     Addresses { get; protected set; }
		public SizeInfo        Sizes     { get; protected set; }
		public InternalInfo    Internal  { get; protected set; }
		public FieldInfo       Fields    { get; protected set; }
		public ObjectLayout<T> Layout    { get; protected set; }


		protected readonly InspectorMode Mode;

		public Inspector(ref T t, InspectorMode mode = InspectorMode.All)
		{
			Mode      = mode;
			Metadata  = new MetadataInfo(ref t);
			Addresses = new AddressInfo(ref t);
			Sizes     = new SizeInfo();
			Internal  = new InternalInfo(ref t);
			Fields    = new FieldInfo();

			if (!typeof(T).IsArray)
				Layout = new ObjectLayout<T>(ref t);
		}

		public sealed class FieldInfo
		{
			public LitePointer<FieldDesc>[] FieldDescs { get; }

			internal FieldInfo()
			{
				if (typeof(T).IsArray) {
					FieldDescs = null;
				}
				else {
					FieldDescs = Runtime.Runtime.GetFieldDescs<T>();
					FieldDescs = FieldDescs.OrderBy(x => x.Value.Offset).ToArray();
				}
			}

			private ConsoleTable ToTable()
			{
				var table = new ConsoleTable("Offset", "Address", "CorType", "Static", "Size");

				if (FieldDescs != null) {
					foreach (var v in FieldDescs) {
						table.AddRow(v.Value.Offset, Hex.ToHex(v.Address), v.Value.CorType,
							v.Value.IsStatic ? StringUtils.Check : StringUtils.BallotX, v.Value.Size);
					}
				}


				return table;
			}

			public override string ToString()
			{
				return CreateLabelString("FieldDescs:", ToTable());
			}
		}

		private const string EEClassStr     = "EEClass";
		private const string MethodTableStr = "Method Table";
		private const string CanonMTStr     = "Canon MT";

		public class InternalInfo
		{
			// Value types have a MethodTable, but not a
			// MethodTable*.
			public MethodTable* MethodTable { get; }
			public EEClass*     EEClass     { get; }
			public MethodTable* Canon       { get; }


			protected internal InternalInfo(ref T t)
			{
				MethodTable = Runtime.Runtime.ReadMethodTable(ref t);
				EEClass     = MethodTable->EEClass;
				Canon       = MethodTable->Canon;
			}

			protected virtual ConsoleTable ToTable()
			{
				var table = new ConsoleTable("Info", "Value");
				table.AddRow(MethodTableStr, Hex.ToHex(MethodTable));


				if (MethodTable->UnionType == Runtime.CLRTypes.MethodTable.LowBits.EEClass) {
					table.AddRow(EEClassStr, Hex.ToHex(EEClass));
				}
				else if (MethodTable->UnionType == Runtime.CLRTypes.MethodTable.LowBits.MethodTable)
					table.AddRow(CanonMTStr, Hex.ToHex(Canon));


				return table;
			}

			public override string ToString()
			{
				return CreateLabelString("Internal:", ToTable());
			}
		}


		public class MetadataInfo
		{
			public T    Value       { get; }
			public bool IsBlittable { get; }
			public bool IsValueType { get; }

			protected internal MetadataInfo(ref T t)
			{
				Value       = t;
				IsBlittable = Runtime.Runtime.IsBlittable<T>();
				IsValueType = typeof(T).IsValueType;
			}


			protected virtual ConsoleTable ToTable()
			{
				var table = new ConsoleTable("Info", "Value");
				table.AddRow("Value", typeof(T).IsArray ? Collections.ListToString((IList) Value) : Value.ToString());
				table.AddRow("Blittable", IsBlittable ? StringUtils.Check : StringUtils.BallotX);
				table.AddRow("Value type", IsValueType ? StringUtils.Check : StringUtils.BallotX);
				return table;
			}

			public override string ToString()
			{
				return CreateLabelString("Metadata:", ToTable());
			}
		}

		public class AddressInfo
		{
			public IntPtr Address { get; }

			protected internal AddressInfo(ref T t)
			{
				Address = Unsafe.AddressOf(ref t);
			}

			protected virtual ConsoleTable ToTable()
			{
				var table = new ConsoleTable(String.Empty, "Address");
				table.AddRow("Address type", Hex.ToHex(Address));
				return table;
			}

			public override string ToString()
			{
				return CreateLabelString("Addresses:", ToTable());
			}
		}

		public class SizeInfo
		{
			public int Size { get; }

			protected internal SizeInfo()
			{
				Size = Unsafe.SizeOf<T>();
			}

			protected virtual ConsoleTable ToTable()
			{
				//var table = new ConsoleTable("Size type", "Value");
				//table.AddRow("Size", Size);
				//return table;
				var table = new ConsoleTable(String.Empty, "Size");
				table.AddRow("Size value", Size);

				return table;
			}

			public override string ToString()
			{
				return CreateLabelString("Sizes:", ToTable());
			}
		}

		protected internal static string CreateLabelString(string label, ConsoleTable table)
		{
			var cols = table.Columns.Count;

			return String.Format("\n{0}\n{1}\n", ANSI.BoldString(label), table.ToMarkDownString());
		}

		public static void Write(ref T t, bool printStructures = false, InspectorMode mode = InspectorMode.All)
		{
			var inspector = new Inspector<T>(ref t, mode);
			Console.WriteLine(inspector);

			if (printStructures) {
				PrintStructures(inspector);
			}
		}

		protected static void PrintStructures(Inspector<T> inspector)
		{
			Console.WriteLine(ANSI.BoldString(MethodTableStr + ':'));
			Console.WriteLine(*inspector.Internal.MethodTable);

			if (inspector.Internal.MethodTable->UnionType == (MethodTable.LowBits.EEClass)) {
				Console.WriteLine(ANSI.BoldString(EEClassStr + ':'));
				Console.WriteLine(inspector.Internal.EEClass->ToString());
			}

			if (inspector.Internal.MethodTable->UnionType == (MethodTable.LowBits.MethodTable)) {
				Console.WriteLine(ANSI.BoldString(CanonMTStr + ':'));
				Console.WriteLine(inspector.Internal.MethodTable->Canon->ToString());
			}
		}


		public override string ToString()
		{
			var sb = new StringBuilder();


			if (Mode.HasFlag(InspectorMode.Meta)) {
				sb.Append(Metadata);
			}

			if (Mode.HasFlag(InspectorMode.Address)) {
				sb.Append(Addresses);
			}

			if (Mode.HasFlag(InspectorMode.Size)) {
				sb.Append(Sizes);
			}

			if (Mode.HasFlag(InspectorMode.Internal)) {
				sb.Append(Internal);
			}

			if (Mode.HasFlag(InspectorMode.FieldDescs)) {
				sb.Append(Fields);
			}

			if (Mode.HasFlag(InspectorMode.Layout)) {
				sb.Append(Layout);
			}

			return sb.ToString();
		}
	}

}