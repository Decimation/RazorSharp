using System;
using RazorCommon;
using RazorSharp.Runtime;
using RazorSharp.Runtime.CLRTypes;
// ReSharper disable InconsistentNaming

namespace RazorSharp.Analysis
{

	public unsafe class ReferenceInspector<T> : Inspector<T> where T : class
	{
		public ReferenceInspector(ref T t, InspectorMode mode = InspectorMode.All) : base(ref t, mode)
		{
			Metadata  = new ReferenceMetadataInfo(ref t);
			Sizes     = new ReferenceSizeInfo(ref t);
			Addresses = new ReferenceAddressInfo(ref t);
		}


		private sealed class ReferenceMetadataInfo : MetadataInfo
		{
			public ObjHeader*   Header      { get; }
			public EEClass*     EEClass     { get; }

			internal ReferenceMetadataInfo(ref T t) : base(ref t)
			{
				Header      = Runtime.Runtime.ReadObjHeader(ref t);
				EEClass     = MethodTable->EEClass;
			}

			protected internal override ConsoleTable ToTable()
			{

				var table = base.ToTable();
				table.AddRow("EEClass", Hex.ToHex(EEClass));
				table.AddRow("Object Header", Hex.ToHex(Header));

				return table;
			}
		}

		private sealed class ReferenceSizeInfo : SizeInfo
		{
			public int BaseInstance   { get; }
			public int Heap           { get; }
			public int BaseFieldsSize { get; }

			internal ReferenceSizeInfo(ref T t) : base()
			{
				BaseInstance   = Unsafe.BaseInstanceSize<T>();
				Heap           = Unsafe.HeapSize(ref t);
				BaseFieldsSize = Unsafe.BaseFieldsSize<T>();
			}

			protected internal override ConsoleTable ToTable()
			{
				var table = base.ToTable();
				table.AddRow("Base instance size", BaseInstance);
				table.AddRow("Heap size", Heap);
				table.AddRow("Base fields size", BaseFieldsSize);
				return table;
			}
		}

		private sealed class ReferenceAddressInfo : AddressInfo
		{
			public IntPtr Heap   { get; }
			public IntPtr Fields { get; }

			/// <summary>
			/// String data if the type is a string,
			/// Array data if the type is an array
			/// </summary>
			public IntPtr HeapMisc { get; }

			internal ReferenceAddressInfo(ref T t) : base(ref t)
			{
				Heap   = Unsafe.AddressOfHeap(ref t);
				Fields = Unsafe.AddressOfHeap(ref t, OffsetType.Fields);
				if (typeof(T).IsArray)
					HeapMisc = Unsafe.AddressOfHeap(ref t, OffsetType.ArrayData);
				else if (typeof(T) == typeof(string))
					HeapMisc  = Unsafe.AddressOfHeap(ref t, OffsetType.StringData);
				else HeapMisc = IntPtr.Zero;
			}

			protected internal override ConsoleTable ToTable()
			{
				var table = base.ToTable();
				table.AddRow("Heap", Hex.ToHex(Heap));
				table.AddRow("Fields", Hex.ToHex(Fields));

				if (typeof(T).IsArray) {
					table.AddRow("Array data", Hex.ToHex(HeapMisc));
				}

				else if (typeof(T) == typeof(String)) {
					table.AddRow("String data", Hex.ToHex(HeapMisc));
				}
				else { }


				return table;
			}
		}

		public new static void Write(ref T t)
		{
			Console.WriteLine(new ReferenceInspector<T>(ref t));
		}

		public override string ToString()
		{
			var table = new ConsoleTable("Property", "Value");
			switch (_mode) {
				case InspectorMode.None:
					break;
				case InspectorMode.Meta:
					table.AddAllRows(((ReferenceMetadataInfo) Metadata).ToTable().Rows);
					break;
				case InspectorMode.Address:
					table.AddAllRows(((ReferenceAddressInfo) Addresses).ToTable().Rows);

					break;
				case InspectorMode.Size:
					table.AddAllRows(((ReferenceSizeInfo) Sizes).ToTable().Rows);
					break;
				case InspectorMode.All:
					table.AddAllRows(((ReferenceMetadataInfo) Metadata).ToTable().Rows);
					table.AddAllRows(((ReferenceAddressInfo) Addresses).ToTable().Rows);
					table.AddAllRows(((ReferenceSizeInfo) Sizes).ToTable().Rows);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}


			return table.ToMarkDownString();
		}
	}

}