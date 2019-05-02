#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RazorCommon;
using RazorCommon.Diagnostics;
using RazorCommon.Strings;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities;

#endregion

namespace RazorSharp.Analysis
{
	public static class Inspect
	{
		// todo: maybe add type name/info to labels

		#region Helper methods

		private const string JOIN_BAR = " | ";

		private static ConsoleTable StackHeapTypeTable<T>(ref T t, ToStringOptions options)
		{
			var type = typeof(T).GetMetaType();
			Conditions.Require(!typeof(T).IsValueType);
			Pointer<T> addr = Unsafe.AddressOf(ref t);
			var row = new List<object>
			{
				CreateRowEntry<byte>(addr.CopyOutBytes(IntPtr.Size), options)
			};
			var table = new ConsoleTable(String.Format("Pointer to {0}", type.Name));
			table.AddRow(row.ToArray());
			return table;
		}

		private static ConsoleTable StackValueTypeTable<T>(ref T t, ToStringOptions options)
		{
			var type = typeof(T).GetMetaType();
			Conditions.Require(typeof(T).IsValueType);
			List<MetaField> fields     = type.Fields.Where(x => !x.IsStatic).ToList();
			List<string>    fieldNames = fields.Select(x => x.Name).ToList();
			var             table      = new ConsoleTable(fieldNames.ToArray());
			Pointer<T>      addr       = Unsafe.AddressOf(ref t);
			var             row        = new List<object>();


			foreach (var f in fields) {
				Pointer<byte> rowPtr = f.GetAddress(ref t);
				int           size   = f.Size;
				addr += size;
				row.Add(CreateRowEntry<byte>(rowPtr.CopyOut(size), options));
			}

			table.AddRow(row.ToArray());
			return table;
		}

		private static string CreateInternalRowEntry(byte[] mem, ToStringOptions options)
		{
			return /*InterpretInternal
				? CreateRowEntry<byte>(mem, options)
				: */mem.AutoJoin(JOIN_BAR, options);
		}

		private static string CreateRowEntry<TAs>(byte[] mem, ToStringOptions options)
		{
			switch (options) {
				case ToStringOptions.None:
					break;
				case ToStringOptions.Hex:
					break;
				case ToStringOptions.ZeroPadHex:
					break;
				case ToStringOptions.PrefixHex:
					break;
			}

			// Byte is default for TAs
			if (typeof(TAs) != typeof(byte)) {
				return Conversions.ConvertArray<TAs>(mem).AutoJoin(JOIN_BAR, options);
			}


			return mem.AutoJoin(JOIN_BAR, options);
		}

		#endregion

		#region Layout

		public static void Layout<T>(T value) where T : class
		{
			Console.WriteLine(LayoutString(value));
		}

		public static void Layout<T>(ref T value)
		{
			Console.WriteLine(LayoutString(ref value));
		}

		public static void Layout<T>()
		{
			Console.WriteLine(LayoutString<T>());
		}

		public static string LayoutString<T>(T value) where T : class
		{
			return LayoutString(ref value);
		}

		public static string LayoutString<T>()
		{
			return LayoutTable<T>().ToString();
		}

		public static string LayoutString<T>(ref T value)
		{
			var         table     = LayoutTable<T>();
			var         type      = typeof(T).GetMetaType();
			MetaField[] fields    = type.InstanceFields.ToArray();
			int         lim       = fields.Length;
			var         addresses = new object[lim];
			var         values    = new object[lim];

			Conditions.Assert(lim == type.NumInstanceFields);

			for (int i = 0; i < lim; i++) {
				var field = fields[i];

				addresses[i] = field.GetAddress(ref value).ToString(PointerFormat.FORMAT_PTR);
				values[i]    = field.GetValue(value) ?? StringConstants.NULL_STR;
			}


			table.AttachEnd("Value", values)
			     .AttachEnd("Address", addresses);

			return table.ToString();
		}

		public static ConsoleTable LayoutTable<T>()
		{
			var         type   = typeof(T).GetMetaType();
			MetaField[] fields = type.InstanceFields.ToArray();
			var         table  = new ConsoleTable("Name", "Size", "Type", "Offset");

			foreach (var field in fields) {
				table.AddRow(field.Name, field.Size, field.FieldType.Name, field.Offset);
			}

			return table;
		}

		#endregion

		#region Stack

		public static void Stack<T>(ref T value, ToStringOptions options = Hex.DEFAULT)
		{
			Console.WriteLine(StackString(ref value, options));
		}

		public static string StackString<T>(ref T value, ToStringOptions options = Hex.DEFAULT)
		{
			Pointer<T> addr = Unsafe.AddressOf(ref value);
			int        size = Unsafe.SizeOf<T>();
			var        type = typeof(T).GetMetaType();

			var table = type.RuntimeType.IsValueType
				? StackValueTypeTable(ref value, options)
				: StackHeapTypeTable(ref value, options);


			var sb = new StringBuilder();
			sb.AppendFormat("{0} @ {1:P}\n", value.GetType().Name, addr);
			sb.AppendFormat("Size: {0}\n", size);
			sb.Append(table);
			return sb.ToString();
		}

		#endregion

		#region Heap

		public static string HeapString<T>(T value, ToStringOptions options = Hex.DEFAULT) where T : class
		{
			// Sizes
			Pointer<byte> addr     = Unsafe.AddressOfHeap(value, OffsetOptions.HEADER);
			int           heapSize = Unsafe.HeapSize(value);

			// Type info
			var             type       = value.GetType().GetMetaType();
			List<MetaField> fields     = type.IsArray ? null : type.Fields.Where(x => !x.IsStatic).ToList();
			List<string>    fieldNames = fields == null ? new List<string>() : fields.Select(x => x.Name).ToList();
			fieldNames.Insert(0, "(Header)");
			fieldNames.Insert(1, "(MethodTable*)");
			var table = new ConsoleTable(fieldNames.ToArray());

			// Object header
			byte[] objHeaderMem = Runtime.ReadObjHeader(value).CopyOutBytes(IntPtr.Size);
			string objHeaderStr = CreateInternalRowEntry(objHeaderMem, options);

			// MethodTable*
			byte[] methodTablePtrMem = Unsafe.AddressOfHeap(value, OffsetOptions.NONE).CopyOut(IntPtr.Size);
			string methodTablePtrStr = CreateInternalRowEntry(methodTablePtrMem, options);

			var row = new List<object>
			{
				objHeaderStr, methodTablePtrStr
			};

			var           offsetType = type.IsArray ? OffsetOptions.ARRAY_DATA : OffsetOptions.FIELDS;
			Pointer<byte> dataAddr   = Unsafe.AddressOfHeap(value, offsetType);

			if (type.IsArray) {
				table.AddColumn("(length)");

				// IntPtr 1: Header
				// IntPtr 2: MethodTable*
				// IntPtr 3: Length and padding (64bit)

				//int arraySize = heapSize - IntPtr.Size * 3;
				int arraySize = heapSize - Offsets.OffsetToArrayData;
				int elemSize  = type.ComponentSize;

				row.Add(CreateInternalRowEntry(dataAddr.CopyOut(-IntPtr.Size, IntPtr.Size), options));
				for (int i = 0; i < arraySize; i += elemSize) {
					table.AddColumn(String.Format("(Index {0})", i / elemSize));
					row.Add(CreateRowEntry<byte>(dataAddr.CopyOut(i, elemSize), options));
				}
			}
			else {
				Conditions.NotNull(fields, nameof(fields));

				foreach (var f in fields) {
					Pointer<byte> rowPtr = f.GetAddress(ref value);
					int           size   = f.Size;
					dataAddr += size;
					row.Add(CreateRowEntry<byte>(rowPtr.CopyOut(size), options));
				}

				Pointer<byte> endAddr = addr - IntPtr.Size + heapSize;

				// Surplus memory
				if (dataAddr < endAddr) {
					int ptrDiff = (endAddr - dataAddr).ToInt32();

					if (type.IsString) {
						for (int i = 0; i < ptrDiff; i++) {
							table.AddColumn(String.Format("(Character {0})", i + 2));
							row.Add(CreateRowEntry<byte>(dataAddr.CopyOut(sizeof(char)), options));
							dataAddr += sizeof(char);
						}
					}
					else {
						table.AddColumn("(...)");
						row.Add(CreateRowEntry<byte>(dataAddr.CopyOut(ptrDiff), options));
					}
				}
			}


			// Add the rows
			table.AddRow(row.ToArray());

			var sb = new StringBuilder();
			sb.AppendFormat("{0} @ {1:P}\n", value.GetType().Name, addr);
			sb.AppendFormat("Base size: {0}\n", type.BaseSize);
			sb.AppendFormat("Heap size: {0}\n", heapSize);
			sb.Append(table);
			return sb.ToString();
		}


		public static void Heap<T>(T value, ToStringOptions options = Hex.DEFAULT) where T : class
		{
			Console.WriteLine(HeapString(value, options));
		}

		#endregion
	}
}