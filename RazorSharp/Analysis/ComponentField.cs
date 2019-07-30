using System;
using RazorSharp.CoreClr;
using RazorSharp.CoreClr.Meta;

namespace RazorSharp.Analysis
{
	/// <summary>
	/// Describes the component the <see cref="ComponentField"/> represents
	/// </summary>
	public enum ComponentType
	{
		/// <summary>
		/// The <see cref="ComponentField"/> represents a <see cref="char"/> in a <see cref="string"/>
		/// </summary>
		StringChar,
		
		/// <summary>
		/// The <see cref="ComponentField"/> represents an element in an array
		/// </summary>
		ArrayElement,
	}
	
	/// <summary>
	/// Represents a component in a dynamically-sized object - an array or string.
	/// </summary>
	public class ComponentField : MemoryField
	{
		/// <summary>
		/// Index in the array/string
		/// </summary>
		public int Index { get; }

		/// <summary>
		/// Type of this <see cref="ComponentField"/>
		/// </summary>
		public ComponentType Type { get; }
		
		public int BaseOffset { get; }

		public override int Offset => BaseOffset + (Index * Size);

		public override string Name {
			get {
				string s;
				switch (Type) {
					case ComponentType.StringChar:
						s = "Char";
						break;
					case ComponentType.ArrayElement:
						s = "Element";
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				return String.Format("({0} #{1})", s, Index);
			}
		}

		internal ComponentField(MetaType type, int index) : base(type.ComponentSize)
		{
			Index = index;
			Type  = type.IsString ? ComponentType.StringChar : ComponentType.ArrayElement;
			BaseOffset = type.IsString ? Offsets.StringOverhead : Offsets.ArrayOverhead;
			
			// int ofs = baseOfs + (i * elemSize);
		}
	}
}