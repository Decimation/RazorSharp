#region

using System;
using System.Collections;
using System.Linq;
using System.Text;
using RazorCommon;
using RazorCommon.Extensions;
using RazorCommon.Strings;
using RazorSharp.Pointers;
using RazorSharp.Runtime.CLRTypes;

#endregion

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

		/// <summary>
		/// WIP
		/// </summary>
		MethodDescs = 32,
		Layout = 64,
		All    = Meta | Address | Size | Internal | Layout | FieldDescs | MethodDescs,
	}

	public unsafe class Inspector<T>
	{
		public MetadataInfo Metadata  { get; protected set; }
		public AddressInfo  Addresses { get; protected set; }
		public SizeInfo     Sizes     { get; protected set; }
		public InternalInfo Internal  { get; protected set; }
		public FieldInfo    Fields    { get; protected set; }

		//public MethodInfo      Methods   { get; protected set; }
		public ObjectLayout<T> Layout { get; protected set; }

		protected readonly      InspectorMode Mode;
		private static readonly string        Separator = new string('-', Console.BufferWidth);

		public Inspector(ref T t, InspectorMode mode = InspectorMode.All)
		{
			if (!typeof(T).IsValueType) {
				//throw new Exception("Use RefInspector for reference types");
			}

			Mode      = mode;
			Metadata  = new MetadataInfo(ref t);
			Addresses = new AddressInfo(ref t);
			Sizes     = new SizeInfo();
			Internal  = new InternalInfo(ref t);
			Fields    = new FieldInfo();

			//Methods   = new MethodInfo();
			Layout = new ObjectLayout<T>(ref t, false);
		}

		// WIP
		/*public sealed class MethodInfo
		{
			public Pointer<MethodDesc>[] MethodDescs { get; }

			internal MethodInfo()
			{
				MethodDescs = Runtime.Runtime.GetMethodDescs<T>();
				MethodDescs = MethodDescs.OrderBy(x => (long) x.Value.Function).ToArray();
			}

			private ConsoleTable ToTable()
			{
				var table = new ConsoleTable("Function", "MethodDesc");
				foreach (var v in MethodDescs) {
					table.AddRow(Hex.ToHex(v.Value.Function), Hex.ToHex(v.Address));
				}

				return table;
			}

			public override string ToString()
			{
				return CreateLabelString("MethodDescs:", ToTable());
			}
		}*/

		public sealed class FieldInfo
		{
			public FieldDesc*[] FieldDescs { get; }

			internal FieldInfo()
			{
				if (typeof(T).IsArray) {
					FieldDescs = null;
				}
				else {
					FieldDescs = Runtime.Runtime.GetFieldDescs<T>();


					Pointer<FieldDesc>[] tmp = new Pointer<FieldDesc>[FieldDescs.Length];
					for (int i = 0; i < FieldDescs.Length; i++) {
						tmp[i] = FieldDescs[i];
					}

					tmp = tmp.OrderBy(x => x.Value.Offset).ToArray();
					for (int i = 0; i < tmp.Length; i++) {
						FieldDescs[i] = (FieldDesc*) tmp[i].Address;
					}
				}
			}

			private ConsoleTable ToTable()
			{
				var table = new ConsoleTable("Field Offset", "FD Address", "CorType", "Static", "Size");

				if (FieldDescs != null) {
					foreach (var v in FieldDescs) {
						table.AddRow(v->Offset, Hex.ToHex(v), v->CorType,
							v->IsStatic ? StringUtils.Check : StringUtils.BallotX, v->Size);
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
		private const string ObjHeaderStr   = "ObjHeader";

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
				var table = new ConsoleTable(String.Empty, MethodTableStr);
				table.AddRow("Address", Hex.ToHex(MethodTable));
				table.AttachColumn(EEClassStr, Hex.ToHex(EEClass));
				table.AttachColumn(CanonMTStr, Hex.ToHex(Canon));


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
				table.AddRow("Value",
					typeof(T).IsIListType() ? Collections.ListToString((IList) Value) : Value.ToString());

				//table.AddRow("Blittable", IsBlittable ? StringUtils.Check : StringUtils.BallotX);
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
				table.AddRow("Address", Hex.ToHex(Address));
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
			WriteInspector(inspector, printStructures);
		}

		protected static void WriteInspector(Inspector<T> inspector, bool printStructures)
		{
			Console.WriteLine(Separator);
			Console.WriteLine("Inspection of type {0}", typeof(T).Name);
			Console.WriteLine(inspector);

			if (printStructures) {
				PrintStructures(inspector);
			}

			Console.WriteLine(Separator);
		}

		private static void PrintStructures(Inspector<T> inspector)
		{
			const char colon = ':';

			Console.WriteLine(ANSI.BoldString(MethodTableStr + colon));
			Console.WriteLine(*inspector.Internal.MethodTable);

			Console.WriteLine(ANSI.BoldString(EEClassStr + colon));
			Console.WriteLine(inspector.Internal.EEClass->ToString());

			Console.WriteLine(ANSI.BoldString(CanonMTStr + colon));
			Console.WriteLine(inspector.Internal.MethodTable->Canon->ToString());
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

			if (Mode.HasFlag(InspectorMode.MethodDescs)) {
				//sb.Append(Methods);
			}

			if (Mode.HasFlag(InspectorMode.Layout)) {
				sb.Append(Layout);
			}

			return sb.ToString();
		}
	}

}