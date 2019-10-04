using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;
using RazorSharp.CoreClr.Metadata.Enums;
using RazorSharp.Memory;
using RazorSharp.Memory.Components;
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

		private object Value {
			get => m_value;
			set {
				if (value.GetType() != Type || value == null) {
					throw Guard.InvalidOperationFail(nameof(value));
				}

				m_value = value;
			}
		}

		private Pointer<byte> Address { get; set; }

		private InspectOptions Options { get; }

		private InspectOptions Completed { get; set; }

		private bool HasValue => Value != null;

		private ObjectGuide[] m_guides;


		internal ObjectInfo(MetaType t, InspectOptions options)
		{
			if (options.HasFlagFast(InspectOptions.None)) {
				throw new ArgumentException();
			}
			
			Type    = t;
			Options = options;
		}

		private void HandleOption(InspectOptions options, Action fn)
		{
			if (Options.HasFlagFast(options) && !Completed.HasFlagFast(options)) {
				fn();
				Completed |= options;
			}
		}

		internal ObjectInfo Update<T>(ref T t)
		{
			Update();
			
			Value = t;
			Address = Unsafe.AddressOf(ref t).Cast();
			
			foreach (var objectGuide in m_guides) {
				objectGuide.Update(ref t);
			}

			return this;
		}
		
		internal ObjectInfo Update()
		{
			if (HasValue) {
				HandleOption(InspectOptions.MemoryFields, AddMemoryFields);
			}

			HandleOption(InspectOptions.Fields, AddFields);

			HandleOption(InspectOptions.Padding, AddPaddingFields);

			m_guides = BuildGuides();

			return this;
		}

		private void AddFields()
		{
			var fields = Type.Fields.Where(f => !f.IsStatic).ToArray();

			var structures = new List<IStructure>(fields.Length);
			structures.AddRange(fields);
			Structures = structures.ToArray();
		}

		private void AddMemoryFields()
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

				// ReSharper disable once PossibleNullReferenceException
				for (int i = 0; i < array.Length; i++) {
					structures.Add(new ComponentField(Type, i));
				}
			}


			// We assume that fields are already added
			var buf = Structures.ToList();
			buf.AddRange(structures);
			Structures = buf.ToArray();
		}

		private void AddPaddingFields()
		{
			var padding          = new List<MemoryField>();
			var nextOffsetOrSize = Type.InstanceFieldsSize;
			var memFields        = Type.Fields.Where(f => !f.IsStatic).ToArray();

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


			// We assume that fields are already added
			var buf = Structures.ToList();
			buf.AddRange(padding);
			Structures = buf.OrderBy(f => f.Offset).ToArray();
		}


		private ObjectGuide[] BuildGuides()
		{
			var dumps = new ObjectGuide[Structures.Length];
			for (int i = 0; i < dumps.Length; i++) {
				dumps[i] = new ObjectGuide(Structures[i], Options);
			}

			return dumps;
		}

		public void Dump()
		{
			foreach (var structure in m_guides) {
				Console.WriteLine(structure);
			}
		}

		public override string ToString()
		{
			return base.ToString();
		}
	}
}