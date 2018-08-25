#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using BenchmarkDotNet.Running;
using RazorCommon;
using RazorCommon.Extensions;
using RazorCommon.Strings;
using RazorSharp;
using RazorSharp.Analysis;
using RazorSharp.CLR;
using RazorSharp.CLR.Structures;
using RazorSharp.Memory;
using RazorSharp.Pointers;
using RazorSharp.Utilities;
using RazorSharp.Utilities.Exceptions;
using Test.Testing;
using Test.Testing.Benchmarking;
using static RazorSharp.Unsafe;
using Unsafe = RazorSharp.Unsafe;
using static RazorSharp.Analysis.InspectorHelper;
using static RazorSharp.Memory.SignatureCall;
using Module = System.Reflection.Module;

#endregion

namespace Test
{

	#region

	using CSUnsafe = System.Runtime.CompilerServices.Unsafe;

	#endregion

	/**
	 * RazorSharp
	 *
	 * History:
	 * 	- RazorSharp (deci-common-c)
	 * 	- RazorSharpNeue
	 * 	- RazorCLR
	 * 	- RazorSharp
	 *
	 * Notes:
	 *  - 32-bit is not fully supported
	 *  - Most types are probably not thread-safe
	 *
	 * Goals:
	 *  - Provide identical functionality of ClrMD, SOS, and Reflection
	 * 	  but in a faster and more efficient way
	 */
	internal static unsafe class Program
	{


#if DEBUG
		static Program()
		{
			StandardOut.ModConsole();

			/**
			 * RazorSharp is only compatible on:
			 *
			 * - x64
			 * - Windows
			 * - CLR
			 *
			 */
			RazorContract.Assert(IntPtr.Size == 8);
			RazorContract.Assert(Environment.Is64BitProcess);
			RazorContract.Assert(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

//			Logger.Log(Flags.Info, "Architecture: x64");
//			Logger.Log(Flags.Info, "Byte order: {0}", BitConverter.IsLittleEndian ? "Little Endian" : "Big Endian");
//			Logger.Log(Flags.Info, "CLR {0}", Environment.Version);
		}
#endif


		private static T* AddrOf<T>(ref T t) where T : unmanaged
		{
			return (T*) AddressOf(ref t);
		}

		private class AddressEventArgs : EventArgs
		{

			public IntPtr OldAddrOfData { get; }

			public IntPtr NewAddrOfData { get; }

			internal AddressEventArgs(IntPtr oldAddrOfData, IntPtr newAddrOfData)
			{
				OldAddrOfData = oldAddrOfData;
				NewAddrOfData = newAddrOfData;
			}

		}

		private class ObjectSentinel : IDisposable
		{

			/// <summary>
			///     Used to tell the pinning thread to stop pinning the object.
			/// </summary>
			private readonly Thread m_thread;

			private          IntPtr     m_origHeapAddr;
			private readonly IntPtr     m_ptrAddr;
			private volatile bool       m_keepAlive;
			public event AddressChanged Event;
			public IntPtr               PtrAddr => m_ptrAddr;

			public delegate void AddressChanged(object sender, AddressEventArgs e);

			private void Watch()
			{
				while (m_keepAlive) {
					var heap = *(IntPtr*) m_ptrAddr;
					if (heap != m_origHeapAddr) {
						OnRaiseCustomEvent(new AddressEventArgs(m_origHeapAddr, heap));
					}
				}
			}

			// Wrap event invocations inside a protected virtual method
			// to allow derived classes to override the event invocation behavior
			protected virtual void OnRaiseCustomEvent(AddressEventArgs e)
			{
				// Make a temporary copy of the event to avoid possibility of
				// a race condition if the last subscriber unsubscribes
				// immediately after the null check and before the event is raised.
				var handler = Event;

				// Event will be null if there are no subscribers
				if (handler != null) {
					// Use the () operator to raise the event.
					handler(this, e);
				}

				m_origHeapAddr = e.NewAddrOfData;
			}

			public static ObjectSentinel Create<T>(ref T t)
			{
				return new ObjectSentinel(AddressOf(ref t));
			}

			private ObjectSentinel(IntPtr ptrAddr)
			{
				m_ptrAddr      = ptrAddr;
				m_origHeapAddr = Marshal.ReadIntPtr(m_ptrAddr);

				m_keepAlive = true;
				m_thread = new Thread(Watch)
				{
					IsBackground = true
				};

				m_thread.Start();
			}

			public void Dispose()
			{
				m_keepAlive = false;
				m_thread.Join();
			}


		}

		struct Vec2
		{
			private float _x,
			              _y;

			public void doSomething()
			{
				Console.WriteLine("foo");
				_x++;
				_y++;
			}
		}



		public static void Main(string[] args)
		{
			// todo: implement dynamic allocation system


			Console.WriteLine(GCHeap.GlobalHeap->GCCount);




//			BenchmarkRunner.Run<SignatureCallBenchmarking>();

			/*var md = Runtime.GetMethodDesc<Vec2>("doSomething");
			Console.WriteLine(md);

			var fd = Runtime.GetFieldDesc<Vec2>("_x");
			Console.WriteLine(fd);

			Console.WriteLine(fd.Reference.getFieldInfo());
			Debug.Assert(fd.Reference.getFieldInfo() == (fd.Reference.FieldInfo));


			var f = typeof(Vec2).Module.ModuleHandle.ResolveFieldHandle(fd.Reference.MemberDef);
			Console.WriteLine("-> {0}", FieldInfo.GetFieldFromHandle(f));
			var x = fd.Reference.GetModule();
			Console.WriteLine(Hex.ToHex(x));


			Console.WriteLine(GCHeap.GCCount);*/



			/*var fd = Runtime.GetFieldDesc<Vec2>("_x");
			var x  = fd.Reference.GetModule();
			Console.WriteLine(fd.Reference.RuntimeType.Module.ResolveField(fd.Reference.MemberDef));


			Console.WriteLine(Hex.ToHex(x));


			var mt = Runtime.MethodTableOf<Vec2>();
			Type o =(Type) CLRFunctions.JIT_GetRuntimeType(mt);
			Console.WriteLine(o.Name);
			Console.WriteLine(mt->ToString());

			Debug.Assert(typeof(Vec2).MetadataToken ==
			             Constants.TokenFromRid(Runtime.MethodTableOf<Vec2>()->Token, CorTokenType.mdtTypeDef));*/




//			var fd = Runtime.GetFieldDesc<Vec2>("_x");

//			BenchmarkRunner.Run<FieldDescsBenchmarking>();


			/*Transpile(typeof(Program), "GetGCCount_sc");
			Console.WriteLine(GetGCCount_sc(GCHeap.Heap.ToPointer()));

			Transpile<GCHeap_t>();
			GCHeap_t* gc = (GCHeap_t*) GCHeap.Heap;

			Console.WriteLine(gc->GetGCCount_sc__this());

			Console.WriteLine(gc->GCCount_prop);*/
		}

		static void __break()
		{
			Console.ReadLine();
		}

		private static void Hook<TOrig, TNew>(string origFn, string newFn)
		{
			Hook(typeof(TOrig), origFn, typeof(TNew), newFn);
		}

		private static void Hook(Type tOrig, string origFn, Type tNew, string newFn)
		{
			var origMd = Runtime.GetMethodDesc(tOrig, origFn);
			var newMd  = Runtime.GetMethodDesc(tNew, newFn);
			origMd.Reference.SetFunctionPointer(newMd.Reference.Function);
		}

		private static void hk_intercept(void* __this, int i)
		{
			Console.WriteLine("Hook {0} | {1}", Hex.ToHex(__this), i);
		}


		struct UObj<T> : IDisposable
		{
			private IntPtr m_addr;

			public IntPtr Address => m_addr;

			internal UObj(IntPtr p)
			{
				m_addr = p;
			}

			/// <summary>
			/// Don't create a new instance using this
			/// </summary>
			public ref T Reference {
				get {
					IntPtr ptrPtr = AddressOf(ref m_addr);
					return ref Memory.AsRef<T>(ptrPtr);
				}
			}

			public void Dispose()
			{
				Marshal.FreeHGlobal(m_addr - IntPtr.Size);
			}

			public override string ToString()
			{
				return Reference.ToString();
			}
		}


		static UObj<T> New<T>() where T : class
		{
			RazorContract.Assert(!typeof(T).IsArray);
			RazorContract.Assert(typeof(T) != typeof(string));
			RazorContract.Assert(!typeof(T).IsIListType());


			var lpMem = Memory.AllocUnmanaged<byte>(Unsafe.BaseInstanceSize<T>());
			lpMem.Increment(sizeof(long));
			lpMem.Write((long) Runtime.MethodTableOf<T>());


			return new UObj<T>(lpMem.Address);
		}


		/**
		 * Dependencies:
		 *
		 * RazorSharp:
		 *  - RazorCommon
		 * 	- CompilerServices.Unsafe
		 *  - RazorInvoke
		 *
		 * Test:
		 *  - RazorCommon
		 *  - CompilerServices.Unsafe
		 * 	- NUnit
		 *  - BenchmarkDotNet
		 */

		/**
		 * Class this ptr:
		 *
		 * public IntPtr __this {
		 *		get {
		 *			var v = this;
		 *			var hThis = Unsafe.AddressOfHeap(ref v);
		 *			return hThis;
		 *		}
		 *	}
		 *
		 *
		 * Struct this ptr:
		 *
		 * public IntPtr __this {
		 *		get => Unsafe.AddressOf(ref this);
		 * }
		 */

		/**
		 * CLR										Used in										Equals
		 *
		 * MethodTable.BaseSize						Unsafe.BaseInstanceSize, Unsafe.HeapSize	-
		 * MethodTable.ComponentSize				Unsafe.HeapSize								-
		 * MethodTable.NumInstanceFieldBytes		Unsafe.BaseFieldsSize						-
		 * EEClass.m_cbNativeSize					Unsafe.NativeSize							Marshal.SizeOf, EEClassLayoutInfo.m_cbNativeSize
		 * EEClassLayoutInfo.m_cbNativeSize			-											Marshal.SizeOf, EEClass.m_cbNativeSize
		 * EEClassLayoutInfo.m_cbManagedSize		-											Unsafe.SizeOf, Unsafe.BaseFieldsSize (value types)
		 */


		/**
		 * #defines:
		 *
		 * FEATURE_COMINTEROP
		 * _TARGET_64BIT_
		 */

	}

}