using System;
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
			public ObjHeader* Header  { get; }


			internal ReferenceInternalInfo(ref T t) : base(ref t)
			{
				Header  = Runtime.ReadObjHeader(ref t);

			}

			protected internal override ConsoleTable ToTable()
			{
				var table = base.ToTable();

				table.AddRow("EEClass", Hex.ToHex(EEClass));
				table.AddRow("Object Header", Hex.ToHex(Header));
				return table;
			}
		}

		public sealed class ReferenceMetadataInfo : MetadataInfo
		{
			internal ReferenceMetadataInfo(ref T t) : base(ref t) { }

			protected internal override ConsoleTable ToTable()
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

			protected internal override ConsoleTable ToTable()
			{
				var table = base.ToTable();
				table.AddRow("Heap size", Heap);
				table.AddRow("Base instance size", BaseInstance);
				table.AddRow("Base fields size", BaseFieldsSize);
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
			var inspector = new RefInspector<T>(ref t);
			Console.WriteLine(inspector);


		}
	}

}