#region

using System;
using System.Diagnostics;
using RazorSharp.CoreClr.Structures;
using RazorSharp.Memory.Pointers;
using RazorSharp.Native;
using RazorSharp.Native.Images;
using RazorSharp.Native.Win32;
using SimpleSharp;
using SimpleSharp.Utilities;

#endregion

namespace RazorSharp.Memory
{
	public unsafe class AddressInfo
	{
		public AddressInfo(Pointer<byte> ptr)
		{
			Address = ptr;


			IsOnStack         = Mem.IsOnStack(Address);
			IsInHeap          = GlobalHeap.IsHeapPointer(ptr);
			IsInUnmanagedHeap = Mem.IsInUnmanagedHeap(ptr);

			Page        = Kernel32.VirtualQuery(ptr.Address);
			Module      = Modules.FromAddress(ptr);
			Segment     = Segments.GetSegment(ptr, Module?.FileName);
			IsAllocated = AllocHelper.IsAllocated(ptr);
		}

		public Pointer<byte> Address { get; }

		public bool IsInSegment => Segment != default;

		public ImageSectionInfo Segment { get; }

		public bool IsInModule => Module != null;

		public ProcessModule Module { get; }

		public bool IsInPage => Page != default;

		public MemoryBasicInformation Page { get; }

		public bool IsAllocated { get; }

		public bool IsOnStack { get; }

		public bool IsInHeap { get; }

		public bool IsInUnmanagedHeap { get; }


		public override string ToString()
		{
			var table = new ConsoleTable("Info", "Value");
			table.AddRow("Address", Address);

			table.AddRow("Is in segment", IsInSegment.Prettify());
			if (IsInSegment) {
				table.AddRow("Segment", Segment);
			}

			table.AddRow("Is in module", IsInModule.Prettify());
			if (IsInModule) {
				table.AddRow("Module", Module);
			}

			table.AddRow("Is in page", IsInPage.Prettify());
			if (IsInPage) {
				table.AddRow("Page", Page);
			}

			table.AddRow("Is on stack", IsOnStack.Prettify());
			table.AddRow("In in heap", IsInHeap.Prettify());
			table.AddRow("Is in unmanaged heap", IsInUnmanagedHeap.Prettify());
			table.AddRow("Is allocated", IsAllocated.Prettify());
			
			return table.ToString();
		}
	}
}