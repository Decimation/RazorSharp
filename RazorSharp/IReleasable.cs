using RazorSharp.CoreClr;
using RazorSharp.Native.Symbols;

namespace RazorSharp
{
	/// <summary>
	/// <list type="bullet">
	/// <item>
	/// <description><see cref="Global"/></description>
	/// </item>
	/// <item>
	/// <description><see cref="Clr"/></description>
	/// </item>
	/// <item>
	/// <description><see cref="ModuleInitializer"/></description>
	/// </item>
	/// <item>
	/// <description><see cref="SymbolManager"/></description>
	/// </item>
	/// </list>
	/// </summary>
	internal interface IReleasable
	{
		bool IsSetup { get; set; }
		void Setup();
		void Close();
	}
}