using System;
using System.Collections.Generic;
using System.Linq;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities;
using SimpleSharp;

namespace RazorSharp.Analysis
{
	public static unsafe class Gadget
	{
		public static LayoutInfo Layout(Type type, GadgetOptions options)
		{
			var fields = type.GetMetaType().InstanceFields.ToArray();
			var info   = new LayoutInfo();


			info.AddFieldNames(fields);


			if (options.HasFlag(GadgetOptions.FieldSizes)) {
				info.AttachColumn("Size", fields.Select(f => (object) f.Size).ToArray());
			}

			if (options.HasFlag(GadgetOptions.FieldOffsets)) {
				info.AttachColumn("Offset", fields.Select(f => (object) f.Offset).ToArray());
			}

			if (options.HasFlag(GadgetOptions.FieldTypes)) {
				info.AttachColumn("Type", fields.Select(f => (object) f.FieldType.Name).ToArray());
			}
			
			if (options.HasFlag(GadgetOptions.InternalStructures)) {
				if (Runtime.IsStruct(type)) {
					throw new InvalidOperationException("Structs do not have internal structures");
				}

				var objHeaderRow = new List<KeyValuePair<string,object>>(info.Table.Columns.Count);

				objHeaderRow.Add((new KeyValuePair<string, object>("Name", "Object Header")));
				
				if (options.HasFlagFast(GadgetOptions.FieldOffsets)) {
					objHeaderRow.Add(new KeyValuePair<string, object>("Offset", -IntPtr.Size));
				}

				if (options.HasFlagFast(GadgetOptions.FieldSizes)) {
					objHeaderRow.Add(new KeyValuePair<string, object>("Size", sizeof(ObjHeader)));
				}

				if (options.HasFlagFast(GadgetOptions.FieldTypes)) {
					objHeaderRow.Add(new KeyValuePair<string, object>("Type", nameof(ObjHeader)));
				}

				objHeaderRow.TrimExcess();
				
				info.Table.AddRowPairs(objHeaderRow.ToArray());
			}

			return info;
		}


		public static LayoutInfo Layout<T>(ref T value, GadgetOptions options)
		{
			var type   = value.GetType();
			var info   = Layout(type, options);
			var fields = type.GetMetaType().InstanceFields.ToArray();

			if (options.HasFlag(GadgetOptions.FieldAddresses)) {
				var addrList = new List<object>(fields.Length);

				foreach (var field in fields) {
					addrList.Add(field.GetAddress(ref value));
				}

				

				info.AttachColumn("Address", addrList.ToArray());
			}

			if (options.HasFlag(GadgetOptions.FieldValues)) {
				var valList = new List<object>(fields.Length);

				foreach (var field in fields) {
					valList.Add(field.GetValue(value));
				}

				info.AttachColumn("Value", valList.ToArray());
			}

			// MUST BE LAST
			if (options.HasFlag(GadgetOptions.InternalStructures)) {

				var objHeaderRow = new List<KeyValuePair<string,object>>(info.Table.Columns.Count);

				Unsafe.TryGetAddressOfHeap(value, OffsetOptions.HEADER, out var objHeader);

				if (options.HasFlagFast(GadgetOptions.FieldAddresses)) {
					
					objHeaderRow.Add(new KeyValuePair<string, object>("Address", objHeader));
				}
				
				if (options.HasFlagFast(GadgetOptions.FieldValues)) {
					objHeaderRow.Add(new KeyValuePair<string, object>("Value", objHeader.ToString("O")));
				}

				objHeaderRow.TrimExcess();
				
				info.Table.AddRowPairs(objHeaderRow.ToArray());

				Unsafe.TryGetAddressOfHeap(value, OffsetOptions.NONE, out var mtPtr);
			}


			return info;
		}

		public static LayoutInfo Layout<T>(T value, GadgetOptions options)
		{
			if (Runtime.IsStruct(value) && options.HasFlag(GadgetOptions.FieldAddresses)) {
				throw new InvalidOperationException("Use the ref-qualified method");
			}

			return Layout(ref value, options);
		}

		public static LayoutInfo Layout<T>(GadgetOptions options)
		{
			var info = Layout(typeof(T), options);


			return info;
		}
	}
}