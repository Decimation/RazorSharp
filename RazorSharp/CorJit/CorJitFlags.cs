using System;
using RazorSharp.CorJit;

namespace RazorSharp
{
	public class CorJitFlags
	{
		private CorJitFlag _corJitFlags;

		public CorJitFlags()
		{
			_corJitFlags = 0;
		}

		public CorJitFlags(CorJitFlag corJitFlags)
		{
			Set(corJitFlags);
		}

		public void Reset()                 => _corJitFlags = 0;
		public void Set(CorJitFlag    flag) => _corJitFlags |= (CorJitFlag) ((UInt32) 1 << (Int32) flag);
		public void Clear(CorJitFlag  flag) => _corJitFlags &= ~(CorJitFlag) ((UInt32) 1 << (Int32) flag);
		public void Add(CorJitFlag    flag) => _corJitFlags |= flag;
		public void Remove(CorJitFlag flag) => _corJitFlags &= ~flag;
		public bool IsSet(CorJitFlag  flag) => _corJitFlags.HasFlag(flag);
		public bool IsEmpty()               => _corJitFlags == 0;
	}
}