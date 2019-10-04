using System;
using System.Text;
using RazorSharp.Memory;
using RazorSharp.Memory.Components;
using RazorSharp.Memory.Pointers;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Security;
using SimpleSharp.Diagnostics;

namespace RazorSharp.Analysis
{
	internal class ObjectGuide
	{
		private IStructure Struct { get; }

		private InspectOptions Options { get; }

		private Pointer<byte> Address { get; set; }

		private object Value { get; set; }
		
		internal ObjectGuide(IStructure structure, InspectOptions options)
		{
			Struct  = structure;
			Address = Mem.Nullptr;
			Value   = null;
			Options = options;
		}

		internal void Update<T>(ref T t)
		{
			Value   = Struct.GetValue(t);
			Address = Struct.GetAddress(ref t);
			
			Conditions.AssertDebug(Value == Address.ReadAny(Value.GetType()));
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("Name: {0} | ", Struct.Name)
			  .AppendFormat("Offset: {0} | ", Struct.Offset)
			  .AppendFormat("Size: {0}", Struct.Size);

			if (Options.HasFlagFast(InspectOptions.Values)) {
				if (Value == null) {
					throw Guard.InvalidOperationFail("Value is required");
				}
					
				sb.AppendFormat(" | Value: {0}", Value);
			}

			if (Options.HasFlagFast(InspectOptions.Addresses)) {
				if (Address.IsNull) {
					throw Guard.InvalidOperationFail("Address is null");
				}
				sb.AppendFormat(" | Address: {0}", Address);
			}

			return sb.ToString();
		}
	}
}