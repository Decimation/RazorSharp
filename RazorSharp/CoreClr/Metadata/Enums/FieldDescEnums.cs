namespace RazorSharp.CoreClr.Metadata.Enums
{
	public enum ProtectionLevel
	{
		Private           = 4,
		PrivateProtected  = 8,
		Internal          = 12,
		Protected         = 16,
		ProtectedInternal = 20,
		Public            = 24
	}
	
	internal enum MbMask
	{
		PackedMbLayoutMbMask       = 0x01FFFF,
		PackedMbLayoutNameHashMask = 0xFE0000
	}
}