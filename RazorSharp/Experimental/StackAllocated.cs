using System;
using System.Collections;
using System.Diagnostics;
using RazorCommon;
using RazorCommon.Extensions;
using RazorSharp.Analysis;

namespace RazorSharp.Experimental
{
	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	public unsafe struct StackAllocated<T> where T : class
	{
		private readonly byte* m_stackPtr;

		// 'Heap' pointer points to m_stackPtr + sizeof(ObjHeader)
		private T m_dummy;

		public T Value {
			get => m_dummy;
			set => m_dummy = ReAllocateRefOnStack(ref value);
		}

		private T ReAllocateRefOnStack(ref T refValue)
		{
			var refMem  = Unsafe.MemoryOf(ref refValue);
			var allocSize = Unsafe.BaseInstanceSize<T>();
			Debug.Assert(refMem.Length == allocSize);

			for (int i = 0; i < allocSize; i++) {
				m_stackPtr[i] = refMem[i];
			}

			// Skip over ObjHeader
			Unsafe.WriteReference(ref refValue, m_stackPtr + IntPtr.Size);
			return refValue;
		}

		public StackAllocated(byte* stackPtr)
		{
			m_stackPtr = stackPtr;

			T dummy = Activator.CreateInstance<T>();
			m_dummy = dummy;
			m_dummy = ReAllocateRefOnStack(ref dummy);

		}

		public override string ToString()
		{
			var table = new ConsoleTable("Field", "Value");
			table.AddRow("Value", Value);
			table.AddRow("Stack", Hex.ToHex(m_stackPtr));
			table.AddRow("Dummy heap pointer", Hex.ToHex(Unsafe.AddressOfHeap(ref m_dummy)));
			return table.ToMarkDownString();
		}
	}

}