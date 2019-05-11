using System;
using System.Reflection;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory.Pointers;

namespace RazorSharp.CoreClr.Meta
{
	/// <summary>
	/// Represents a readable structure or field in memory. This is used for fields that exist in memory but
	/// don't have a <see cref="MemberInfo"/>.
	/// <example><see cref="MethodTable"/>, <see cref="ObjHeader"/></example>
	/// </summary>
	public abstract class TransientField : IReadableStructure
	{
		public int Token => Constants.INVALID_VALUE;

		public abstract string Name { get; }

		public abstract object GetValue(object value);

		public abstract Pointer<byte> GetAddress<TInstance>(ref TInstance value);

		public int Offset {
			get => Constants.INVALID_VALUE;
			set => throw new InvalidOperationException();
		}

		public int MemoryOffset { get; }

		public int Size { get; }

		public abstract string TypeName { get; }

		internal TransientField(int memOffset) : this(memOffset, IntPtr.Size)
		{
			
		}
		
		internal TransientField(int memOffset, int size)
		{
			MemoryOffset = memOffset;
			Size = size;
		}
	}
}