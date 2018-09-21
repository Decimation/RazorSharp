using System.Reflection;
using RazorSharp.CLR.Structures;
using RazorSharp.Pointers;

namespace RazorSharp.CLR
{

	internal static unsafe class ReflectionUtil
	{
		internal static Pointer<byte> GetPointerForPointerField<TInstance>(Pointer<FieldDesc> pFd, ref TInstance inst)
		{
			var value = pFd.Reference.GetValue(inst);
			return Pointer.Unbox(value);
		}
	}

}