#region

using System;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Common;

#endregion

// ReSharper disable InconsistentNaming

namespace RazorSharp.Analysis
{
	[Obsolete]
	public unsafe class ReferenceInspector<T> : Inspector<T> where T : class
	{
		public ReferenceInspector(ref T t, InspectorMode mode = InspectorMode.Default) : base(ref t, mode)
		{
			base.Metadata  = new ReferenceMetadataInfo(ref t);
			base.Sizes     = new ReferenceSizeInfo(ref t);
			base.Addresses = new ReferenceAddressInfo(ref t);
			base.Internal  = new ReferenceInternalInfo(ref t);
		}

		public new ReferenceSizeInfo     Sizes     => (ReferenceSizeInfo) base.Sizes;
		public new ReferenceMetadataInfo Metadata  => (ReferenceMetadataInfo) base.Metadata;
		public new ReferenceAddressInfo  Addresses => (ReferenceAddressInfo) base.Addresses;
		public new ReferenceInternalInfo Internal  => (ReferenceInternalInfo) base.Internal;


		internal new static void Write(ref T t, bool printStructures = false,
			InspectorMode                    mode = InspectorMode.Default)
		{
			var inspector = new ReferenceInspector<T>(ref t, mode);
			WriteInspector(inspector, printStructures);
		}

		public sealed class ReferenceInternalInfo : InternalInfo
		{
			internal ReferenceInternalInfo(ref T t) : base(ref t)
			{
				Header = Runtime.ReadObjHeader(ref t);
			}

			// todo: was public
			internal ObjHeader* Header { get; }

			protected override ConsoleTable ToTable()
			{
				var table = base.ToTable();

				table.AttachColumn("Object Header", Hex.ToHex(Header));
				return table;
			}
		}

		public sealed class ReferenceMetadataInfo : MetadataInfo
		{
			internal ReferenceMetadataInfo(ref T t) : base(ref t)
			{
				IsHeapPointer = GCHeap.GlobalHeap.Reference.IsHeapPointer(t);
			}

			public bool IsHeapPointer { get; }

			protected override ConsoleTable ToTable()
			{
				var table = base.ToTable();
				table.AddRow("Heap pointer", IsHeapPointer.Prettify());
				return table;
			}
		}

		public sealed class ReferenceSizeInfo : SizeInfo
		{
			private readonly string m_typeName;

			internal ReferenceSizeInfo(ref T t)
			{
				Heap              = Unsafe.HeapSize(ref t);
				BaseInstance      = Unsafe.BaseInstanceSize<T>();
				BaseFieldsUnboxed = Unsafe.BaseFieldsSize(t);
				m_typeName        = t.GetType().Name;
			}

			public int Heap              { get; }
			public int BaseInstance      { get; }
			public int BaseFieldsUnboxed { get; }

			protected override ConsoleTable ToTable()
			{
				/*var table = base.ToTable();
				table.AddRow("Heap size", Heap);
				table.AddRow("Base instance size", BaseInstance);
				table.AddRow("Base fields size", BaseFieldsSize);
				return table;*/

				var table = base.ToTable();

				// todo: if the value is boxed
				//if (m_typeName != typeof(T).Name) {
				table.AttachColumn($"Base fields size <{m_typeName}> {1}", BaseFieldsUnboxed);

				//}

				table.AttachColumn("Heap size", Heap);
				table.AttachColumn("Base instance size", BaseInstance);


				table.DetachFromColumns(Unsafe.INVALID_VALUE);
				return table;
			}
		}


		public sealed class ReferenceAddressInfo : AddressInfo
		{
			internal ReferenceAddressInfo(ref T t) : base(ref t)
			{
				Heap   = Unsafe.AddressOfHeap(ref t).Address;
				Fields = Unsafe.AddressOfHeap(ref t, OffsetType.Fields).Address;
				Header = (IntPtr) Runtime.ReadObjHeader(ref t);
				if (typeof(T).IsArray)
					HeapMisc = Unsafe.AddressOfHeap(ref t, OffsetType.ArrayData).Address;
				else if (typeof(T) == typeof(string))
					HeapMisc = Unsafe.AddressOfHeap(ref t, OffsetType.StringData).Address;
				else
					HeapMisc = IntPtr.Zero;
			}

			public IntPtr Heap   { get; }
			public IntPtr Fields { get; }
			public IntPtr Header { get; }

			/// <summary>
			///     String data if the type is a string,
			///     Array data if the type is an array
			/// </summary>
			public IntPtr HeapMisc { get; }

			protected override ConsoleTable ToTable()
			{
				var table = base.ToTable();
				table.AttachColumn("Heap", Hex.ToHex(Heap));
				table.AttachColumn("Fields", Hex.ToHex(Fields));
				table.AttachColumn("Header", Hex.ToHex(Header));


				if (typeof(T).IsArray)
					table.AttachColumn("Array data", Hex.ToHex(HeapMisc));

				else if (typeof(T) == typeof(string)) table.AttachColumn("String data", Hex.ToHex(HeapMisc));


				return table;
			}
		}
	}
}