#region

using System;
using System.Reflection;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using SimpleSharp.Diagnostics;

#endregion

namespace RazorSharp.CoreClr.Meta.Virtual
{
	/// <summary>
	///     Represents a readable structure or field in memory. This is used for fields that exist in memory but
	///     don't have a <see cref="MemberInfo" />.
	///     <example><see cref="MethodTable" />, <see cref="ObjHeader" />, etc</example>
	/// </summary>
	public abstract class VirtualField : IReadableStructure
	{
		private readonly int m_offset;

		protected VirtualField(int memOffset, int offset, int size)
		{
			MemoryOffset = memOffset;
			Size         = size;
			m_offset     = offset;
		}

		protected VirtualField(int memOffset, int offset) : this(memOffset, offset, IntPtr.Size) { }

		public int Token => throw new NotSupportedException();

		public abstract string Name { get; }

		public abstract object GetValue(object value);

		public abstract Pointer<byte> GetAddress<TInstance>(ref TInstance value);

		/// <summary>
		///     Field offset
		/// </summary>
		public int Offset {
			get => m_offset;
			set => throw new NotSupportedException();
		}

		public int MemoryOffset { get; }

		public int Size { get; }

		public abstract string TypeName { get; }

		public MemberInfo Info => throw new NotSupportedException();

		protected Pointer<byte> GetAddress<TInstance>(ref TInstance value, OffsetOptions options)
		{
			Conditions.Require(!RtInfo.IsStruct(value), nameof(value));
			Unsafe.TryGetAddressOfHeap(value, options, out Pointer<byte> ptr);

			return ptr;
		}
	}
}