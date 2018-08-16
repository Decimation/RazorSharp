#region

using System;
using System.Diagnostics;
using RazorCommon;
using RazorSharp.Runtime.CLRTypes;

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.Analysis
{

	public unsafe class RefInspector<T> : Inspector<T> where T : class
	{
		public new ReferenceSizeInfo     Sizes     => (ReferenceSizeInfo) base.Sizes;
		public new ReferenceMetadataInfo Metadata  => (ReferenceMetadataInfo) base.Metadata;
		public new ReferenceAddressInfo  Addresses => (ReferenceAddressInfo) base.Addresses;
		public new ReferenceInternalInfo Internal  => (ReferenceInternalInfo) base.Internal;

		public RefInspector(ref T t, InspectorMode mode = InspectorMode.Default) : base(ref t, mode)
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
				Header = Runtime.Runtime.ReadObjHeader(ref t);
			}

			protected override ConsoleTable ToTable()
			{
				var table = base.ToTable();

				table.AttachColumn("Object Header", Hex.ToHex(Header));
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
			public int Heap              { get; }
			public int BaseInstance      { get; }
			public int BaseFieldsUnboxed { get; }

			private readonly string m_typeName;

			internal ReferenceSizeInfo(ref T t)
			{
				Heap              = Unsafe.HeapSize(ref t);
				BaseInstance      = Unsafe.BaseInstanceSize<T>();
				BaseFieldsUnboxed = Unsafe.BaseFieldsSize(ref t);
				m_typeName        = t.GetType().Name;
			}

			protected override ConsoleTable ToTable()
			{
				/*var table = base.ToTable();
				table.AddRow("Heap size", Heap);
				table.AddRow("Base instance size", BaseInstance);
				table.AddRow("Base fields size", BaseFieldsSize);
				return table;*/

				var table = base.ToTable();

				// todo: if the value is boxed
				if (m_typeName != typeof(T).Name) {
					table.AttachColumn($"Base fields size <{m_typeName}>", BaseFieldsUnboxed);
				}

				table.AttachColumn("Heap size", Heap);
				table.AttachColumn("Base instance size", BaseInstance);


				table.DetachFromColumns(Unsafe.InvalidValue);
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

				else if (typeof(T) == typeof(string)) {
					table.AttachColumn("String data", Hex.ToHex(HeapMisc));
				}


				return table;
			}
		}


		public new static void Write(ref T t, bool printStructures = false, InspectorMode mode = InspectorMode.Default)
		{
			var inspector = new RefInspector<T>(ref t, mode);
			WriteInspector(inspector, printStructures);
		}
	}

}