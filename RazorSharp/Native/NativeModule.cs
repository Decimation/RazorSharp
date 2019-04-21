using System;
using System.Linq;
using System.Runtime.CompilerServices;
using RazorSharp.Memory;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native.Win32;

namespace RazorSharp.Native
{
	public struct NativeModule : IDisposable
	{
		private readonly (string m_fileName, Pointer<byte> m_baseAddr) m_value;

		public string FileName => m_value.m_fileName;

		public string Name => FileName.Split('\\').Last();

		public Pointer<byte> BaseAddress => m_value.m_baseAddr;


		public static NativeModule NullModule => new NativeModule(null, null);

		public static bool operator ==(NativeModule l, NativeModule r)
		{
			return l.m_value == r.m_value;
		}

		public static bool operator !=(NativeModule l, NativeModule r)
		{
			return l.m_value != r.m_value;
		}


		internal NativeModule(string fileName, Pointer<byte> baseAddr)
		{
			m_value = (fileName, baseAddr);
		}

		/// <summary>
		/// This should only be freed if it was loaded using <see cref="Modules.LoadModule"/>
		/// (<see cref="ProcessApi.LoadLibrary"/>)
		/// </summary>
		public void Dispose()
		{
			ProcessApi.FreeLibrary(BaseAddress.Address);
		}

		public override string ToString()
		{
			return String.Format("Name: \"{0}\", base address: {1:P}", Name, BaseAddress);
		}
	}
}