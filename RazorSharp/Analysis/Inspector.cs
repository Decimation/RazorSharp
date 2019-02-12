#region

using System;
using System.Collections;
using System.Linq;
using System.Text;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.CLR.Structures.EE;
using RazorSharp.Common;
using RazorSharp.Memory;
using RazorSharp.Pointers;

// ReSharper disable StaticMemberInGenericType

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

	[Obsolete]
	public static class InspectorHelper
	{
		public static string LayoutString<T>(ref T t, bool fieldsOnly = false)
		{
			var v = new ObjectLayout<T>(ref t, fieldsOnly).Table;
			return v.ToMarkDownString();
		}

		public static string LayoutString<T>(bool fieldsOnly = false)
		{
			var v = new ObjectLayout<T>(fieldsOnly).Table;
			return v.ToMarkDownString();
		}


		internal static string CreateLabelString(string label, ConsoleTable table)
		{
			return string.Format("\n{0}\n{1}\n", /*ANSI.BoldString(label)*/ label, table.ToMarkDownString());
		}

		#region Value types

		public static void InspectVal<T>(ref T t,
			InspectorMode                      mode) where T : struct
		{
			Inspector<T>.Write(ref t, false, mode);
		}

		public static void InspectVal<T>(ref T t, bool printStructures = false,
			InspectorMode                      mode = InspectorMode.Default) where T : struct
		{
			Inspector<T>.Write(ref t, printStructures, mode);
		}

		#endregion

		#region Reference types

		public static void Inspect<T>(ref T t, InspectorMode mode) where T : class
		{
			// todo: This causes an InvalidProgramException
			if (mode.HasFlag(InspectorMode.MethodDescs) && typeof(T) == typeof(string))
				throw new Exception(
					$"Flag {InspectorMode.MethodDescs} cannot be used on typeof({typeof(string).Name})");

			ReferenceInspector<T>.Write(ref t, false, mode);
		}

		public static void Inspect<T>(ref T t, bool printStructures = false,
			InspectorMode                   mode = InspectorMode.Default) where T : class
		{
			ReferenceInspector<T>.Write(ref t, printStructures, mode);
		}

		#endregion
	}

	[Obsolete]
	public unsafe class Inspector<T>
	{
		private const           string EEClassStr           = "EEClass";
		private const           string MethodTableStr       = "Method Table";
		private const           string CanonMTStr           = "Canon MT";
		private const           string ObjHeaderStr         = "ObjHeader";
		private const           string EEClassLayoutInfoStr = "EEClassLayoutInfo";
		private static readonly string Separator            = new string('-', Console.BufferWidth);

		private readonly ConsoleTable m_layout;

		protected readonly InspectorMode Mode;


		public Inspector(ref T t, InspectorMode mode = InspectorMode.Default)
		{
			Mode = mode;

			if (Mode.HasFlag(InspectorMode.Meta)) Metadata = new MetadataInfo(ref t);

			if (Mode.HasFlag(InspectorMode.Address)) Addresses = new AddressInfo(ref t);

			if (Mode.HasFlag(InspectorMode.Size)) Sizes = new SizeInfo();

			if (Mode.HasFlag(InspectorMode.Internal)) Internal = new InternalInfo(ref t);

			if (Mode.HasFlag(InspectorMode.FieldDescs) && !typeof(T).IsArray) Fields = new FieldInfo(ref t);

			if (Mode.HasFlag(InspectorMode.MethodDescs)) Methods = new MethodInfo();

			if (Mode.HasFlag(InspectorMode.Layout) && !typeof(T).IsArray)
				m_layout = new ObjectLayout<T>(ref t, false).Table;
		}

		public MetadataInfo Metadata  { get; protected set; }
		public AddressInfo  Addresses { get; protected set; }
		public SizeInfo     Sizes     { get; protected set; }
		public InternalInfo Internal  { get; protected set; }
		public FieldInfo    Fields    { get; protected set; }
		public MethodInfo   Methods   { get; protected set; }

		internal static void Write(ref T t, bool printStructures = false, InspectorMode mode = InspectorMode.Default)
		{
			var inspector = new Inspector<T>(ref t, mode);
			WriteInspector(inspector, printStructures);
		}

		protected static void WriteInspector(Inspector<T> inspector, bool printStructures)
		{
			Console.WriteLine(Separator);
			Console.WriteLine("Inspection of type {0}", typeof(T).Name);
			Console.WriteLine(inspector);

			if (printStructures) PrintStructures(inspector);

			Console.WriteLine(Separator);
		}

		private static void PrintStructures(Inspector<T> inspector)
		{
			const char colon = ':';

//			Console.WriteLine(ANSI.BoldString(MethodTableStr + colon));
			Console.WriteLine(MethodTableStr + colon);
			Console.WriteLine(inspector.Internal.MethodTable);

//			Console.WriteLine(ANSI.BoldString(EEClassStr + colon));
			Console.WriteLine(EEClassStr + colon);
			Console.WriteLine(inspector.Internal.EEClass);

			if (inspector.Internal.Canon != inspector.Internal.MethodTable) {
//				Console.WriteLine(ANSI.BoldString(CanonMTStr + colon));
				Console.WriteLine(CanonMTStr + colon);
				Console.WriteLine(inspector.Internal.MethodTable.Reference.Canon);
			}

			if (inspector.Internal.EEClass.Reference.HasLayout) {
//				Console.WriteLine(ANSI.BoldString(EEClassLayoutInfoStr + colon));
				Console.WriteLine(EEClassLayoutInfoStr + colon);
				Console.WriteLine(inspector.Internal.EEClass.Reference.LayoutInfo->ToString());
			}
		}


		public override string ToString()
		{
			var sb = new StringBuilder();

			if (Mode.HasFlag(InspectorMode.Meta)) sb.Append(Metadata);

			if (Mode.HasFlag(InspectorMode.Address)) sb.Append(Addresses);

			if (Mode.HasFlag(InspectorMode.Size)) sb.Append(Sizes);

			if (Mode.HasFlag(InspectorMode.Internal)) sb.Append(Internal);

			if (Mode.HasFlag(InspectorMode.FieldDescs) && !typeof(T).IsArray) sb.Append(Fields);

			if (Mode.HasFlag(InspectorMode.MethodDescs)) sb.Append(Methods);

			if (Mode.HasFlag(InspectorMode.Layout) && !typeof(T).IsArray)
				sb.Append(InspectorHelper.CreateLabelString("Memory layout:", m_layout));

			return sb.ToString();
		}


		public sealed class MethodInfo
		{
			internal MethodInfo()
			{
				MethodDescs = typeof(T).GetMethodDescs();
				MethodDescs = MethodDescs.OrderBy(x => (long) x.Reference.Function).ToArray();
			}

			// todo: was public
			internal Pointer<MethodDesc>[] MethodDescs { get; }

			private ConsoleTable ToTable()
			{
				var table = new ConsoleTable("MethodDesc Address", "Function Address", "Name");
				foreach (Pointer<MethodDesc> v in MethodDescs)
					table.AddRow(Hex.ToHex(v.Address), Hex.ToHex(v.Reference.Function), v.Reference.Name);

				return table;
			}

			public override string ToString()
			{
				return InspectorHelper.CreateLabelString("MethodDescs:", ToTable());
			}
		}

		public sealed class FieldInfo
		{
			private readonly ConsoleTable m_table;


			internal FieldInfo(ref T value)
			{
				if (typeof(T).IsArray) {
					FieldDescs = null;
				}
				else {
					FieldDescs = typeof(T).GetFieldDescs();
					FieldDescs = FieldDescs.OrderBy(x => x.Reference.Offset).ToArray();
				}

				m_table = ToTable(ref value);
			}

			// todo: was public
			internal Pointer<FieldDesc>[] FieldDescs { get; }

			private ConsoleTable ToTable(ref T value)
			{
				const string omitted = "-";

				var table = new ConsoleTable("Field Offset", "FieldDesc Address", "Field Address", "CorType",
					"Static", "Size", "Name", "Value", "Metadata Token");

				if (FieldDescs != null)
					foreach (Pointer<FieldDesc> v in FieldDescs) {
						string fieldAddrHex =
							v.Reference.IsStatic ? omitted : Hex.ToHex(v.Reference.GetAddress(ref value));

						table.AddRow(v.Reference.Offset, Hex.ToHex(v.Address), fieldAddrHex, v.Reference.CorType,
							v.Reference.IsStatic.Prettify(), v.Reference.Size,
							v.Reference.Name, v.Reference.GetValue(value), Hex.ToHex(v.Reference.Token));
					}

				return table;
			}

			public override string ToString()
			{
				return InspectorHelper.CreateLabelString("FieldDescs:", m_table);
			}
		}

		public class InternalInfo
		{
			protected internal InternalInfo(ref T t)
			{
				MethodTable = Runtime.ReadMethodTable(ref t);
				EEClass     = MethodTable.Reference.EEClass;
				Canon       = MethodTable.Reference.Canon;
			}

			// Value types have a MethodTable, but not a
			// MethodTable*.

			// todo: these were public

			internal Pointer<MethodTable> MethodTable { get; }
			internal Pointer<EEClass>     EEClass     { get; }
			internal Pointer<MethodTable> Canon       { get; }

			protected virtual ConsoleTable ToTable()
			{
				var table = new ConsoleTable(string.Empty, MethodTableStr);
				table.AddRow("Address", Hex.ToHex(MethodTable.Address));
				table.AttachColumn(EEClassStr, Hex.ToHex(EEClass.Address));
				table.AttachColumn(CanonMTStr, Hex.ToHex(Canon.Address));


				return table;
			}

			public override string ToString()
			{
				return InspectorHelper.CreateLabelString("Internal:", ToTable());
			}
		}

		public class MetadataInfo
		{
			protected internal MetadataInfo(ref T t)
			{
				Value       = t;
				IsBlittable = Runtime.IsBlittable<T>();
				IsValueType = typeof(T).IsValueType;
				IsOnStack   = Mem.IsOnStack(ref t);
			}

			public T    Value       { get; }
			public bool IsOnStack   { get; }
			public bool IsBlittable { get; }
			public bool IsValueType { get; }

			protected virtual ConsoleTable ToTable()
			{
				var table = new ConsoleTable("Info", "Value");
				table.AddRow("Value",
					typeof(T).IsIListType()
						? string.Format("[{0}]", Collections.ToString((IList) Value))
						: Value.ToString());

				table.AddRow("Blittable", IsBlittable.Prettify());
				table.AddRow("Value type", IsValueType.Prettify());
				table.AddRow("On stack", IsOnStack.Prettify());
				return table;
			}

			public override string ToString()
			{
				return InspectorHelper.CreateLabelString("Metadata:", ToTable());
			}
		}

		public class AddressInfo
		{
			protected internal AddressInfo(ref T t)
			{
				Address = Unsafe.AddressOf(ref t).Address;
			}

			// Address is the also the address of fields for value types
			public IntPtr Address { get; }

			protected virtual ConsoleTable ToTable()
			{
				var table = new ConsoleTable(string.Empty, "Address");
				table.AddRow("Address", Hex.ToHex(Address));
				if (typeof(T).IsValueType) table.AttachColumn("Fields", Hex.ToHex(Address));

				return table;
			}

			public override string ToString()
			{
				return InspectorHelper.CreateLabelString("Addresses:", ToTable());
			}
		}

		public class SizeInfo
		{
			protected internal SizeInfo()
			{
				Size       = Unsafe.SizeOf<T>();
				Native     = Unsafe.NativeSizeOf<T>();
				BaseFields = Unsafe.BaseFieldsSize<T>();
				Managed    = Unsafe.ManagedSizeOf<T>();
			}

			public int Size       { get; }
			public int Native     { get; }
			public int BaseFields { get; }
			public int Managed    { get; }

			protected virtual ConsoleTable ToTable()
			{
				//var table = new ConsoleTable("Size type", "Value");
				//table.AddRow("Size", Size);
				//return table;
				var table = new ConsoleTable(string.Empty, "Size");
				table.AddRow("Size value", Size);
				table.AttachColumn("Native size", Native);
				table.AttachColumn("Managed size", Managed);
				table.AttachColumn($"Base fields size <{typeof(T).Name}>", BaseFields);


				return table;
			}

			public override string ToString()
			{
				return InspectorHelper.CreateLabelString("Sizes:", ToTable());
			}
		}
	}
}