using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using RazorSharp.Common;
using RazorSharp.Native.Enums;
using RazorSharp.Native.Structures;
using static RazorSharp.Native.DbgHelp;

namespace RazorSharp.Native
{

	public class StackTrace
	{
		public readonly StackCall[] Calls;




		public StackTrace(uint processId, uint threadId)
		{
			List<StackCall> stackCalls = new List<StackCall>();

			//StackWalk64 Callbacks
			SymFunctionTableAccess64Delegate functionTableAccessRoutine = SymFunctionTableAccess64;
			SymGetModuleBase64Delegate       getModuleBaseRoutine       = SymGetModuleBase64;

			IntPtr       lpContextRecord = new IntPtr();
			STACKFRAME64 stackFrame      = new STACKFRAME64();

			//Get handle for thread and its process
			IntPtr hProcess = Kernel32.OpenProcess(ProcessAccess.All, false, processId);
			Debug.Assert(hProcess != IntPtr.Zero);

			IntPtr hThread = Kernel32.OpenThread(ThreadAccess.All, false, threadId);
			Debug.Assert(hThread != IntPtr.Zero, Kernel32.GetLastError().ToString());

			//Initialize Symbol handler
			SymInitialize(hProcess, null, false);


			//Determine Image & Processor types
			bool wow64         = false;
			uint processorType = Functions.GetProcessorType();

			if (processorType == (uint) ImageFileMachine.AMD64 | processorType == (uint) ImageFileMachine.IA64) {
				wow64 = Functions.IsWow64(hProcess);
			}

			//Initialize thread context & stack frame based on architectures
			if (wow64) {
				processorType = (uint) ImageFileMachine.I386;

				//Load 32-bit modules for symbol access
				Functions.LoadModules(hProcess, ListModules._32Bit);

				//Initialize an X86_CONTEXT
				X86_CONTEXT contextRecord = new X86_CONTEXT();
				contextRecord.ContextFlags = (uint) ContextFlags.X86ContextAll;
				lpContextRecord            = Marshal.AllocHGlobal(Marshal.SizeOf(contextRecord));
				Marshal.StructureToPtr(contextRecord, lpContextRecord, false);



				//Get context of thread
				Kernel32.Wow64SuspendThread(hThread);
				Kernel32.Wow64GetThreadContext(hThread, lpContextRecord);


				//Initialize Stack frame for first call to StackWalk64
				contextRecord = (X86_CONTEXT) Marshal.PtrToStructure(lpContextRecord, typeof(X86_CONTEXT));
				stackFrame = Functions.InitializeStackFrame64
					(AddressMode.AddrModeFlat, contextRecord.Eip, contextRecord.Esp, contextRecord.Ebp, new ulong());
				Console.WriteLine("::Wow64");
			}
			else if (processorType == (uint) ImageFileMachine.I386) {
				processorType = (uint) ImageFileMachine.I386;

				//Load 32-bit modules for symbol access
				Functions.LoadModules(hProcess, ListModules._32Bit);

				//Initialize an X86_CONTEXT
				X86_CONTEXT contextRecord = new X86_CONTEXT();
				contextRecord.ContextFlags = (uint) ContextFlags.X86ContextAll;
				lpContextRecord            = Marshal.AllocHGlobal(Marshal.SizeOf(contextRecord));
				Marshal.StructureToPtr(contextRecord, lpContextRecord, false);


				//Get context of thread
				Kernel32.SuspendThread(hThread);
				Kernel32.GetThreadContext(hThread, lpContextRecord);


				//Initialize Stack frame for first call to StackWalk64
				contextRecord = (X86_CONTEXT) Marshal.PtrToStructure(lpContextRecord, typeof(X86_CONTEXT));
				stackFrame = Functions.InitializeStackFrame64
					(AddressMode.AddrModeFlat, contextRecord.Eip, contextRecord.Esp, contextRecord.Ebp, new ulong());
			}
			else if (processorType == (uint) ImageFileMachine.AMD64) {
				//Load 64-bit modules for symbol access
				Functions.LoadModules(hProcess, ListModules._64Bit);

				//Initialize AMD64_CONTEXT
				AMD64_CONTEXT contextRecord = new AMD64_CONTEXT();
				contextRecord.ContextFlags = (uint) ContextFlags.AMD64ContextAll;
				lpContextRecord            = Marshal.AllocHGlobal(Marshal.SizeOf(contextRecord));
				Marshal.StructureToPtr(contextRecord, lpContextRecord, false);



				//Get context of thread
				Kernel32.SuspendThread(hThread);
				Kernel32.GetThreadContext(hThread, lpContextRecord);


				//Initialize Stack frame for first call to StackWalk64
				contextRecord = (AMD64_CONTEXT) Marshal.PtrToStructure(lpContextRecord, typeof(AMD64_CONTEXT));
				stackFrame = Functions.InitializeStackFrame64
					(AddressMode.AddrModeFlat, contextRecord.Rip, contextRecord.Rsp, contextRecord.Rsp, new ulong());
			}
			else if (processorType == (uint) ImageFileMachine.IA64) {
				//Load 64-bit modules for symbol access
				Functions.LoadModules(hProcess, ListModules._64Bit);

				//Initialize IA64_CONTEXT
				IA64_CONTEXT contextRecord = new IA64_CONTEXT();
				contextRecord.ContextFlags = (uint) ContextFlags.IA64ContextAll;
				lpContextRecord            = Marshal.AllocHGlobal(Marshal.SizeOf(contextRecord));
				Marshal.StructureToPtr(contextRecord, lpContextRecord, false);



				//Get context of thread
				Kernel32.SuspendThread(hThread);
				Kernel32.GetThreadContext(hThread, lpContextRecord);


				//Initialize Stack frame for first call to StackWalk64
				contextRecord = (IA64_CONTEXT) Marshal.PtrToStructure(lpContextRecord, typeof(IA64_CONTEXT));
				stackFrame = Functions.InitializeStackFrame64
				(AddressMode.AddrModeFlat, contextRecord.StIIP, contextRecord.IntSp, contextRecord.RsBSP,
					contextRecord.IntSp);
			}

			//Marshal stack frame to unmanaged memory
			IntPtr lpStackFrame = Marshal.AllocHGlobal(Marshal.SizeOf(stackFrame));
			Marshal.StructureToPtr(stackFrame, lpStackFrame, false);

			//Walk the Stack
			for (int frameNum = 0;; frameNum++) {
				//Get Stack frame
				StackWalk64(processorType, hProcess, hThread, lpStackFrame, lpContextRecord,
					null, functionTableAccessRoutine, getModuleBaseRoutine, null);
				stackFrame = (STACKFRAME64) Marshal.PtrToStructure(lpStackFrame, typeof(STACKFRAME64));

				if (stackFrame.AddrReturn.Offset == 0) {
					break;
				} //End of stack reached


				Console.WriteLine(stackFrame);


				stackCalls.Add(new StackCall(hProcess, stackFrame.AddrPC.Offset, stackFrame.AddrReturn.Offset,
					(int) threadId));

			}

			Calls = stackCalls.ToArray();

			//Cleanup
			SymCleanup(hProcess);
			Marshal.FreeHGlobal(lpStackFrame);
			Marshal.FreeHGlobal(lpContextRecord);
			Kernel32.ResumeThread(hThread);
			Kernel32.CloseHandle(hThread);
			Kernel32.CloseHandle(hProcess);
		}
	}

}