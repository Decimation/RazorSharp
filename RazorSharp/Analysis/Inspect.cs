using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RazorCommon;
using RazorSharp.CLR;
using RazorSharp.CLR.Meta;
using RazorSharp.CLR.Structures;
using RazorSharp.Utilities;

namespace RazorSharp.Analysis
{
	public static class Inspect
	{
		/// <summary>
		/// Whether to interpret the <see cref="MethodTable"/>, <see cref="ObjHeader"/>, and other internal
		/// data structures as the TAs parameters.
		/// </summary>
		public static bool InterpretInternal { get; set; } = false;

		// todo
		public static bool SmartInterpret { get; set; }

		private const ToStringOptions DEFAULT = ToStringOptions.Hex;

		public static void Stack<T>(ref T t, ToStringOptions options = DEFAULT)
		{
			Console.WriteLine(StackString(ref t, options));
		}

		public static string StackString<T>(ref T t, ToStringOptions options = DEFAULT)
		{
			var addr = Unsafe.AddressOf(ref t);
			var size = Unsafe.SizeOf<T>();
			var type = Meta.GetType<T>();

			var table    = type.RuntimeType.IsValueType ? StackValueType(ref t, options) : StackHeapType(ref t, options);
			

			var sb = new StringBuilder();
			sb.AppendFormat("{0} @ {1:P}\n", t.GetType().Name, addr);
			sb.AppendFormat("Size: {0}\n", size);
			sb.Append(table.ToMarkDownString());
			return sb.ToString();
		}

		private static ConsoleTable StackHeapType<T>(ref T t, ToStringOptions options)
		{
			var type = Meta.GetType<T>();
			Conditions.RequiresClassType<T>();
			var addr = Unsafe.AddressOf(ref t);
			var row = new List<object>();
			row.Add(CreateRowEntry<byte>(addr.CopyOutBytes(IntPtr.Size), options));
			var table = new ConsoleTable(string.Format("Pointer to {0}",type.Name));
			table.AddRow(row.ToArray());
			return table;
		}
		private static ConsoleTable StackValueType<T>(ref T t, ToStringOptions options)
		{
			
			var type = Meta.GetType<T>();
			Conditions.RequiresValueType<T>();
			var fields     = type.Fields.Where(x => !x.IsStatic).ToList();
			var fieldNames = fields.Select(x => x.Name).ToList();
			var table      = new ConsoleTable(fieldNames.ToArray());
			var addr       = Unsafe.AddressOf(ref t);
			var row        = new List<object>();

			
			
			foreach (var f in fields) {
				var rowPtr = f.GetAddress(ref t);
				int size   = f.Size;
				addr += size;
				row.Add(CreateRowEntry<byte>(rowPtr.CopyOut(size), options));
			}

			table.AddRow(row.ToArray());
			return table;
		}

		public static void Heap<T, TAs>(T t, ToStringOptions options = DEFAULT) where T : class
		{
			Console.WriteLine(HeapString<T, TAs>(t, options));
		}

		public static void Heap<T>(T t, ToStringOptions options = DEFAULT) where T : class
		{
			Console.WriteLine(HeapString(t, options));
		}

		private static string CreateInternalRowEntry(byte[] mem, ToStringOptions options)
		{
			return InterpretInternal
				? CreateRowEntry<byte>(mem, options)
				: Collections.CreateString(" | ", mem, options);
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
				case ToStringOptions.Decimal:
					break;
				default:
					break;
			}

			// Byte is default for TAs
			if (typeof(TAs) != typeof(byte)) {
				return Collections.CreateString(" | ", RazorConvert.ConvertArray<TAs>(mem), options);
			}

			return Collections.CreateString(" | ", mem, options);
		}

		public static string HeapString<T, TAs>(T t, ToStringOptions options = DEFAULT) where T : class
		{
			// Sizes
			var addr     = Unsafe.AddressOfHeap(t);
			var heapSize = Unsafe.HeapSize(t);

			// Type info
			var type       = Meta.GetType<T>();
			var fields     = type.IsArray ? null : type.Fields.Where(x => !x.IsStatic).ToList();
			var fieldNames = fields == null ? new List<string>() : fields.Select(x => x.Name).ToList();
			fieldNames.Insert(0, "Header");
			fieldNames.Insert(1, "MethodTable*");
			var table = new ConsoleTable(fieldNames.ToArray());

			// Object header
			var objHeaderMem = Runtime.ReadObjHeader(t).CopyOutBytes(IntPtr.Size);
			var objHeaderStr = CreateInternalRowEntry(objHeaderMem, options);

			// MethodTable*
			var methodTablePtrMem = Unsafe.AddressOfHeap(t, OffsetType.None).CopyOut(IntPtr.Size);
			var methodTablePtrStr = CreateInternalRowEntry(methodTablePtrMem, options);

			var row = new List<object>
			{
				objHeaderStr, methodTablePtrStr
			};

			var offsetType = type.IsArray ? OffsetType.ArrayData : OffsetType.Fields;
			var dataAddr   = Unsafe.AddressOfHeap(t, offsetType);

			if (type.IsArray) {
				table.AddColumn("length");

				// IntPtr 1: Header
				// IntPtr 2: MethodTable*
				// IntPtr 3: Length and padding (64bit)
				int arraySize = heapSize - (IntPtr.Size * 3);
				int elemSize  = type.ComponentSize;

				row.Add(CreateInternalRowEntry(dataAddr.CopyOut(-IntPtr.Size, IntPtr.Size), options));
				for (int i = 0; i < arraySize; i += elemSize) {
					table.AddColumn(String.Format("Index {0}", i / elemSize));
					row.Add(CreateRowEntry<TAs>(dataAddr.CopyOut(i, elemSize), options));
				}
			}
			else {
				Conditions.RequiresNotNull(fields);

				foreach (var f in fields) {
					var rowPtr = f.GetAddress(ref t);
					int size   = f.Size;
					dataAddr += size;
					row.Add(CreateRowEntry<TAs>(rowPtr.CopyOut(size), options));
				}

				var endAddr = (addr - IntPtr.Size) + heapSize;

				if (dataAddr < endAddr) {
					var ptrDiff = endAddr - dataAddr;

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

		public static string HeapString<T>(T t, ToStringOptions options = DEFAULT) where T : class
		{
			return HeapString<T, byte>(t, options);
		}
	}
}