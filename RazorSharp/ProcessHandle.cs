using System;
using System.Diagnostics;
using System.Text;
using RazorSharp.Memory;
using RazorSharp.Native;
using RazorSharp.Pointers;

namespace RazorSharp
{
	// todo: WIP
	internal class ProcessHandle : IDisposable
	{
		private readonly Pointer<byte> m_handlePtr;
		private readonly IntPtr        m_procHandle;
		private readonly SigScanner    m_sigScanner;


		public ProcessHandle(string name) : this(Process.GetProcessesByName(name)[0]) { }

		public ProcessHandle(Process proc)
		{
			Process      = proc;
			m_procHandle = Kernel32.OpenProcess(proc);
			m_handlePtr  = m_procHandle;
			m_sigScanner = new SigScanner(proc);

			SelectModule(Process.MainModule);
		}

		public Process Process { get; }

		public void Dispose()
		{
			Debug.Assert(Kernel32.CloseHandle(m_procHandle));
		}


		public void SelectModule(string name)
		{
			m_sigScanner.SelectModule(name);
		}

		public void SelectModule(ProcessModule module)
		{
			m_sigScanner.SelectModule(module);
		}

		/// <summary>
		///     Reads a 16-bit encoded string (<see cref="StringTypes.UniStr" />) (<see cref="Char" />)
		/// </summary>
		/// <param name="addr">Address of the string</param>
		/// <param name="len">Number of bytes to read to retrieve the string from</param>
		/// <returns></returns>
		public unsafe string ReadString16(Pointer<char> addr, int len = 256)
		{
			fixed (byte* b = ReadBytes(addr.Address, len)) {
				Pointer<char> mem = b;
				return mem.ReadString(StringTypes.UniStr);
			}
		}

		/// <summary>
		///     Reads an 8-bit encoded string (<see cref="StringTypes.AnsiStr" />) (<see cref="Byte" />)
		/// </summary>
		/// <param name="addr">Address of the string</param>
		/// <param name="len">Number of bytes to read to retrieve the string from</param>
		/// <returns></returns>
		public unsafe string ReadString(Pointer<byte> addr, int len = 256)
		{
			fixed (byte* b = ReadBytes(addr.Address, len)) {
				Pointer<byte> mem = b;
				return mem.ReadString(StringTypes.AnsiStr);
			}
		}

		public void WriteString(Pointer<byte> addr, string s)
		{
			WriteBytes(addr.Address, Encoding.UTF8.GetBytes(s));
		}

		public void WriteString16(Pointer<char> addr, string s)
		{
			WriteBytes(addr.Address, Encoding.Unicode.GetBytes(s));
		}

		public void WriteBytes(Pointer<byte> addr, byte[] mem)
		{
			int dwSize               = mem.Length;
			int numberOfBytesWritten = 0;

			// Write the memory
			Trace.Assert(Kernel32.WriteProcessMemory(m_procHandle, addr.Address, mem, dwSize,
				ref numberOfBytesWritten));

			Trace.Assert(numberOfBytesWritten == dwSize);
		}

		public byte[] ReadBytes(Pointer<byte> addr, int count)
		{
			var   mem               = new byte[count];
			ulong numberOfBytesRead = 0;


			// Read the memory
			Trace.Assert(Kernel32.ReadProcessMemory(m_procHandle, addr.Address, mem, (uint) count, ref
				numberOfBytesRead));

			Trace.Assert(numberOfBytesRead == (ulong) count);
			return mem;
		}

		public void Write<T>(Pointer<T> addr, T value)
		{
			int numberOfBytesWritten = 0;
			int dwSize               = Unsafe.SizeOf<T>();

			// Write the memory
			Trace.Assert(Kernel32.WriteProcessMemory(m_procHandle, addr.Address, Unsafe.AddressOf(ref value).Address,
				dwSize, ref numberOfBytesWritten));

			Trace.Assert(numberOfBytesWritten == dwSize);
		}

		public T Read<T>(Pointer<T> addr)
		{
			T     t                 = default;
			ulong numberOfBytesRead = 0;
			uint  size              = (uint) Unsafe.SizeOf<T>();

			// Read the memory
			Trace.Assert(Kernel32.ReadProcessMemory(m_procHandle, addr.Address, Unsafe.AddressOf(ref t).Address, size,
				ref numberOfBytesRead));

			Trace.Assert(numberOfBytesRead == size);


			return t;
		}


		public override string ToString()
		{
			return string.Format("Process: {0} ({1})", Process.ProcessName, Process.Id);
		}
	}
}