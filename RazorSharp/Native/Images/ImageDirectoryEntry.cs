namespace RazorSharp.Native.Enums.Images
{
	internal enum ImageDirectoryEntry : ushort
	{
		EXPORT    = 0, // Export Directory
		IMPORT    = 1, // Import Directory
		RESOURCE  = 2, // Resource Directory
		EXCEPTION = 3, // Exception Directory
		SECURITY  = 4, // Security Directory
		BASERELOC = 5, // Base Relocation Table
		DEBUG     = 6, // Debug Directory

		// COPYRIGHT      =  7,  // (X86 usage)
		ARCHITECTURE   = 7,  // Architecture Specific Data
		GLOBALPTR      = 8,  // RVA of GP
		TLS            = 9,  // TLS Directory
		LOAD_CONFIG    = 10, // Load Configuration Directory
		BOUND_IMPORT   = 11, // Bound Import Directory in headers
		IAT            = 12, // Import Address Table
		DELAY_IMPORT   = 13, // Delay Load Import Descriptors
		COM_DESCRIPTOR = 14  // COM Runtime descriptor
	}                        // end enum ImageDirectoryEntry
}