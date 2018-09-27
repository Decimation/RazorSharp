#region

using System.Collections.Generic;
using JetBrains.Annotations;
using RazorSharp.CLR;

#endregion

namespace RazorSharp.Memory
{

	public struct PatternPair
	{
		private readonly string m_szFnName;
		private readonly byte[] m_rgPattern;
		private readonly long   m_offset;

		public string Name    => m_szFnName;
		public byte[] Pattern => m_rgPattern;
		public long   Offset  => m_offset;


		internal PatternPair(string szName, byte[] rgPattern, long offset)
		{
			m_szFnName  = szName;
			m_rgPattern = rgPattern;
			m_offset    = offset;
		}
	}

	public class Cache<TType>
	{
		/// <summary>
		///     The pattern for each <see cref="PatternPair" /> corresponds to the index in this list
		/// </summary>
		private readonly List<PatternPair> m_rgPatternPairs;

		/// <summary>
		///     The indices of this correspond to <see cref="m_rgPatternPairs" />
		/// </summary>
		private readonly byte[][] m_rgPatternMap;

		private int m_runningIndex;

		public List<PatternPair> Pairs => m_rgPatternPairs;


		public void AddCache(string fnName, bool isGet = false, long ofsGuess = 0)
		{
			if (isGet) {
				fnName = SpecialNames.NameOfGetPropertyMethod(fnName);
			}

			PatternPair pair = new PatternPair(fnName, m_rgPatternMap[m_runningIndex++], ofsGuess);
			m_rgPatternPairs.Add(pair);
		}

		/// <summary>
		/// </summary>
		/// <param name="patterns">
		///     2D <see cref="System.Byte" /> array of patterns. Sequential to the <see cref="PatternPair" />
		/// </param>
		public Cache([NotNull] byte[][] patterns)
		{
			m_rgPatternMap   = patterns;
			m_runningIndex   = 0;
			m_rgPatternPairs = new List<PatternPair>();
		}


	}

}