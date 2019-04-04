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
using RazorSharp.CoreClr.Structures;
using RazorSharp.Pointers;
using RazorSharp.Utilities;

#endregion

namespace RazorSharp.Analysis
{
	public static class Inspect
	{
		private const string JOIN_BAR = " | ";

		/// <summary>
		///     Whether to interpret the <see cref="MethodTable" />, <see cref="ObjHeader" />, and other internal
		///     data structures as the TAs parameters.
		/// </summary>
		public static bool InterpretInternal { get; set; } = false;

		#region Layout

		public static void Layout<T>(T t) where T : class
		{
			Console.WriteLine(LayoutString(t));
		}

		public static void Layout<T>(ref T t)
		{
			Console.WriteLine(LayoutString(ref t));
		}

		public static void Layout<T>()
		{
			Console.WriteLine(LayoutString<T>());
		}

		public static string LayoutString<T>(T t) where T : class
		{
			throw new NotImplementedException();
		}

		public static string LayoutString<T>()
		{
			return LayoutTable<T>().ToMarkDownString();
		}

		public static string LayoutString<T>(ref T t)
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

				addresses[i] = field.GetAddress(ref t).ToString("P");
				values[i]    = field.GetValue(t); // todo
			}

			table.Attach(1, "Address", addresses)
			     .Attach("Value", values);

			return table.ToMarkDownString();
		}

		private static ConsoleTable LayoutTable<T>()
		{
			var         type   = typeof(T).GetMetaType();
			MetaField[] fields = type.InstanceFields.ToArray();
			var         table  = new ConsoleTable("Offset", "Size", "Type", "Name");

			foreach (var field in fields) {
				table.AddRow(field.Offset,
				             field.Size,
				             field.FieldType.Name,
				             field.Name);
			}

			return table;
		}

		#endregion

		#region Stack

		public static void Stack<T>(ref T t, ToStringOptions options = Hex.DEFAULT)
		{
			Console.WriteLine(StackString(ref t, options));
		}

		public static string StackString<T>(ref T t, ToStringOptions options = Hex.DEFAULT)
		{
			Pointer<T> addr = Unsafe.AddressOf(ref t);
			int        size = Unsafe.SizeOf<T>();
			var        type = typeof(T).GetMetaType();

			var table = type.RuntimeType.IsValueType
				? StackValueTypeTable(ref t, options)
				: StackHeapTypeTable(ref t, options);


			var sb = new StringBuilder();
			sb.AppendFormat("{0} @ {1:P}\n", t.GetType().Name, addr);
			sb.AppendFormat("Size: {0}\n", size);
			sb.Append(table.ToMarkDownString());
			return sb.ToString();
		}

		#endregion

		#region Heap

		public static string HeapString<T, TAs>(T t, ToStringOptions options = Hex.DEFAULT) where T : class
		{
			// Sizes
			Pointer<byte> addr     = Unsafe.AddressOfHeap(t);
			int           heapSize = Unsafe.HeapSize(t);

			// Type info
			var             type       = typeof(T).GetMetaType();
			List<MetaField> fields     = type.IsArray ? null : type.Fields.Where(x => !x.IsStatic).ToList();
			List<string>    fieldNames = fields == null ? new List<string>() : fields.Select(x => x.Name).ToList();
			fieldNames.Insert(0, "Header");
			fieldNames.Insert(1, "MethodTable*");
			var table = new ConsoleTable(fieldNames.ToArray());

			// Object header
			byte[] objHeaderMem = Runtime.ReadObjHeader(t).CopyOutBytes(IntPtr.Size);
			string objHeaderStr = CreateInternalRowEntry(objHeaderMem, options);

			// MethodTable*
			byte[] methodTablePtrMem = Unsafe.AddressOfHeap(t, OffsetType.None).CopyOut(IntPtr.Size);
			string methodTablePtrStr = CreateInternalRowEntry(methodTablePtrMem, options);

			var row = new List<object>
			{
				objHeaderStr, methodTablePtrStr
			};

			var           offsetType = type.IsArray ? OffsetType.ArrayData : OffsetType.Fields;
			Pointer<byte> dataAddr   = Unsafe.AddressOfHeap(t, offsetType);

			if (type.IsArray) {
				table.AddColumn("length");

				// IntPtr 1: Header
				// IntPtr 2: MethodTable*
				// IntPtr 3: Length and padding (64bit)
				int arraySize = heapSize - IntPtr.Size * 3;
				int elemSize  = type.ComponentSize;

				row.Add(CreateInternalRowEntry(dataAddr.CopyOut(-IntPtr.Size, IntPtr.Size), options));
				for (int i = 0; i < arraySize; i += elemSize) {
					table.AddColumn(String.Format("Index {0}", i / elemSize));
					row.Add(CreateRowEntry<TAs>(dataAddr.CopyOut(i, elemSize), options));
				}
			}
			else {
				Conditions.NotNull(fields, nameof(fields));

				foreach (var f in fields) {
					Pointer<byte> rowPtr = f.GetAddress(ref t);
					int           size   = f.Size;
					dataAddr += size;
					row.Add(CreateRowEntry<TAs>(rowPtr.CopyOut(size), options));
				}

				Pointer<byte> endAddr = addr - IntPtr.Size + heapSize;

				if (dataAddr < endAddr) {
					Pointer<byte> ptrDiff = endAddr - dataAddr;

					table.AddColumn("...");
					row.Add(CreateRowEntry<TAs>(dataAddr.CopyOut(ptrDiff.ToInt32()), options));
				}
			}


			// Add the rows
			table.AddRow(row.ToArray());

			var sb = new StringBuilder();
			sb.AppendFormat("{0} @ {1:P}\n", t.GetType().Name, addr);
			sb.AppendFormat("Base size: {0}\n", type.BaseSize);
			sb.AppendFormat("Heap size: {0}\n", heapSize);
			sb.Append(table.ToMarkDownString());
			return sb.ToString();
		}

		public static string HeapString<T>(T t, ToStringOptions options = Hex.DEFAULT) where T : class
		{
			return HeapString<T, byte>(t, options);
		}

		public static void Heap<T, TAs>(T t, ToStringOptions options = Hex.DEFAULT) where T : class
		{
			Console.WriteLine(HeapString<T, TAs>(t, options));
		}

		public static void Heap<T>(T t, ToStringOptions options = Hex.DEFAULT) where T : class
		{
			Console.WriteLine(HeapString(t, options));
		}

		#endregion

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
			return InterpretInternal
				? CreateRowEntry<byte>(mem, options)
				: mem.AutoJoin(JOIN_BAR, options);
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
	}
}