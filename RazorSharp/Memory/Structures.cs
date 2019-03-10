using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using RazorCommon;
using RazorCommon.Utilities;
using RazorSharp.Clr;
using RazorSharp.Clr.Meta;
using RazorSharp.Utilities;

namespace RazorSharp.Memory
{
	internal static class Structures
	{
		internal static void ReorganizeQ(Type t, params int[] offsets)
		{
			if (!LayoutMismatch(t)) {
				Global.Log.Debug("Verified layout integrity of {Name}",t.Name);
				return;
			}
			var fields = t.GetFields(ReflectionUtil.ALL_INSTANCE_FLAGS).ToList();
			Conditions.Assert(fields.Count == offsets.Length);

			fields = fields.OrderBy(Runtime.ReadOffset).ToList();

			for (int i = 0; i < offsets.Length; i++) {
				Runtime.WriteOffset(fields[i], offsets[i]);
				Console.WriteLine("{0} {1}", fields[i].Name, Runtime.ReadOffset(fields[i]));
			}
		}

		private static bool LayoutMismatch(Type t)
		{
			(FieldInfo[] fields, FieldOffsetAttribute[] attributes) =
				Runtime.GetAnnotatedFields<FieldOffsetAttribute>(t);
			var metaFields = fields.Select(x => new MetaField(x.GetFieldDesc())).ToArray();
			
			for (int i = 0; i < fields.Length; i++) {
				if (metaFields[i].Offset != attributes[i].Value) {
					return true;
				}
			}

			return false;
		}

		internal static void ReorganizeAuto(Type t)
		{
			if (!LayoutMismatch(t)) {
				Global.Log.Debug("Verified layout integrity of {Name}",t.Name);
				return;
			}
			else {
				Global.Log.Debug("Correcting layout of {Name}",t.Name);
			}
			
			(FieldInfo[] fields, FieldOffsetAttribute[] attributes) =
				Runtime.GetAnnotatedFields<FieldOffsetAttribute>(t);


			var prevField  = fields[0];
			var prevMField = new MetaField(prevField.GetFieldDesc());

			var unions = new Dictionary<int, MetaField[]>();

			for (int i = 1; i < fields.Length; i++) {
				
				var currentField  = fields[i];
				var currentMField = new MetaField(currentField.GetFieldDesc());
				int expectedOffset = prevMField.Size + prevMField.Offset;


				if (!(i + 1 >= fields.Length)) {
					var nextField = fields[i + 1];
					var nextMField = new MetaField(nextField.GetFieldDesc());

					if (currentMField.Offset == nextMField.Offset) {
						var unionFieldsGroup = fields
						                      .Select(x => new MetaField(x.GetFieldDesc()))
						                      .Where(x => x.Offset == currentMField.Offset)
						                      .ToArray();

						var key = currentMField.Offset;
						if (!unions.ContainsKey(key)) {
							unions.Add(key, unionFieldsGroup);
							fields     = Collections.RemoveAt(fields, i);
							attributes = Collections.RemoveAt(attributes, i);
						}

						if (key != expectedOffset) {
							foreach (var field in unions[key]) {
								var oldOfs = field.Offset;
								field.Offset = expectedOffset;
								Global.Log.Debug("Adjusted union field {Name} ({OldOfs} -> {NewOfs})",
								                 field.Name, oldOfs, expectedOffset);
							}
						}
					}
				}
				else {
					if (expectedOffset != currentMField.Offset && !unions.ContainsKey(expectedOffset)) {
						var oldOfs = currentMField.Offset;
						currentMField.Offset = expectedOffset;

						Global.Log.Debug("Adjusted field {Name} ({OldOfs} -> {NewOfs})",
						                 currentField.Name, oldOfs, expectedOffset);
					}
				}


				prevField  = currentField;
				prevMField = new MetaField(currentField.GetFieldDesc());
			}

			Global.Log.Debug("Detected unions: {Count}", unions.Count);

			foreach (var union in unions) {
				Global.Log.Debug("Union: {Str} {Key}", Collections.CreateString(union.Value), union.Key);
			}
		}

		internal static void ReorganizeSequential(Type t)
		{
			var fields    = t.GetFieldDescs().OrderBy(x => x.Reference.Offset).ToList();
			var prevField = fields[0];

			for (int i = 1; i < fields.Count; i++) {
				var currentField = fields[i];
				currentField.Reference.Offset = prevField.Reference.Size;
				var diff = currentField.Reference.Offset - prevField.Reference.Size;
				Conditions.Assert(diff == 0, diff.ToString());
				prevField = currentField;
			}
		}

		internal static void ReorganizeSequential<T>()
		{
			Memory.Structures.ReorganizeSequential(typeof(T));
		}
	}
}