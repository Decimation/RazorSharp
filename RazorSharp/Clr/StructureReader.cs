using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using RazorSharp.Clr.Structures;
using RazorSharp.Memory;
using RazorSharp.Pointers;

namespace RazorSharp.Clr
{
	public class StructureReader : BinaryReader
	{
		public StructureReader([NotNull] Stream input) : base(input) { }
		public StructureReader([NotNull] Stream input, [NotNull] Encoding encoding) : base(input, encoding) { }
		public StructureReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) { }

		
		
		public Pointer<T> ReadPointer<T>()
		{
			return Mem.Is64Bit ? ReadInt64() : ReadInt32();
		}

		public Pointer<byte> ReadPointer()
		{
			return ReadPointer<byte>();
		}
		
		public VirtualFieldDesc ReadFieldDesc()
		{
			var pMT = ReadPointer<MethodTable>();
			var dw1 = ReadUInt32();
			var dw2 = ReadUInt32();
			
			return new VirtualFieldDesc(pMT, dw1, dw2);
		}
	}
}