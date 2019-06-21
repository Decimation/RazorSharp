using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.CoreClr.Meta.Interfaces
{
	/// <summary>
	/// Represents a readable structure or field in memory.
	/// </summary>
	public interface IReadableStructure : IMetadata
	{
		/// <summary>
		/// Gets the value of this structure.
		/// </summary>
		/// <param name="value">Instance, if needed</param>
		object GetValue(object value);

		/// <summary>
		/// Gets the address of this structure.
		/// </summary>
		Pointer<byte> GetAddress<TInstance>(ref TInstance value);

		/// <summary>
		/// Structure offset
		/// <remarks>This is <see cref="FieldDesc.Offset"/> for most types.</remarks>
		/// </summary>
		int Offset { get; set; }

		/// <summary>
		/// Memory offset relative to <see cref="Unsafe.AddressOfData{T}(ref T)"/>
		/// </summary>
		int MemoryOffset { get; }

		/// <summary>
		/// The size of this structure.
		/// </summary>
		int Size { get; }

		/// <summary>
		/// The type name of this structure.
		/// </summary>
		string TypeName { get; }
	}
}