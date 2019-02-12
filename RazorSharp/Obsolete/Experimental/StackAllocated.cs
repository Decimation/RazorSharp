#region

using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using RazorSharp.Common;
using RazorSharp.Utilities.Exceptions;

#endregion

namespace RazorSharp.Obsolete.Experimental
{
	#region

	#endregion

	/// <summary>
	///     Creates types in stack memory.
	///     <para></para>
	///     Types that cannot be created in stack memory:
	///     <para></para>
	///     - String
	///     <para></para>
	///     - IList
	///     <para></para>
	/// </summary>
	/// <remarks>
	///     Old namespace: Experimental
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	[Obsolete]
	internal unsafe struct StackAllocated<T> where T : class
	{
		/// <summary>
		///     Types that can't be created in stack memory
		///     (out of the types that have been tested)
		/// </summary>
		private static readonly Type[] DisallowedTypes =
		{
			typeof(string),
			typeof(IList)
		};

		private readonly byte* m_stackPtr;

		// 'Heap' pointer points to m_stackPtr + sizeof(ObjHeader)
		private T m_dummy;

		public T Value {
			get => m_dummy;
			set => m_dummy = ReAllocateRefOnStack(ref value);
		}

		private T ReAllocateRefOnStack(ref T refValue)
		{
			byte[] refMem    = Unsafe.MemoryOf(refValue);
			int    allocSize = Unsafe.BaseInstanceSize<T>();
			Debug.Assert(refMem.Length == allocSize);

			for (int i = 0; i < allocSize; i++) m_stackPtr[i] = refMem[i];

			// Skip over ObjHeader
			Unsafe.WriteReference(ref refValue, m_stackPtr + IntPtr.Size);
			return refValue;
		}

		/// <summary>
		///     Use: Use stackalloc to allocate "Unsafe.BaseInstanceSize" bytes on
		///     the stack. Then pass the byte* pointer.
		/// </summary>
		/// <param name="stackPtr"></param>
		public StackAllocated(byte* stackPtr)
		{
			if (DisallowedTypes.Contains(typeof(T)))
				throw new TypeException($"Type {typeof(T).Name} cannot be created in stack memory.");

			m_stackPtr = stackPtr;

			var dummy = Activator.CreateInstance<T>();
			m_dummy = dummy;
			m_dummy = ReAllocateRefOnStack(ref dummy);
		}

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Value", Value);
			table.AddRow("Stack", Hex.ToHex(m_stackPtr));
			table.AddRow("Dummy heap pointer", Hex.ToHex(Unsafe.AddressOfHeap(ref m_dummy).Address));
			return table.ToMarkDownString();
		}
	}
}