#region

using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using RazorSharp;
using RazorSharp.Pointers;

#endregion

namespace Test.Testing.Benchmarking
{

	public unsafe class PointerBenchmarking
	{
		private Pointer<int> m_lpInt32;
		private int          m_value;
		private int*         m_lpInt32Native;

		[GlobalSetup]
		public void Setup()
		{
			m_value         = 0xFF;
			m_lpInt32       = Unsafe.AddressOf(ref m_value);
			m_lpInt32Native = (int*) Unsafe.AddressOf(ref m_value).ToPointer();
		}

		[Benchmark]
		public void ReadAsRef()
		{
			int x = m_lpInt32.Reference;
		}

		[Benchmark]
		public void WriteAsRef()
		{
			m_lpInt32.Reference = 0;
		}

		[Benchmark]
		public void ReadNative()
		{
			int x = *m_lpInt32Native;
		}

		[Benchmark]
		public void Read()
		{
			int x = m_lpInt32.Value;
		}

		[Benchmark]
		public void Write()
		{
			m_lpInt32.Value = 0;
		}

		[Benchmark]
		public void WriteNative()
		{
			*m_lpInt32Native = 0;
		}


	}

	public unsafe class PointerArithmeticBenchmarking
	{
		private int*         m_lpInt32Native;
		private Pointer<int> m_lpInt32;
		private int[]        m_rgInt32;
		private GCHandle     m_hGC;

		[GlobalSetup]
		public void Setup()
		{
			m_rgInt32       = new int[10];
			m_hGC           = GCHandle.Alloc(m_rgInt32, GCHandleType.Pinned);
			m_lpInt32       = Unsafe.AddressOfHeap(ref m_rgInt32, OffsetType.ArrayData);
			m_lpInt32Native = (int*) m_lpInt32.Address;
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			m_hGC.Free();
		}

		[Benchmark]
		public void Increment()
		{
			m_lpInt32++;
		}

		[Benchmark]
		public void IncrementNative()
		{
			m_lpInt32Native++;
		}

		[Benchmark]
		public void Decrement()
		{
			m_lpInt32--;
		}

		[Benchmark]
		public void DecrementNative()
		{
			m_lpInt32Native--;
		}
	}

	public unsafe class ReadWritePointerBenchmarking
	{
		private Pointer<int> m_lpInt32;
		private int          m_value;

		[GlobalSetup]
		public void Setup()
		{
			m_lpInt32 = Unsafe.AddressOf(ref m_value);
		}

		[Benchmark]
		public void Read()
		{
			int x = m_lpInt32.Value;
		}

		[Benchmark]
		public void Write()
		{
			m_lpInt32.Value = 0;
		}

		[Benchmark]
		public void WriteAsRef()
		{
			m_lpInt32.Reference = 0;
		}

		[Benchmark]
		public void ReadAsRef()
		{
			int x = m_lpInt32.Reference;
		}
	}

	public unsafe class ArrayPointerBenchmarking
	{
		private int*         m_lpInt32Native;
		private Pointer<int> m_lpInt32;
		private int[]        m_rgInt32;
		private GCHandle     m_hGC;

		[GlobalSetup]
		public void Setup()
		{
			m_rgInt32       = new int[10];
			m_hGC           = GCHandle.Alloc(m_rgInt32, GCHandleType.Pinned);
			m_lpInt32       = Unsafe.AddressOfHeap(ref m_rgInt32, OffsetType.ArrayData);
			m_lpInt32Native = (int*) m_lpInt32.Address;
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			m_hGC.Free();
		}

		[Benchmark]
		public void IndexNative()
		{
			for (int i = 0; i < 10; i++) {
				int x = m_lpInt32Native[i];
			}
		}

		[Benchmark]
		public void Index()
		{
			for (int i = 0; i < 10; i++) {
				int x = m_lpInt32[i];
			}
		}
	}

}