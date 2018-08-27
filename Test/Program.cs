#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using BenchmarkDotNet.Running;
using RazorCommon;
using RazorCommon.Extensions;
using RazorCommon.Strings;
using RazorInvoke;
using RazorInvoke.Libraries;
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

#endregion

namespace Test
{

	#region

	using Unsafe = RazorSharp.Unsafe;
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
			 * - .NET CLR
			 *
			 * Current target: .NET 4.7.2
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
					IntPtr heap = *(IntPtr*) m_ptrAddr;
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
				AddressChanged handler = Event;

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


		public static void Main(string[] args)
		{
			// MethodDesc::GetMethodTable
			const long target      = 0x000000018000B260;
			const long textSegment = 0x0000000180001000;
			const long ofs         = 0xA260;
			Debug.Assert(textSegment + ofs == target);

			var field = Runtime.GetFieldDesc<string>("m_firstChar");
			Console.WriteLine(field);

		}


		private static void __break()
		{
			Console.ReadLine();
		}

		private static void Hook<TOrig, TNew>(string origFn, string newFn)
		{
			Hook(typeof(TOrig), origFn, typeof(TNew), newFn);
		}

		private static void Hook(Type tOrig, string origFn, Type tNew, string newFn)
		{
			Pointer<MethodDesc> origMd = Runtime.GetMethodDesc(tOrig, origFn);
			Pointer<MethodDesc> newMd  = Runtime.GetMethodDesc(tNew, newFn);
			origMd.Reference.SetFunctionPointer(newMd.Reference.Function);
		}


		private struct UObj<T> : IDisposable
		{
			private IntPtr m_addr;

			public IntPtr Address => m_addr;

			internal UObj(IntPtr p)
			{
				m_addr = p;
			}

			/// <summary>
			///     Don't create a new instance using this
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


		private static UObj<T> New<T>() where T : class
		{
			RazorContract.Assert(!typeof(T).IsArray);
			RazorContract.Assert(typeof(T) != typeof(string));
			RazorContract.Assert(!typeof(T).IsIListType());


			Pointer<byte> lpMem = Memory.AllocUnmanaged<byte>(BaseInstanceSize<T>());
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