namespace RazorSharp.CoreClr.Meta
{
	public enum MetaInfoType
	{
		/// <summary>
		/// Member is a <see cref="MetaField"/>
		/// </summary>
		FIELD,
		
		/// <summary>
		/// Member is a <see cref="MetaIL"/>
		/// </summary>
		IL,
		
		/// <summary>
		/// Member is a <see cref="MetaMethod"/>
		/// </summary>
		METHOD,
		
		/// <summary>
		/// Member is a <see cref="MetaType"/>
		/// </summary>
		TYPE
	}
}