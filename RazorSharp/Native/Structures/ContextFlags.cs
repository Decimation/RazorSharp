using System;

namespace RazorSharp.Native.Structures
{

	[Flags]
    public enum ContextFlags
    {
        None = 0,
        X86Context = 0x10000,
        X86ContextControl = X86Context | 0x1,
        X86ContextInteger = X86Context | 0x2,
        X86ContextSegments = X86Context | 0x4,
        X86ContextFloatingPoint = X86Context | 0x8,
        X86ContextDebugRegisters = X86Context | 0x10,
        X86ContextExtendedRegisters = X86Context | 0x20,
        X86ContextFull = X86Context | X86ContextControl | X86ContextInteger | X86ContextSegments,
        X86ContextAll = X86Context | X86ContextControl | X86ContextInteger | X86ContextSegments | X86ContextFloatingPoint |
        X86ContextDebugRegisters | X86ContextExtendedRegisters,
        AMD64Context = 0x100000,
        AMD64ContextControl = AMD64Context | 0x1,
        AMD64ContextInteger = AMD64Context | 0x2,
        AMD64ContextSegments = AMD64Context | 0x4,
        AMD64ContextFloatingPoint = AMD64Context | 0x8,
        AMD64ContextDebugRegisters = AMD64Context | 0x10,
        AMD64ContextFull = AMD64Context | AMD64ContextControl | AMD64ContextInteger | AMD64ContextFloatingPoint,
        AMD64ContextAll = AMD64Context | AMD64ContextControl | AMD64ContextInteger | AMD64ContextSegments |
        AMD64ContextFloatingPoint | AMD64ContextDebugRegisters,
        IA64Context = 0x80000,
        IA64ContextControl = IA64Context | 0x1,
        IA64ContextLowerFloatingPoint = IA64Context | 0x2,
        IA64ContextHigherFloatingPoint = IA64Context | 0x4,
        IA64ContextInteger = IA64Context | 0x8,
        IA64ContextDebug = IA64Context | 0x10,
        IA64ContextIA32Control = IA64Context | 0x20,
        IA64ContextFloatingPoint = IA64Context | IA64ContextLowerFloatingPoint | IA64ContextHigherFloatingPoint,
        IA64ContextFull = IA64Context | IA64ContextControl | IA64ContextFloatingPoint | IA64ContextInteger | IA64ContextIA32Control,
        IA64ContextAll = IA64Context | IA64ContextControl | IA64ContextFloatingPoint | IA64ContextInteger |
        IA64ContextDebug | IA64ContextIA32Control,
    }


}