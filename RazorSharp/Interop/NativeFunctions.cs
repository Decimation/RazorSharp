#region

using System.Reflection;
using InlineIL;
using JetBrains.Annotations;

#endregion

namespace RazorSharp.Interop
{
	#region

	using UI = UsedImplicitlyAttribute;
	using SAMethodSig = StandAloneMethodSig;
	using CC = CallingConventions;

	#endregion

	/// <summary>
	/// Provides functions for calling native functions using the <c>calli</c> opcode.
	/// </summary>
	public static unsafe class NativeFunctions
	{
		#region Call

		/// <summary>
		///     Calls a native function with the <c>calli</c> instruction.
		/// </summary>
		/// <param name="fn">Function address</param>
		/// <typeparam name="TRet">Return type</typeparam>
		/// <returns>A value of <typeparamref name="TRet" /></returns>
		public static TRet Call<TRet>([UI] void* fn)
		{
			// @formatter:off
			
			IL.Emit.Ldarg_0();                                                 		// Load arg "fn"
			IL.Emit.Conv_I();                                                  		// Convert arg "fn" to native
			IL.Emit.Calli(new SAMethodSig(CC.Standard, 								// Calling convention
			                              new TypeRef(typeof(TRet)))); 				// Return type
			return IL.Return<TRet>();
			
			// @formatter:on
		}
		
		/// <summary>
		///     Calls a native function with the <c>calli</c> instruction.
		/// </summary>
		/// <param name="fn">Function address</param>
		/// <param name="arg1">Argument #1</param>
		/// <typeparam name="TRet">Return type</typeparam>
		/// <typeparam name="TArg1"><paramref name="arg1" /> type</typeparam>
		/// <returns>A value of <typeparamref name="TRet" /></returns>
		public static TRet Call<TRet, TArg1>([UI] void* fn, [UI] TArg1 arg1)
		{
			// @formatter:off
			
			IL.Emit.Ldarg_1();                                                 		// Load arg "arg1"
			IL.Emit.Ldarg_0();                                                 		// Load arg "fn"
			IL.Emit.Conv_I();                                                  		// Convert arg "fn" to native
			IL.Emit.Calli(new SAMethodSig(CC.Standard, 								// Calling convention
			                              new TypeRef(typeof(TRet)),   				// Return type
			                              new TypeRef(typeof(TArg1))));   			// Arg "arg1" type #1
			return IL.Return<TRet>();

			// @formatter:on
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
		public static TRet Call<TRet, TArg1, TArg2>([UI] void* fn, [UI] TArg1 arg1, [UI] TArg2 arg2)
		{
			// @formatter:off
			
			IL.Emit.Ldarg_1();                                                 		// Load arg "arg1"
			IL.Emit.Ldarg_2();                                                 		// Load arg "arg2"
			IL.Emit.Ldarg_0();                                                 		// Load arg "fn"
			IL.Emit.Conv_I();                                                  		// Convert arg "fn" to native
			IL.Emit.Calli(new SAMethodSig(CC.Standard, 								// Calling convention
			                              new TypeRef(typeof(TRet)),   				// Return type
			                              new TypeRef(typeof(TArg1)),     			// Arg "arg1" type #1
			                              new TypeRef(typeof(TArg2))));				// Arg "arg2" type #2
			return IL.Return<TRet>();

			// @formatter:on
		}
		
		/// <summary>
		///     Calls a native function with the <c>calli</c> instruction.
		/// </summary>
		/// <param name="fn">Function address</param>
		/// <param name="arg1">Argument #1</param>
		/// <typeparam name="TRet">Return type</typeparam>
		/// <returns>A value of <typeparamref name="TRet" /></returns>
		public static TRet Call<TRet>([UI] void* fn, [UI] void* arg1)
		{
			// @formatter:off
			
			IL.Emit.Ldarg_1();                                                  	// Load arg "arg1"
			IL.Emit.Ldarg_0();                                                  	// Load arg "fn"
			IL.Emit.Conv_I();                                                   	// Convert arg "fn" to native
			IL.Emit.Calli(new SAMethodSig(CC.Standard,  							// Calling convention
			                              new TypeRef(typeof(TRet)),    			// Return type
			                              new TypeRef(typeof(void*)))); 			// Arg "arg1" type #1
			return IL.Return<TRet>();

			// @formatter:on
		}

		/// <summary>
		///     Calls a native function with the <c>calli</c> instruction.
		/// </summary>
		/// <param name="fn">Function address</param>
		/// <param name="arg1">Argument #1</param>
		/// <param name="arg2">Argument #2</param>
		/// <typeparam name="TRet">Return type</typeparam>
		/// <returns>A value of <typeparamref name="TRet" /></returns>
		public static TRet Call<TRet>([UI] void* fn, [UI] void* arg1, [UI] void* arg2)
		{
			// @formatter:off
			
			IL.Emit.Ldarg_1();                                                  	// Load arg "arg1"
			IL.Emit.Ldarg_2();                                                  	// Load arg "arg2"
			IL.Emit.Ldarg_0();                                                  	// Load arg "fn"
			IL.Emit.Conv_I();                                                   	// Convert arg "fn" to native
			IL.Emit.Calli(new SAMethodSig(CC.Standard,  							// Calling convention
			                              new TypeRef(typeof(TRet)),    			// Return type
			                              new TypeRef(typeof(void*)),				// Arg "arg1" type #1
			                              new TypeRef(typeof(void*)))); 			// Arg "arg2" type #2
			return IL.Return<TRet>();

			// @formatter:on
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
		public static TRet Call<TRet, TArg3>([UI] void* fn, [UI] void* arg1, [UI] void* arg2, [UI] TArg3 arg3)
		{
			// @formatter:off
			
			IL.Emit.Ldarg_1();                                                  	// Load arg "arg1"
			IL.Emit.Ldarg_2();                                                  	// Load arg "arg2"
			IL.Emit.Ldarg_3();                                                  	// Load arg "arg3"
			IL.Emit.Ldarg_0();                                                  	// Load arg "fn"
			IL.Emit.Conv_I();                                                   	// Convert arg "fn" to native
			IL.Emit.Calli(new SAMethodSig(CC.Standard,  							// Calling convention
			                              new TypeRef(typeof(TRet)),    			// Return type
			                              new TypeRef(typeof(void*)),				// Arg "arg1" type #1
			                              new TypeRef(typeof(void*)),				// Arg "arg2" type #2
			                              new TypeRef(typeof(TArg3)))); 			// Arg "arg3" type #3
			return IL.Return<TRet>();

			// @formatter:on
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
		public static TRet Call<TRet, TArg2>([UI] void* fn, [UI] void* arg1, [UI] TArg2 arg2)
		{
			// @formatter:off
			
			IL.Emit.Ldarg_1();                                                  	// Load arg "arg1"
			IL.Emit.Ldarg_2();                                                  	// Load arg "arg2"
			IL.Emit.Ldarg_0();                                                  	// Load arg "fn"
			IL.Emit.Conv_I();                                                   	// Convert arg "fn" to native
			IL.Emit.Calli(new SAMethodSig(CC.Standard,  							// Calling convention
			                              new TypeRef(typeof(TRet)),    			// Return type
			                              new TypeRef(typeof(void*)),				// Arg "arg1" type #1
			                              new TypeRef(typeof(TArg2)))); 			// Arg "arg2" type #2
			return IL.Return<TRet>();

			// @formatter:on
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
		public static TRet Call<TRet, TArg2, TArg3>([UI] void* fn, [UI] void* arg1, [UI] TArg2 arg2, [UI] TArg3 arg3)
		{
			// @formatter:off
			
			IL.Emit.Ldarg_1();                                                  	// Load arg "arg1"
			IL.Emit.Ldarg_2();                                                  	// Load arg "arg2"
			IL.Emit.Ldarg_3();                                                  	// Load arg "arg3"
			IL.Emit.Ldarg_0();                                                  	// Load arg "fn"
			IL.Emit.Conv_I();                                                   	// Convert arg "fn" to native
			IL.Emit.Calli(new SAMethodSig(CC.Standard,  							// Calling convention
			                              new TypeRef(typeof(TRet)),    			// Return type
			                              new TypeRef(typeof(void*)),				// Arg "arg1" type #1
			                              new TypeRef(typeof(TArg2)), 				// Arg "arg2" type #2
			                              new TypeRef(typeof(TArg3)))); 			// Arg "arg3" type #3
			return IL.Return<TRet>();

			// @formatter:on
		}

		#endregion

		#region CallVoid

		/// <summary>
		///     Calls a native function with the <c>calli</c> instruction.
		/// </summary>
		/// <param name="fn">Function address</param>
		/// <param name="arg1">Argument #1</param>
		/// <typeparam name="T1"><paramref name="arg1" /> type</typeparam>
		public static void CallVoid<T1>([UI] void* fn, [UI] T1 arg1)
		{
			// @formatter:off
			
			IL.Emit.Ldarg_1();                                                 		// Load arg "arg1"
			IL.Emit.Ldarg_0();                                                 		// Load arg "fn"
			IL.Emit.Conv_I();                                                  		// Convert arg "fn" to native
			IL.Emit.Calli(new SAMethodSig(CC.Standard, 								// Calling convention
			                              new TypeRef(typeof(void)),   				// Return type
			                              new TypeRef(typeof(T1))));   				// Arg "arg1" type #1

			// @formatter:on
		}

		/// <summary>
		///     Calls a native function with the <c>calli</c> instruction.
		/// </summary>
		/// <param name="fn">Function address</param>
		/// <param name="arg1">Argument #1</param>
		public static void CallVoid([UI] void* fn, [UI] void* arg1)
		{
			// @formatter:off
			
			IL.Emit.Ldarg_1();                                                  	// Load arg "arg1"
			IL.Emit.Ldarg_0();                                                  	// Load arg "fn"
			IL.Emit.Conv_I();                                                   	// Convert arg "fn" to native
			IL.Emit.Calli(new SAMethodSig(CC.Standard,  							// Calling convention
			                              new TypeRef(typeof(void)),    			// Return type
			                              new TypeRef(typeof(void*)))); 			// Arg "arg1" type #1

			// @formatter:on
		}

		/// <summary>
		///     Calls a native function with the <c>calli</c> instruction.
		/// </summary>
		/// <param name="fn">Function address</param>
		/// <param name="arg1">Argument #1</param>
		/// <param name="arg2">Argument #2</param>
		public static void CallVoid([UI] void* fn, [UI] void* arg1, [UI] void* arg2)
		{
			// @formatter:off
			
			IL.Emit.Ldarg_1();                                                  	// Load arg "arg1"
			IL.Emit.Ldarg_2();                                                  	// Load arg "arg2"
			IL.Emit.Ldarg_0();                                                  	// Load arg "fn"
			IL.Emit.Conv_I();                                                   	// Convert arg "fn" to native
			IL.Emit.Calli(new SAMethodSig(CC.Standard,  							// Calling convention
			                              new TypeRef(typeof(void)),    			// Return type
			                              new TypeRef(typeof(void*)),				// Arg "arg1" type #1
			                              new TypeRef(typeof(void*)))); 			// Arg "arg2" type #2

			// @formatter:on
		}

		#endregion

		#region CallReturnPointer

		/// <summary>
		///     Calls a native function with the <c>calli</c> instruction.
		/// </summary>
		/// <param name="fn">Function address</param>
		public static void* CallReturnPointer([UI] void* fn)
		{
			// @formatter:off
			
			IL.Emit.Ldarg_0();                                                 		// Load arg "fn"
			IL.Emit.Conv_I();                                                  		// Convert arg "fn" to native
			IL.Emit.Calli(new SAMethodSig(CC.Standard, 								// Calling convention
			                              new TypeRef(typeof(void*)))); 			// Return type
			return IL.ReturnPointer();
			
			// @formatter:on
		}

		/// <summary>
		///     Calls a native function with the <c>calli</c> instruction.
		/// </summary>
		/// <param name="fn">Function address</param>
		/// <param name="arg1">Argument #1</param>
		public static void* CallReturnPointer([UI] void* fn, [UI] void* arg1)
		{
			// @formatter:off
			
			IL.Emit.Ldarg_1();                                                 		// Load arg "arg1"
			IL.Emit.Ldarg_0();                                                 		// Load arg "fn"
			IL.Emit.Conv_I();                                                  		// Convert arg "fn" to native
			IL.Emit.Calli(new SAMethodSig(CC.Standard, 								// Calling convention
			                              new TypeRef(typeof(void*)),				// Return type
			                              new TypeRef(typeof(void*)))); 			// Arg "arg1" type #1
			return IL.ReturnPointer();
			
			// @formatter:on
		}

		/// <summary>
		///     Calls a native function with the <c>calli</c> instruction.
		/// </summary>
		/// <param name="fn">Function address</param>
		/// <param name="arg1">Argument #1</param>
		/// <param name="arg2">Argument #2</param>
		public static void* CallReturnPointer([UI] void* fn, [UI] void* arg1, [UI] void* arg2)
		{
			// @formatter:off
			
			IL.Emit.Ldarg_1();                                                 		// Load arg "arg1"
			IL.Emit.Ldarg_2();                                                 		// Load arg "arg2"
			IL.Emit.Ldarg_0();                                                 		// Load arg "fn"
			IL.Emit.Conv_I();                                                  		// Convert arg "fn" to native
			IL.Emit.Calli(new SAMethodSig(CC.Standard, 								// Calling convention
			                              new TypeRef(typeof(void*)),				// Return type
			                              new TypeRef(typeof(void*)),				// Arg "arg1" type #1
			                              new TypeRef(typeof(void*)))); 			// Arg "arg2" type #2
			return IL.ReturnPointer();
			
			// @formatter:on
		}

		/// <summary>
		///     Calls a native function with the <c>calli</c> instruction.
		/// </summary>
		/// <param name="fn">Function address</param>
		/// <param name="arg1">Argument #1</param>
		/// <param name="arg2">Argument #2</param>
		public static void* CallReturnPointer<TArg2>([UI] void* fn, [UI] void* arg1, [UI] TArg2 arg2)
		{
			// @formatter:off
			
			IL.Emit.Ldarg_1();                                                 		// Load arg "arg1"
			IL.Emit.Ldarg_2();                                                 		// Load arg "arg2"
			IL.Emit.Ldarg_0();                                                 		// Load arg "fn"
			IL.Emit.Conv_I();                                                  		// Convert arg "fn" to native
			IL.Emit.Calli(new SAMethodSig(CC.Standard, 								// Calling convention
			                              new TypeRef(typeof(void*)),				// Return type
			                              new TypeRef(typeof(void*)),				// Arg "arg1" type #1
			                              new TypeRef(typeof(TArg2)))); 			// Arg "arg2" type #2
			return IL.ReturnPointer();
			
			// @formatter:on
		}

		#endregion
	}
}