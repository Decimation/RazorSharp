using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RazorCommon;
using RazorSharp.Runtime;
using RazorSharp.Runtime.CLRTypes;

// ReSharper disable InconsistentNaming

namespace RazorSharp.Analysis
{

	using Runtime = Runtime.Runtime;

	public unsafe class RefInspector<T> : Inspector<T> where T : class
	{
		public new ReferenceSizeInfo     Sizes     => (ReferenceSizeInfo) base.Sizes;
		public new ReferenceMetadataInfo Metadata  => (ReferenceMetadataInfo) base.Metadata;
		public new ReferenceAddressInfo  Addresses => (ReferenceAddressInfo) base.Addresses;
		public new ReferenceInternalInfo Internal  => (ReferenceInternalInfo) base.Internal;

		public RefInspector(ref T t, InspectorMode mode = InspectorMode.All) : base(ref t, mode)
		{
			base.Metadata  = new ReferenceMetadataInfo(ref t);
			base.Sizes     = new ReferenceSizeInfo(ref t);
			base.Addresses = new ReferenceAddressInfo(ref t);
			base.Internal  = new ReferenceInternalInfo(ref t);
		}

		public sealed class ReferenceInternalInfo : InternalInfo
		{
			public ObjHeader* Header { get; }

			internal ReferenceInternalInfo(ref T t) : base(ref t)
			{
				Header = Runtime.ReadObjHeader(ref t);
			}

			protected override ConsoleTable ToTable()
			{
				var table = base.ToTable();

				table.AddRow("Object Header", Hex.ToHex(Header));
				return table;
			}
		}

		public sealed class ReferenceMetadataInfo : MetadataInfo
		{
			internal ReferenceMetadataInfo(ref T t) : base(ref t) { }

			protected override ConsoleTable ToTable()
			{
				var table = base.ToTable();
				return table;
			}
		}

		public sealed class ReferenceSizeInfo : SizeInfo
		{
			public int Heap           { get; }
			public int BaseInstance   { get; }
			public int BaseFieldsSize { get; }

			internal ReferenceSizeInfo(ref T t) : base()
			{
				Heap           = Unsafe.HeapSize(ref t);
				BaseInstance   = Unsafe.BaseInstanceSize<T>();
				BaseFieldsSize = Unsafe.BaseFieldsSize<T>();
			}

			protected override ConsoleTable ToTable()
			{
				/*var table = base.ToTable();
				table.AddRow("Heap size", Heap);
				table.AddRow("Base instance size", BaseInstance);
				table.AddRow("Base fields size", BaseFieldsSize);
				return table;*/

				var table = base.ToTable();
				table.AttachColumn("Heap size", Heap);
				table.AttachColumn("Base instance size", BaseInstance);
				table.AttachColumn("Base fields size", BaseFieldsSize);


				return table;
			}
		}


		public sealed class ReferenceAddressInfo : AddressInfo
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

			protected override ConsoleTable ToTable()
			{
				var table = base.ToTable();
				table.AttachColumn("Heap", Hex.ToHex(Heap));
				table.AttachColumn("Fields", Hex.ToHex(Fields));


				if (typeof(T).IsArray) {
					table.AttachColumn("Array data", Hex.ToHex(HeapMisc));
				}

				else if (typeof(T) == typeof(String)) {
					table.AttachColumn("String data", Hex.ToHex(HeapMisc));
				}
				else { }


				return table;
			}
		}


		public new static void Write(ref T t, bool printStructures = false, InspectorMode mode = InspectorMode.All)
		{
			var inspector = new RefInspector<T>(ref t, mode);
			Console.WriteLine(inspector);

			if (printStructures) {
				PrintStructures(inspector);
			}
		}
	}

}