namespace RazorSharp.Pointers
{

	public static class PointerSettings
	{


		static PointerSettings()
		{
			RetainTagValue = false;
		}

		/// <summary>
		///     <para>
		///         When <c>true</c>, <see cref="TaggedPointer{T}.Tag" /> will retain its value
		///         when the pointer <see cref="TaggedPointer{T}.Pointer" /> is changed.
		///     </para>
		///     <para>
		///         When <c>false</c>, <see cref="TaggedPointer{T}.Tag" /> will be set to <c>0</c>
		///         when the pointer <see cref="TaggedPointer{T}.Pointer" /> is changed.
		///     </para>
		/// </summary>
		public static bool RetainTagValue { get; set; }

		#region Format specifiers

		internal const string FMT_O   = "O";
		internal const string FMT_P   = "P";
		internal const string FMT_I   = "I";
		internal const string FMT_B   = "B";
		internal const string NULLPTR = "(null)";

		#endregion

	}

}