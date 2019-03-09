#region

using System;
using System.Reflection.Emit;

#endregion

namespace RazorSharp.Clr.Fixed
{
	/// <summary>
	///     Pins an object in memory even if the type is unblittable.
	///     <remarks>
	///         <para>Source: https://www.reddit.com/r/csharp/comments/917tyq/pinning_unblittable_objects/</para>
	///     </remarks>
	/// </summary>
	public static class ObjectPinner
	{
		private static readonly Action<object, Action<object>> PinImpl = CreatePinImpl();

		private static Action<object, Action<object>> CreatePinImpl()
		{
			var method = new DynamicMethod("InvokeWhilePinnedImpl", typeof(void),
			                               new[] {typeof(object), typeof(Action<object>)}, typeof(ObjectPinner).Module);
			var il = method.GetILGenerator();

			// create a pinned local variable of type object
			// this wouldn't be valid in C#, but the runtime doesn't complain about the IL
			var local = il.DeclareLocal(typeof(object), true);


			// store first argument obj in the pinned local variable
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Stloc_0);

			// invoke the delegate
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldarg_0);
			il.EmitCall(OpCodes.Callvirt, typeof(Action<object>).GetMethod("Invoke"), null);

			il.Emit(OpCodes.Ret);

			return (Action<object, Action<object>>) method.CreateDelegate(typeof(Action<object, Action<object>>));
		}

		/// <summary>
		///     Pins an object in memory, preventing the GC from moving it.
		/// </summary>
		/// <param name="obj">Object to pin</param>
		/// <param name="action">The action during which the object will be pinned</param>
		public static void InvokeWhilePinned(object obj, Action<object> action)
		{
			PinImpl(obj, action);
		}
	}
}