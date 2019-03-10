using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using RazorSharp.Clr.Meta;
using RazorSharp.Clr.Structures;
using RazorSharp.Clr.Structures.EE;
using RazorSharp.Clr.Structures.HeapObjects;
using RazorSharp.Utilities;

namespace RazorSharp.Clr
{
	internal static class Clr
	{
		internal static readonly Type[] ClrTypes =
		{
			typeof(FieldDesc), typeof(MethodDesc), typeof(MethodDescChunk), typeof(MethodTable),
			typeof(ArrayObject), typeof(HeapObject), typeof(StringObject),typeof(EEClass)
		};

		internal static void Setup()
		{
			int[] offsets = new[] {0, IntPtr.Size, IntPtr.Size + sizeof(uint)};
			Memory.Structures.ReorganizeQ(typeof(FieldDesc),offsets: offsets);
			
			
		}
		
		internal static void Reorganize()
		{
			
			foreach (var type in ClrTypes) {
				Memory.Structures.ReorganizeAuto(type);
			}
		}
	}
}