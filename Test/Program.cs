#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Diagnostics;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
using RazorSharp.CoreClr.Structures.EE;
using RazorSharp.Memory;
using RazorSharp.Memory.Calling.Symbols.Attributes;
using RazorSharp.Native;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

#endregion


namespace Test
{
	#region

	using DWORD = UInt32;

	#endregion


	public static unsafe class Program
	{
		// todo: protect address-sensitive functions
		// todo: replace native pointers* with Pointer<T> for consistency
		// todo: RazorSharp, ClrMD, Reflection, Cecil comparison


		[ClrSymcall(Symbol = "Object::GetSize", FullyQualified = true)]
		private static int Size(this object obj)
		{
			return Constants.INVALID_VALUE;
		}


		private static void SaveObjToFile(object o, FileInfo info)
		{
			var mem = Unsafe.MemoryOf(o);

			info.Create();
			using (var stream = info.OpenWrite()) {
				stream.Write(mem, 0, mem.Length);
			}
		}

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Global.Setup();
			Clr.ClrPdb = new FileInfo(@"C:\Symbols\clr.pdb");
			Clr.Setup();


			var mt = typeof(EEClass).GetMethodTable();
			var ee = mt.Reference.EEClass;

			Console.WriteLine(ee.Reference.NativeSize);
			Console.WriteLine(ee.Reference.ComInterfaceType);

			// SHUT IT DOWN
			Symbols.Close();
			Clr.Close();
			Global.Close();
		}


		//This bypasses the restriction that you can't have a pointer to T,
		//letting you write very high-performance generic code.
		//It's dangerous if you don't know what you're doing, but very worth if you do.
		private static T Read<T>(IntPtr address)
		{
			var obj = default(T);
			var tr  = __makeref(obj);

			//This is equivalent to shooting yourself in the foot
			//but it's the only high-perf solution in some cases
			//it sets the first field of the TypedReference (which is a pointer)
			//to the address you give it, then it dereferences the value.
			//Better be 10000% sure that your type T is unmanaged/blittable...
			*(IntPtr*) (&tr) = address;

			return __refvalue(tr, T);
		}


		[DllImport(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\clr.dll")]
		private static extern void* GetCLRFunction(string str);


		private static void Dump<T>(T t, int recursivePasses = 0)
		{
			FieldInfo[] fields = t.GetType().GetMethodTableFields();

			var ct = new ConsoleTable("Field", "Type", "Value");
			foreach (var f in fields) {
				var    val = f.GetValue(t);
				string valStr;
				if (f.FieldType == typeof(IntPtr)) {
					valStr = Hex.TryCreateHex(val);
				}
				else if (val != null) {
					if (val.GetType().IsArray)
						valStr  = ((Array) val).AutoJoin(ToStringOptions.Hex);
					else valStr = val.ToString();
				}
				else {
					valStr = StringConstants.NULL_STR;
				}

				ct.AddRow(f.Name, f.FieldType.Name, valStr);
			}

			Console.WriteLine(ct.ToMarkDownString());
		}

		private static bool TryAlloc(object o, out GCHandle g)
		{
			try {
				g = GCHandle.Alloc(o, GCHandleType.Pinned);
				return true;
			}
			catch {
				g = default;
				return false;
			}
		}
	}
}