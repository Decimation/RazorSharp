#region

using System.Reflection;
using InlineIL;
using JetBrains.Annotations;
using RazorSharp.Interop.Utilities;

#endregion

namespace RazorSharp.Interop
{
	#region

	using SAMethodSig = StandAloneMethodSig;
	using CC = CallingConventions;

	#endregion


	public static unsafe partial class Functions
	{
		// todo: reformat

		/// <summary>
		/// Provides methods for calling native functions using the IL <c>calli</c> opcode.
		/// </summary>
		public static class Native
		{
			#region Call

			/// <summary>
			///     Calls a native function with the <c>calli</c> instruction.
			/// </summary>
			/// <param name="fn">Function address</param>
			/// <typeparam name="TRet">Return type</typeparam>
			/// <returns>A value of <typeparamref name="TRet" /></returns>
			[Native]
			public static TRet Call<TRet>(void* fn)
			{
				IL.Emit.Ldarg_0();                                         // Load arg "fn"
				IL.Emit.Conv_I();                                          // Convert arg "fn" to native
				IL.Emit.Calli(new SAMethodSig(CC.Standard,                 // Calling convention
				                              new TypeRef(typeof(TRet)))); // Return type
				return IL.Return<TRet>();
			}

			/// <summary>
			///     Calls a native function with the <c>calli</c> instruction.
			/// </summary>
			/// <param name="fn">Function address</param>
			/// <param name="arg1">Argument #1</param>
			/// <typeparam name="TRet">Return type</typeparam>
			/// <typeparam name="TArg1"><paramref name="arg1" /> type</typeparam>
			/// <returns>A value of <typeparamref name="TRet" /></returns>
			[Native]
			public static TRet Call<TRet, TArg1>(void* fn, TArg1 arg1)
			{
				IL.Emit.Ldarg_1();                                          // Load arg "arg1"
				IL.Emit.Ldarg_0();                                          // Load arg "fn"
				IL.Emit.Conv_I();                                           // Convert arg "fn" to native
				IL.Emit.Calli(new SAMethodSig(CC.Standard,                  // Calling convention
				                              new TypeRef(typeof(TRet)),    // Return type
				                              new TypeRef(typeof(TArg1)))); // Arg "arg1" type #1
				return IL.Return<TRet>();
			}

			/// <summary>
			///     Calls a native function with the <c>calli</c> instruction.
			/// </summary>
			/// <param name="fn">Function address</param>
			/// <param name="arg1">Argument #1</param>
			/// <param name="arg2">Argument #2</param>
			/// <typeparam name="TRet">Return type</typeparam>
			/// <typeparam name="TArg1"><paramref name="arg1" /> type</typeparam>
			/// <typeparam name="TArg2"><paramref name="arg2" /> type</typeparam>
			/// <returns>A value of <typeparamref name="TRet" /></returns>
			[Native]
			public static TRet Call<TRet, TArg1, TArg2>(void* fn, TArg1 arg1, TArg2 arg2)
			{
				IL.Emit.Ldarg_1();                                          // Load arg "arg1"
				IL.Emit.Ldarg_2();                                          // Load arg "arg2"
				IL.Emit.Ldarg_0();                                          // Load arg "fn"
				IL.Emit.Conv_I();                                           // Convert arg "fn" to native
				IL.Emit.Calli(new SAMethodSig(CC.Standard,                  // Calling convention
				                              new TypeRef(typeof(TRet)),    // Return type
				                              new TypeRef(typeof(TArg1)),   // Arg "arg1" type #1
				                              new TypeRef(typeof(TArg2)))); // Arg "arg2" type #2
				return IL.Return<TRet>();
			}

			/// <summary>
			///     Calls a native function with the <c>calli</c> instruction.
			/// </summary>
			/// <param name="fn">Function address</param>
			/// <param name="arg1">Argument #1</param>
			/// <typeparam name="TRet">Return type</typeparam>
			/// <returns>A value of <typeparamref name="TRet" /></returns>
			[Native]
			public static TRet Call<TRet>(void* fn, void* arg1)
			{
				IL.Emit.Ldarg_1();                                          // Load arg "arg1"
				IL.Emit.Ldarg_0();                                          // Load arg "fn"
				IL.Emit.Conv_I();                                           // Convert arg "fn" to native
				IL.Emit.Calli(new SAMethodSig(CC.Standard,                  // Calling convention
				                              new TypeRef(typeof(TRet)),    // Return type
				                              new TypeRef(typeof(void*)))); // Arg "arg1" type #1
				return IL.Return<TRet>();
			}

			/// <summary>
			///     Calls a native function with the <c>calli</c> instruction.
			/// </summary>
			/// <param name="fn">Function address</param>
			/// <param name="arg1">Argument #1</param>
			/// <param name="arg2">Argument #2</param>
			/// <typeparam name="TRet">Return type</typeparam>
			/// <returns>A value of <typeparamref name="TRet" /></returns>
			[Native]
			public static TRet Call<TRet>(void* fn, void* arg1, void* arg2)
			{
				IL.Emit.Ldarg_1();                                          // Load arg "arg1"
				IL.Emit.Ldarg_2();                                          // Load arg "arg2"
				IL.Emit.Ldarg_0();                                          // Load arg "fn"
				IL.Emit.Conv_I();                                           // Convert arg "fn" to native
				IL.Emit.Calli(new SAMethodSig(CC.Standard,                  // Calling convention
				                              new TypeRef(typeof(TRet)),    // Return type
				                              new TypeRef(typeof(void*)),   // Arg "arg1" type #1
				                              new TypeRef(typeof(void*)))); // Arg "arg2" type #2
				return IL.Return<TRet>();
			}

			/// <summary>
			///     Calls a native function with the <c>calli</c> instruction.
			/// </summary>
			/// <param name="fn">Function address</param>
			/// <param name="arg1">Argument #1</param>
			/// <param name="arg2">Argument #2</param>
			/// <param name="arg3">Argument #3</param>
			/// <typeparam name="TRet">Return type</typeparam>
			/// <typeparam name="TArg3"><paramref name="arg3"/> type</typeparam>
			/// <returns>A value of <typeparamref name="TRet" /></returns>
			[Native]
			public static TRet Call<TRet, TArg3>(void* fn, void* arg1, void* arg2, TArg3 arg3)
			{
				IL.Emit.Ldarg_1();                                          // Load arg "arg1"
				IL.Emit.Ldarg_2();                                          // Load arg "arg2"
				IL.Emit.Ldarg_3();                                          // Load arg "arg3"
				IL.Emit.Ldarg_0();                                          // Load arg "fn"
				IL.Emit.Conv_I();                                           // Convert arg "fn" to native
				IL.Emit.Calli(new SAMethodSig(CC.Standard,                  // Calling convention
				                              new TypeRef(typeof(TRet)),    // Return type
				                              new TypeRef(typeof(void*)),   // Arg "arg1" type #1
				                              new TypeRef(typeof(void*)),   // Arg "arg2" type #2
				                              new TypeRef(typeof(TArg3)))); // Arg "arg3" type #3
				return IL.Return<TRet>();
			}

			/// <summary>
			///     Calls a native function with the <c>calli</c> instruction.
			/// </summary>
			/// <param name="fn">Function address</param>
			/// <param name="arg1">Argument #1</param>
			/// <param name="arg2">Argument #2</param>
			/// <typeparam name="TRet">Return type</typeparam>
			/// <typeparam name="TArg2"><paramref name="arg2" /> type</typeparam>
			/// <returns>A value of <typeparamref name="TRet" /></returns>
			[Native]
			public static TRet Call<TRet, TArg2>(void* fn, void* arg1, TArg2 arg2)
			{
				IL.Emit.Ldarg_1();                                          // Load arg "arg1"
				IL.Emit.Ldarg_2();                                          // Load arg "arg2"
				IL.Emit.Ldarg_0();                                          // Load arg "fn"
				IL.Emit.Conv_I();                                           // Convert arg "fn" to native
				IL.Emit.Calli(new SAMethodSig(CC.Standard,                  // Calling convention
				                              new TypeRef(typeof(TRet)),    // Return type
				                              new TypeRef(typeof(void*)),   // Arg "arg1" type #1
				                              new TypeRef(typeof(TArg2)))); // Arg "arg2" type #2
				return IL.Return<TRet>();
			}

			/// <summary>
			///     Calls a native function with the <c>calli</c> instruction.
			/// </summary>
			/// <param name="fn">Function address</param>
			/// <param name="arg1">Argument #1</param>
			/// <param name="arg2">Argument #2</param>
			/// <param name="arg3">Argument #3</param>
			/// <typeparam name="TRet">Return type</typeparam>
			/// <typeparam name="TArg2"><paramref name="arg2" /> type</typeparam>
			/// <typeparam name="TArg3"><paramref name="arg3" /> type</typeparam>
			/// <returns>A value of <typeparamref name="TRet" /></returns>
			[Native]
			public static TRet Call<TRet, TArg2, TArg3>(void* fn, void* arg1, TArg2 arg2,
			                                            TArg3 arg3)
			{
				IL.Emit.Ldarg_1();                                          // Load arg "arg1"
				IL.Emit.Ldarg_2();                                          // Load arg "arg2"
				IL.Emit.Ldarg_3();                                          // Load arg "arg3"
				IL.Emit.Ldarg_0();                                          // Load arg "fn"
				IL.Emit.Conv_I();                                           // Convert arg "fn" to native
				IL.Emit.Calli(new SAMethodSig(CC.Standard,                  // Calling convention
				                              new TypeRef(typeof(TRet)),    // Return type
				                              new TypeRef(typeof(void*)),   // Arg "arg1" type #1
				                              new TypeRef(typeof(TArg2)),   // Arg "arg2" type #2
				                              new TypeRef(typeof(TArg3)))); // Arg "arg3" type #3
				return IL.Return<TRet>();
			}

			#endregion

			#region CallVoid

			/// <summary>
			///     Calls a native function with the <c>calli</c> instruction.
			/// </summary>
			/// <param name="fn">Function address</param>
			/// <param name="arg1">Argument #1</param>
			/// <typeparam name="T1"><paramref name="arg1" /> type</typeparam>
			[Native]
			public static void CallVoid<T1>(void* fn, T1 arg1)
			{
				IL.Emit.Ldarg_1();                                       // Load arg "arg1"
				IL.Emit.Ldarg_0();                                       // Load arg "fn"
				IL.Emit.Conv_I();                                        // Convert arg "fn" to native
				IL.Emit.Calli(new SAMethodSig(CC.Standard,               // Calling convention
				                              new TypeRef(typeof(void)), // Return type
				                              new TypeRef(typeof(T1)))); // Arg "arg1" type #1
			}

			/// <summary>
			///     Calls a native function with the <c>calli</c> instruction.
			/// </summary>
			/// <param name="fn">Function address</param>
			/// <param name="arg1">Argument #1</param>
			[Native]
			public static void CallVoid(void* fn, void* arg1)
			{
				IL.Emit.Ldarg_1();                                          // Load arg "arg1"
				IL.Emit.Ldarg_0();                                          // Load arg "fn"
				IL.Emit.Conv_I();                                           // Convert arg "fn" to native
				IL.Emit.Calli(new SAMethodSig(CC.Standard,                  // Calling convention
				                              new TypeRef(typeof(void)),    // Return type
				                              new TypeRef(typeof(void*)))); // Arg "arg1" type #1
			}

			/// <summary>
			///     Calls a native function with the <c>calli</c> instruction.
			/// </summary>
			/// <param name="fn">Function address</param>
			/// <param name="arg1">Argument #1</param>
			/// <param name="arg2">Argument #2</param>
			[Native]
			public static void CallVoid(void* fn, void* arg1, void* arg2)
			{
				IL.Emit.Ldarg_1();                                          // Load arg "arg1"
				IL.Emit.Ldarg_2();                                          // Load arg "arg2"
				IL.Emit.Ldarg_0();                                          // Load arg "fn"
				IL.Emit.Conv_I();                                           // Convert arg "fn" to native
				IL.Emit.Calli(new SAMethodSig(CC.Standard,                  // Calling convention
				                              new TypeRef(typeof(void)),    // Return type
				                              new TypeRef(typeof(void*)),   // Arg "arg1" type #1
				                              new TypeRef(typeof(void*)))); // Arg "arg2" type #2
			}

			#endregion

			#region CallReturnPointer

			/// <summary>
			///     Calls a native function with the <c>calli</c> instruction.
			/// </summary>
			/// <param name="fn">Function address</param>
			[Native]
			public static void* CallReturnPointer(void* fn)
			{
				IL.Emit.Ldarg_0();                                          // Load arg "fn"
				IL.Emit.Conv_I();                                           // Convert arg "fn" to native
				IL.Emit.Calli(new SAMethodSig(CC.Standard,                  // Calling convention
				                              new TypeRef(typeof(void*)))); // Return type
				return IL.ReturnPointer();
			}

			/// <summary>
			///     Calls a native function with the <c>calli</c> instruction.
			/// </summary>
			/// <param name="fn">Function address</param>
			/// <param name="arg1">Argument #1</param>
			[Native]
			public static void* CallReturnPointer(void* fn, void* arg1)
			{
				IL.Emit.Ldarg_1();                                          // Load arg "arg1"
				IL.Emit.Ldarg_0();                                          // Load arg "fn"
				IL.Emit.Conv_I();                                           // Convert arg "fn" to native
				IL.Emit.Calli(new SAMethodSig(CC.Standard,                  // Calling convention
				                              new TypeRef(typeof(void*)),   // Return type
				                              new TypeRef(typeof(void*)))); // Arg "arg1" type #1
				return IL.ReturnPointer();
			}

			/// <summary>
			///     Calls a native function with the <c>calli</c> instruction.
			/// </summary>
			/// <param name="fn">Function address</param>
			/// <param name="arg1">Argument #1</param>
			/// <param name="arg2">Argument #2</param>
			[Native]
			public static void* CallReturnPointer(void* fn, void* arg1, void* arg2)
			{
				IL.Emit.Ldarg_1();                                          // Load arg "arg1"
				IL.Emit.Ldarg_2();                                          // Load arg "arg2"
				IL.Emit.Ldarg_0();                                          // Load arg "fn"
				IL.Emit.Conv_I();                                           // Convert arg "fn" to native
				IL.Emit.Calli(new SAMethodSig(CC.Standard,                  // Calling convention
				                              new TypeRef(typeof(void*)),   // Return type
				                              new TypeRef(typeof(void*)),   // Arg "arg1" type #1
				                              new TypeRef(typeof(void*)))); // Arg "arg2" type #2
				return IL.ReturnPointer();
			}

			/// <summary>
			///     Calls a native function with the <c>calli</c> instruction.
			/// </summary>
			/// <param name="fn">Function address</param>
			/// <param name="arg1">Argument #1</param>
			/// <param name="arg2">Argument #2</param>
			[Native]
			public static void* CallReturnPointer<TArg2>(void* fn, void* arg1, TArg2 arg2)
			{
				IL.Emit.Ldarg_1();                                          // Load arg "arg1"
				IL.Emit.Ldarg_2();                                          // Load arg "arg2"
				IL.Emit.Ldarg_0();                                          // Load arg "fn"
				IL.Emit.Conv_I();                                           // Convert arg "fn" to native
				IL.Emit.Calli(new SAMethodSig(CC.Standard,                  // Calling convention
				                              new TypeRef(typeof(void*)),   // Return type
				                              new TypeRef(typeof(void*)),   // Arg "arg1" type #1
				                              new TypeRef(typeof(TArg2)))); // Arg "arg2" type #2
				return IL.ReturnPointer();
			}

			#endregion
		}
	}
}