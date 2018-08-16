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
		None        = 0,
		Meta        = 1,
		Address     = 2,
		Size        = 4,
		Internal    = 8,
		FieldDescs  = 16,
		MethodDescs = 32,
		Layout      = 64,

		Default = Meta | Address | Size | Internal | Layout | FieldDescs
	}

	public unsafe class Inspector<T>
	{
		public MetadataInfo Metadata  { get; protected set; }
		public AddressInfo  Addresses { get; protected set; }
		public SizeInfo     Sizes     { get; protected set; }
		public InternalInfo Internal  { get; protected set; }
		public FieldInfo    Fields    { get; protected set; }
		public MethodInfo   Methods   { get; protected set; }

		private readonly ConsoleTable m_layout;

		protected readonly      InspectorMode Mode;
		private static readonly string        Separator = new string('-', Console.BufferWidth);


		public Inspector(ref T t, InspectorMode mode = InspectorMode.Default)
		{
			if (!typeof(T).IsValueType) {
				//throw new Exception("Use RefInspector for reference types");
			}

			Mode = mode;

			if (Mode.HasFlag(InspectorMode.Meta)) {
				Metadata = new MetadataInfo(ref t);
			}

			if (Mode.HasFlag(InspectorMode.Address)) {
				Addresses = new AddressInfo(ref t);
			}

			if (Mode.HasFlag(InspectorMode.Size)) {
				Sizes = new SizeInfo();
			}

			if (Mode.HasFlag(InspectorMode.Internal)) {
				Internal = new InternalInfo(ref t);
			}

			if (Mode.HasFlag(InspectorMode.FieldDescs) && !typeof(T).IsArray) {
				Fields = new FieldInfo(ref t);
			}

			if (Mode.HasFlag(InspectorMode.MethodDescs)) {
				Methods = new MethodInfo();
			}

			if (Mode.HasFlag(InspectorMode.Layout) && !typeof(T).IsArray) {
				m_layout = new ObjectLayout<T>(ref t, false).Table;
			}
		}


		public sealed class MethodInfo
		{
			public Pointer<MethodDesc>[] MethodDescs { get; }

			internal MethodInfo()
			{
				MethodDescs = Runtime.Runtime.GetMethodDescs<T>();
				MethodDescs = MethodDescs.OrderBy(x => (long) x.Reference.Function).ToArray();
			}

			private ConsoleTable ToTable()
			{
				var table = new ConsoleTable("MethodDesc Address", "Function Address", "Name");
				foreach (var v in MethodDescs) {
					table.AddRow(Hex.ToHex(v.Address), Hex.ToHex(v.Reference.Function), v.Reference.Name);
				}

				return table;
			}

			public override string ToString()
			{
				return CreateLabelString("MethodDescs:", ToTable());
			}
		}

		public sealed class FieldInfo
		{
			public           Pointer<FieldDesc>[] FieldDescs { get; }
			private readonly ConsoleTable         m_table;


			internal FieldInfo(ref T value)
			{
				if (typeof(T).IsArray) {
					FieldDescs = null;
				}
				else {
					FieldDescs = Runtime.Runtime.GetFieldDescs<T>();
					FieldDescs = FieldDescs.OrderBy(x => x.Reference.Offset).ToArray();
				}

				m_table = ToTable(ref value);
			}

			private ConsoleTable ToTable(ref T value)
			{
				const string omitted = "-";

				var table = new ConsoleTable("Field Offset", "FieldDesc Address", "Field Address", "CorType", "Static",
					"Size", "Name", "Value");

				if (FieldDescs != null) {
					foreach (var v in FieldDescs) {
						string fieldAddrHex =
							v.Reference.IsStatic ? omitted : Hex.ToHex(v.Reference.GetAddress(ref value));

						table.AddRow(v.Reference.Offset, Hex.ToHex(v.Address), fieldAddrHex, v.Reference.CorType,
							v.Reference.IsStatic ? StringUtils.Check : StringUtils.BallotX, v.Reference.Size,
							v.Reference.Name, v.Reference.GetValue(value));
					}
				}


				return table;
			}

			public override string ToString()
			{
				return CreateLabelString("FieldDescs:", m_table);
			}
		}

		private const string EEClassStr           = "EEClass";
		private const string MethodTableStr       = "Method Table";
		private const string CanonMTStr           = "Canon MT";
		private const string ObjHeaderStr         = "ObjHeader";
		private const string EEClassLayoutInfoStr = "EEClassLayoutInfo";

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
			public bool IsOnStack   { get; }
			public bool IsBlittable { get; }
			public bool IsValueType { get; }

			protected internal MetadataInfo(ref T t)
			{
				Value       = t;
				IsBlittable = Runtime.Runtime.IsBlittable<T>();
				IsValueType = typeof(T).IsValueType;
				IsOnStack   = Memory.Memory.IsOnStack(ref t);
			}


			protected virtual ConsoleTable ToTable()
			{
				var table = new ConsoleTable("Info", "Value");
				table.AddRow("Value",
					typeof(T).IsIListType()
						? String.Format("[{0}]", Collections.ListToString((IList) Value))
						: Value.ToString());

				table.AddRow("Blittable", IsBlittable ? StringUtils.Check : StringUtils.BallotX);
				table.AddRow("Value type", IsValueType ? StringUtils.Check : StringUtils.BallotX);
				table.AddRow("On stack", IsOnStack ? StringUtils.Check : StringUtils.BallotX);
				return table;
			}

			public override string ToString()
			{
				return CreateLabelString("Metadata:", ToTable());
			}
		}

		public class AddressInfo
		{
			// Address is the also the address of fields for value types
			public IntPtr Address { get; }

			protected internal AddressInfo(ref T t)
			{
				Address = Unsafe.AddressOf(ref t);
			}

			protected virtual ConsoleTable ToTable()
			{
				var table = new ConsoleTable(String.Empty, "Address");
				table.AddRow("Address", Hex.ToHex(Address));
				if (typeof(T).IsValueType) {
					table.AttachColumn("Fields", Hex.ToHex(Address));
				}

				return table;
			}

			public override string ToString()
			{
				return CreateLabelString("Addresses:", ToTable());
			}
		}

		public class SizeInfo
		{
			public int Size       { get; }
			public int Native     { get; }
			public int BaseFields { get; }
			public int Managed    { get; }

			protected internal SizeInfo()
			{
				Size       = Unsafe.SizeOf<T>();
				Native     = Unsafe.NativeSizeOf<T>();
				BaseFields = Unsafe.BaseFieldsSize<T>();
				Managed    = Unsafe.ManagedSizeOf<T>();
			}

			protected virtual ConsoleTable ToTable()
			{
				//var table = new ConsoleTable("Size type", "Value");
				//table.AddRow("Size", Size);
				//return table;
				var table = new ConsoleTable(String.Empty, "Size");
				table.AddRow("Size value", Size);
				table.AttachColumn("Native size", Native);
				table.AttachColumn("Managed size", Managed);
				table.AttachColumn($"Base fields size <{typeof(T).Name}>", BaseFields);


				return table;
			}

			public override string ToString()
			{
				return CreateLabelString("Sizes:", ToTable());
			}
		}

		protected internal static string CreateLabelString(string label, ConsoleTable table)
		{
			return String.Format("\n{0}\n{1}\n", ANSI.BoldString(label), table.ToMarkDownString());
		}

		public static void Write(ref T t, bool printStructures = false, InspectorMode mode = InspectorMode.Default)
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
			Console.WriteLine(inspector.Internal.MethodTable->ToString());

			Console.WriteLine(ANSI.BoldString(EEClassStr + colon));
			Console.WriteLine(inspector.Internal.EEClass->ToString());

			if (inspector.Internal.Canon != inspector.Internal.MethodTable) {
				Console.WriteLine(ANSI.BoldString(CanonMTStr + colon));
				Console.WriteLine(inspector.Internal.MethodTable->Canon->ToString());
			}

			if (inspector.Internal.EEClass->HasLayout) {
				Console.WriteLine(ANSI.BoldString(EEClassLayoutInfoStr + colon));
				Console.WriteLine(inspector.Internal.EEClass->LayoutInfo->ToString());
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

			if (Mode.HasFlag(InspectorMode.FieldDescs) && !typeof(T).IsArray) {
				sb.Append(Fields);
			}

			if (Mode.HasFlag(InspectorMode.MethodDescs)) {
				sb.Append(Methods);
			}

			if (Mode.HasFlag(InspectorMode.Layout) && !typeof(T).IsArray) {
				sb.Append(CreateLabelString("Memory layout:", m_layout));
			}

			return sb.ToString();
		}
	}

}