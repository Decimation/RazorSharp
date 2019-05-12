using System;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;

// ReSharper disable ReturnTypeCanBeEnumerable.Global

namespace RazorSharp.CoreClr.Meta.Transient
{
	// todo: wip

	/// <summary>
	/// Represents a <see cref="string"/> or array element in heap memory.
	/// </summary>
	public class ElementField : TransientField
	{
		private ElementField(int  memOfs,      int  fieldOfs,     MetaType elem,
		                     bool isArrayElem, bool isStringElem, int      index)
			: this(memOfs, fieldOfs, elem, isArrayElem, isStringElem, index,
			       $"({(isArrayElem ? ARRAY_ELEM_NAME : STRING_ELEM_NAME)} {index})") { }

		private ElementField(int    memOfs,      int  fieldOfs,     MetaType elem,
		                     bool   isArrayElem, bool isStringElem, int      index,
		                     string name)
			: base(memOfs, fieldOfs, elem.NumInstanceFieldBytes)
		{
			IsArrayElement  = isArrayElem;
			IsStringElement = isStringElem;
			Index           = isStringElem ? index + 1 : index;
			ElementType     = elem;
			Name            = name;
			TypeName        = ElementType.Name;
		}

		// Used for array padding and length
		private ElementField(int memOfs, int fieldOfs, MetaType elemType, string name)
			: base(memOfs, fieldOfs, elemType.NumInstanceFieldBytes)
		{
			IsArrayElement  = false;
			IsStringElement = false;
			Index           = Constants.INVALID_VALUE;
			ElementType     = elemType;
			Name            = name;
			TypeName        = ElementType.Name;
		}

		/// <summary>
		/// Creates <see cref="ElementField"/>s for array length field and padding (x64)
		/// </summary>
		internal static ElementField[] CreateArrayStructures()
		{
			ElementField[] rgElem;

			if (Mem.Is64Bit) {
				rgElem = new ElementField[2];

				MetaType padElem = typeof(int);
				var padFieldOfs = padElem.NumInstanceFieldBytes;
				var padMemOfs   = IntPtr.Size + padFieldOfs;

				rgElem[1] = new ElementField(padMemOfs, padFieldOfs, padElem, ARRAY_PADDING_FIELD);
			}
			else {
				rgElem = new ElementField[1];
			}

			// Length field

			MetaType elemType = typeof(int);
			var memOfs   = (IntPtr.Size);

			rgElem[0] = new ElementField(memOfs, 0, elemType, ARRAY_LENGTH_FIELD);

			return rgElem;
		}

		internal static ElementField Create(MetaType enclosingType, int index)
		{
			if (enclosingType.IsArray) {
				var elemType = enclosingType.ElementType;
				int mul      = (index * elemType.NumInstanceFieldBytes);
				var fieldOfs = IntPtr.Size + mul;
				var memOfs   = fieldOfs + IntPtr.Size;

				return new ElementField(memOfs, fieldOfs, elemType, true, false, index);
			}

			if (enclosingType.IsString) {
				var elemType = new MetaType(typeof(char));
				var fieldOfs = sizeof(char) + sizeof(int) + (index * elemType.NumInstanceFieldBytes);
				var memOfs   = fieldOfs + IntPtr.Size;

				return new ElementField(memOfs, fieldOfs, elemType, false, true, index);
			}

			throw new Exception();
		}

		public MetaType ElementType { get; }

		public override string Name { get; }

		public override string TypeName { get; }

		public bool IsArrayElement { get; }

		public bool IsStringElement { get; }

		/// <summary>
		/// Element index
		/// </summary>
		public int Index { get; }

		private const string ARRAY_LENGTH_FIELD  = "Length";
		private const string ARRAY_PADDING_FIELD = "Padding";

		private const string ARRAY_ELEM_NAME  = "Element";
		private const string STRING_ELEM_NAME = "Character";

		public override object GetValue(object value)
		{
			switch (value) {
				case string str when IsStringElement:
					return str[Index];
				case Array rg when IsArrayElement:
					return rg.GetValue(Index);
				case Array rg:
					switch (Name) {
						case ARRAY_LENGTH_FIELD:  return rg.Length;
						case ARRAY_PADDING_FIELD: return 0;
						default:                  throw new InvalidOperationException();
					}
			}

			return null;
		}

		public override Pointer<byte> GetAddress<TInstance>(ref TInstance value)
		{
			return base.GetAddress(ref value, OffsetOptions.NONE).Add(MemoryOffset);
		}
	}
}