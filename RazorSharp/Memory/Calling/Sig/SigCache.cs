using Newtonsoft.Json;

namespace RazorSharp.Memory.Calling.Sig
{
	internal class SigCache
	{
		[JsonProperty("name")]
		internal string Name { get; set; }

		[JsonProperty("opcodes64")]
		internal string Opcodes64Signature { get; set; }

		[JsonProperty("opcodes32")]
		internal string Opcodes32Signature { get; set; }

		[JsonProperty("offset64")]
		internal string Offset64String { get; set; }

		[JsonProperty("offset32")]
		internal string Offset32String { get; set; }
	}
}