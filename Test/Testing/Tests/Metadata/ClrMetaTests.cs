using System;
using System.Diagnostics;
using System.Reflection;
using RazorSharp.CLR;
using RazorSharp.CLR.Meta;
using RazorSharp.CLR.Structures;
using RazorSharp.Common;

namespace Test.Testing.Tests.Metadata
{
	public static class ClrMetaTests
	{
		public static void GC()
		{
			string s = "nil";
//			Console.WriteLine("IsInGCHeap: {0}", GCHeap.IsInGCHeap(ref s));
			Console.WriteLine("GCCount: {0}", GCHeap.GlobalHeap.Reference.GCCount);
			Console.WriteLine("IsHeapPointer: {0}", GCHeap.GlobalHeap.Reference.IsHeapPointer(s));
			Console.WriteLine("IsGCInProgress: {0}", GCHeap.GlobalHeap.Reference.IsGCInProgress());
		}

		public static void MethodTable()
		{
			var ms = Meta.GetType<string>();

			Console.WriteLine(ms);
		}

		public static void MethodDesc()
		{
			var mm = Meta.GetType(typeof(Program)).Methods["AddOp"];

			Console.WriteLine(mm);
			Console.WriteLine("IsConstructor: {0}", mm.IsConstructor);
			Console.WriteLine("Token: {0}", mm.Token);
			Console.WriteLine("IsPointingToNativeCode: {0}", mm.IsPointingToNativeCode);
			Console.WriteLine("SizeOf: {0}", mm.SizeOf);

			mm.Reset();

			Console.WriteLine("Info: {0}", mm.Info);
			Console.WriteLine("NativeCode: {0:X}", mm.NativeCode.ToInt64());
			Console.WriteLine("PreImplementedCode: {0:X}", mm.PreImplementedCode.ToInt64());
			Console.WriteLine("HasILHeader: {0}", mm.HasILHeader);
			Console.WriteLine("ILHeader: {0}", mm.GetILHeader());

			// SetILHeader
		}


		public static void FieldDesc()
		{
			var mf = Meta.GetType<string>()["m_firstChar"];


			Console.WriteLine(mf);
			Console.WriteLine("Size: {0}", mf.Size);
			Console.WriteLine("Info: {0}", mf.Info);
		}

		private static bool Compare<T>()
		{
			return Compare(Meta.GetType<T>(), typeof(T));
		}

		public static bool Compare(MetaType meta, Type t)
		{
			//
			// Type
			//

			Debug.Assert(meta.RuntimeType == t);
			Debug.Assert(meta.Token == t.MetadataToken);
			Debug.Assert(meta.Parent.RuntimeType == t.BaseType);


			//
			// Fields
			//

			FieldInfo[] fields     = Runtime.GetFields(t);
			MetaField[] metaFields = meta.Fields.ToArray();
			Debug.Assert(fields.Length == metaFields.Length);
			Collections.OrderBy(ref fields, x => x.MetadataToken);
			Collections.OrderBy(ref metaFields, x => x.Token);

			for (int i = 0; i < fields.Length; i++) {
				Debug.Assert(fields[i].MetadataToken == metaFields[i].Token);
				Debug.Assert(fields[i].DeclaringType == metaFields[i].EnclosingType);
				Debug.Assert(fields[i].FieldType == metaFields[i].FieldType);
			}

			//
			// Methods
			//

			MethodInfo[] methods     = Runtime.GetMethods(t);
			MetaMethod[] metaMethods = meta.Methods.ToArray();
			Debug.Assert(methods.Length == metaMethods.Length);
			Collections.OrderBy(ref methods, x => x.MetadataToken);
			Collections.OrderBy(ref metaMethods, x => x.Token);

			for (int i = 0; i < methods.Length; i++) {
				Debug.Assert(methods[i].MetadataToken == metaMethods[i].Token);
				Debug.Assert(methods[i].DeclaringType == metaMethods[i].EnclosingType);
			}


			return true;
		}
	}
}