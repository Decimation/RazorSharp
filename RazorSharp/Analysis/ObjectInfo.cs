using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Metadata.Enums;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Security;
using SimpleSharp.Diagnostics;
// ReSharper disable ReturnTypeCanBeEnumerable.Local

namespace RazorSharp.Analysis
{
	public class ObjectInfo
	{
		private object m_value;

		public IStructure[] Structures { get; private set; }

		public MetaType Type { get; }

		public object Value {
			get => m_value;
			set {
				if (value.GetType() != Type || value == null) {
					throw Guard.InvalidOperationFail(nameof(value));
				}

				m_value = value;
			}
		}

		public Pointer<byte> Address { get; private set; }

		private InspectOptions Options { get; }


		internal ObjectInfo(MetaType t, InspectOptions options)
		{
			Type    = t;
			Options = options;
		}

		private ObjectInfo Copy()
		{
			var info = new ObjectInfo(Type, Options)
			{
				Address    = Address,
				Structures = Structures,
				Value      = Value
			};
			return info;
		}

		internal ObjectInfo WithFields()
		{
			var fields = Type.Fields.Where(f => !f.IsStatic).ToArray();

			var structures = new List<IStructure>(fields.Length);

			structures.AddRange(fields);

			Structures = structures.ToArray();

			return this;
		}

		internal ObjectInfo WithMemoryFields()
		{
			var structures = new List<IStructure>();

			if (Type.IsStringOrArray) {
				Array array = null;

				if (Type.IsString) {
					string s = m_value as string;
				
					// ReSharper disable once AssignNullToNotNullAttribute
					array = s.ToArray();
				}

				if (Type.IsArray) {
					array = m_value as Array;

					structures.Add(new MemoryField("(Array length)", 0, sizeof(int)));

					if (Mem.Is64Bit) {
						structures.Add(new PaddingField(sizeof(int)));
					}
				}

				for (int i = 0; i < array.Length; i++) {
					structures.Add(new ComponentField(Type, i));
				}
			}
			


			// We assume that fields are already added
			var buf = Structures.ToList();
			buf.AddRange(structures);
			Structures = buf.ToArray();

			return this;
		}

		internal ObjectInfo WithPaddingFields()
		{
			var padding = GetPadding();
			
			// We assume that fields are already added
			var buf = Structures.ToList();
			buf.AddRange(padding);
			Structures = buf.OrderBy(f=>f.Offset).ToArray();
			
			return this;
		}
		
		private MemoryField[] GetPadding()
		{
			var padding = new List<MemoryField>();
			var nextOffsetOrSize = Type.InstanceFieldsSize;
			var memFields        = Type.Fields.Where(f=>!f.IsStatic).ToArray();

			for (int i = 0; i < memFields.Length; i++) {
				// start padding

				if (i != memFields.Length - 1) {
					nextOffsetOrSize = Type.Fields[i + 1].Offset;
				}

				int nextSectOfsCandidate = Type.Fields[i].Offset + Type.Fields[i].Size;

				if (nextSectOfsCandidate < nextOffsetOrSize) {
					int padSize = nextOffsetOrSize - nextSectOfsCandidate;

					padding.Add(new PaddingField(nextSectOfsCandidate, padSize));
				}

				// end padding
			}

			return padding.ToArray();
		}


		public void Dump()
		{
			foreach (var structure in Structures) {
				var sb = new StringBuilder();
				sb.AppendFormat("Name: {0} | ", structure.Name)
				  .AppendFormat("Offset: {0} | ", structure.Offset)
				  .AppendFormat("Size: {0}", structure.Size);

				if (Options.HasFlagFast(InspectOptions.Values)) {
					sb.AppendFormat(" | Value: {0}", structure.GetValue(m_value));
				}


				Console.WriteLine(sb);
			}
		}

		public override string ToString()
		{
			return base.ToString();
		}
	}
}