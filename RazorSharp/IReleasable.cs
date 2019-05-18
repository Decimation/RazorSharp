namespace RazorSharp
{
	public interface IReleasable
	{
		bool IsSetup { get; set; }
		void Setup();
		void Close();
	}
}