namespace RazorSharp.CoreJit
{
	internal class CorJitFlags
	{
		private CorJitFlag _corJitFlags;

		internal CorJitFlags()
		{
			_corJitFlags = 0;
		}

		internal CorJitFlags(CorJitFlag corJitFlags)
		{
			Set(corJitFlags);
		}

		internal void Reset()
		{
			_corJitFlags = 0;
		}

		internal void Set(CorJitFlag flag)
		{
			_corJitFlags |= (CorJitFlag) ((uint) 1 << (int) flag);
		}

		internal void Clear(CorJitFlag flag)
		{
			_corJitFlags &= ~(CorJitFlag) ((uint) 1 << (int) flag);
		}

		internal void Add(CorJitFlag flag)
		{
			_corJitFlags |= flag;
		}

		internal void Remove(CorJitFlag flag)
		{
			_corJitFlags &= ~flag;
		}

		internal bool IsSet(CorJitFlag flag)
		{
			return _corJitFlags.HasFlag(flag);
		}

		internal bool IsEmpty()
		{
			return _corJitFlags == 0;
		}
	}
}