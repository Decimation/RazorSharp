namespace RazorSharp.CoreClr.Meta.Interfaces
{
	public interface IClrStructure<T>
	{
		IMetadata<T> ToMetaStructure();
	}
}