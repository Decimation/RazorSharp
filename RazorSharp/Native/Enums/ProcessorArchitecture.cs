// ReSharper disable InconsistentNaming

namespace RazorSharp.Native.Enums
{
	public enum ProcessorArchitecture : ushort
	{
		/// <summary>
		///     x64 (AMD or Intel)
		/// </summary>
		Amd64 = 9,

		/// <summary>
		///     ARM
		/// </summary>
		Arm = 5,

		/// <summary>
		///     ARM64
		/// </summary>
		Arm64 = 12,

		/// <summary>
		///     Intel Itanium-based
		/// </summary>
		IA64 = 6,

		/// <summary>
		///     x86
		/// </summary>
		Intel = 0,

		Unknown = 0xFFFF
	}
}