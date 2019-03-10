using System;
using RazorSharp.Clr.Structures;
using RazorSharp.Clr.Structures.EE;
using RazorSharp.Clr.Structures.HeapObjects;

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
			Memory.Structures.ReorganizeQ(typeof(FieldDesc),0,IntPtr.Size, IntPtr.Size+sizeof(uint));
		}
		
		internal static void Reorganize()
		{
			
			foreach (var type in ClrTypes) {
				Memory.Structures.ReorganizeAuto(type);
			}
		}
	}
}