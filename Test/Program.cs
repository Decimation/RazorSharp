#region

using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;
using RazorSharp.Memory.Calling.Symbols.Attributes;
using RazorSharp.Native;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using Unsafe = System.Runtime.CompilerServices.Unsafe;

#endregion


namespace Test
{
	#region

	using DWORD = UInt32;
	using CSUnsafe = Unsafe;

	#endregion


	public static unsafe class Program
	{
#if DEBUG
#endif

		// todo: protect address-sensitive functions
		// todo: replace native pointers* with Pointer<T> for consistency
		// todo: RazorSharp, ClrMD, Reflection comparison


		[ClrSymcall(Symbol = "Object::GetSize", FullyQualified = true)]
		private static int Size(this object obj)
		{
			return Constants.INVALID_VALUE;
		}

		[HandleProcessCorruptedStateExceptions]
		public static void Main(string[] args)
		{
			Global.Setup();
			Clr.ClrPdb = new FileInfo(@"C:\Symbols\clr.pdb");
			Clr.Setup();


			const string asmStr = "RazorSharp";
			var          asm    = Assembly.Load(asmStr);

			Pointer<MethodTable> strMT    = typeof(string).GetMethodTable();
			byte[]               ptrBytes = BitConverter.GetBytes(strMT.Address.ToInt64());

			var ai = new AddressInfo(strMT.Cast<byte>());
			Console.WriteLine(ai.Module);

			string foo = "foo";
			var    ai2 = new AddressInfo(RazorSharp.Unsafe.AddressOfHeap(foo));
			Console.WriteLine(ai2.Segment);

			Pointer<byte> fn  = Clr.GetClrFunctionAddress("MethodDesc::SetStableEntryPointInterlocked");
			var           ai3 = new AddressInfo(fn);
			Console.WriteLine(ai3.Segment);


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

		private static T add<T>(T a, T b)
		{
			if (a is int && b is int) {
				int c = __refvalue(__makeref(a), int);
				c += __refvalue(__makeref(b), int);
				return __refvalue(__makeref(c), T);
			}

			return default;
		}

		private static void foo<T>(ref T value)
		{
			//This is the ONLY way to treat value as int, without boxing/unboxing objects
			if (value is int) {
				__refvalue(__makeref(value), int) = 1;
			}
			else {
				value = default;
			}
		}


		private static OpCode[] GetAllOpCodes()
		{
			var         opCodeType   = typeof(OpCodes);
			FieldInfo[] opCodeFields = opCodeType.GetFields(BindingFlags.Public | BindingFlags.Static);

			var rgOpCodes = new OpCode[opCodeFields.Length];
			for (int i = 0;
				i < rgOpCodes.Length;
				i++) {
				rgOpCodes[i] = (OpCode) opCodeFields[i].GetValue(null);
			}

			return rgOpCodes;
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
						valStr  = Collections.CreateString((Array) val, ToStringOptions.Hex);
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