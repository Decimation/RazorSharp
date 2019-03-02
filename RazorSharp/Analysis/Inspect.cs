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
		
		public static void Heap<T, TAs>(T t, ToStringOptions options = DEFAULT) where T : class
		{
			Console.WriteLine(HeapString<T, TAs>(t, options));
		}

		public static void Heap<T>(T t, ToStringOptions options = DEFAULT) where T : class
		{
			Console.WriteLine(HeapString(t, options));
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
			var objHeaderMem = Runtime.ReadObjHeader(t).Reinterpret<byte>().CopyOut(IntPtr.Size);
			var objHeaderStr = CreateInternalRowEntry(objHeaderMem);

			// MethodTable*
			var methodTablePtrMem = Unsafe.AddressOfHeap(t, OffsetType.None).CopyOut(IntPtr.Size);
			var methodTablePtrStr = CreateInternalRowEntry(methodTablePtrMem);

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

				row.Add(CreateInternalRowEntry(dataAddr.CopyOut(-IntPtr.Size, IntPtr.Size)));
				for (int i = 0; i < arraySize; i += elemSize) {
					table.AddColumn(String.Format("Index {0}", i / elemSize));
					row.Add(CreateRowEntry(dataAddr.CopyOut(i, elemSize)));
				}
			}
			else {
				Conditions.RequiresNotNull(fields);

				foreach (var f in fields) {
					var rowPtr = f.GetAddress(ref t);
					int size   = f.Size;
					dataAddr += size;
					row.Add(CreateRowEntry(rowPtr.CopyOut(size)));
				}

				var endAddr = (addr - IntPtr.Size) + heapSize;

				if (dataAddr < endAddr) {
					var ptrDiff = endAddr - dataAddr;

					table.AddColumn("...");
					row.Add(CreateRowEntry(dataAddr.CopyOut(ptrDiff.ToInt32())));
				}
			}

			string CreateInternalRowEntry(byte[] mem)
			{
				return InterpretInternal? CreateRowEntry(mem): Collections.CreateString(" | ",mem, options);
			}
			
			string CreateRowEntry(byte[] mem)
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
					return Collections.CreateString(" | ",RazorConvert.ConvertArray<TAs>(mem), options);
				}

				return Collections.CreateString(" | ",mem, options);
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