using System;
using System.Runtime.InteropServices;

namespace RazorSharp.Native.Structures
{

	[StructLayout(LayoutKind.Explicit, Size = 2672)]
    public struct IA64_CONTEXT
    {
        [FieldOffset(0x000)] public ulong ContextFlags;

        // This section is specified/returned if the ContextFlags word contains
        // the flag CONTEXT_DEBUG.
        [FieldOffset(0x010)] public ulong DbI0;
        [FieldOffset(0x018)] public ulong DbI1;
        [FieldOffset(0x020)] public ulong DbI2;
        [FieldOffset(0x028)] public ulong DbI3;
        [FieldOffset(0x030)] public ulong DbI4;
        [FieldOffset(0x038)] public ulong DbI5;
        [FieldOffset(0x040)] public ulong DbI6;
        [FieldOffset(0x048)] public ulong DbI7;
        [FieldOffset(0x050)] public ulong DbD0;
        [FieldOffset(0x058)] public ulong DbD1;
        [FieldOffset(0x060)] public ulong DbD2;
        [FieldOffset(0x068)] public ulong DbD3;
        [FieldOffset(0x070)] public ulong DbD4;
        [FieldOffset(0x078)] public ulong DbD5;
        [FieldOffset(0x080)] public ulong DbD6;
        [FieldOffset(0x088)] public ulong DbD7;

        // This section is specified/returned if the ContextFlags word contains
        // the flag CONTEXT_LOWER_FLOATING_POINT.
        [FieldOffset(0x090)] public FLOAT128 FltS0;
        [FieldOffset(0x0a0)] public FLOAT128 FltS1;
        [FieldOffset(0x0b0)] public FLOAT128 FltS2;
        [FieldOffset(0x0c0)] public FLOAT128 FltS3;
        [FieldOffset(0x0d0)] public FLOAT128 FltT0;
        [FieldOffset(0x0e0)] public FLOAT128 FltT1;
        [FieldOffset(0x0f0)] public FLOAT128 FltT2;
        [FieldOffset(0x100)] public FLOAT128 FltT3;
        [FieldOffset(0x110)] public FLOAT128 FltT4;
        [FieldOffset(0x120)] public FLOAT128 FltT5;
        [FieldOffset(0x130)] public FLOAT128 FltT6;
        [FieldOffset(0x140)] public FLOAT128 FltT7;
        [FieldOffset(0x150)] public FLOAT128 FltT8;
        [FieldOffset(0x160)] public FLOAT128 FltT9;
        // This section is specified/returned if the ContextFlags word contains
        // the flag CONTEXT_HIGHER_FLOATING_POINT.

        [FieldOffset(0x170)] public FLOAT128 FltS4;
        [FieldOffset(0x180)] public FLOAT128 FltS5;
        [FieldOffset(0x190)] public FLOAT128 FltS6;
        [FieldOffset(0x1a0)] public FLOAT128 FltS7;
        [FieldOffset(0x1b0)] public FLOAT128 FltS8;
        [FieldOffset(0x1c0)] public FLOAT128 FltS9;
        [FieldOffset(0x1d0)] public FLOAT128 FltS10;
        [FieldOffset(0x1e0)] public FLOAT128 FltS11;
        [FieldOffset(0x1f0)] public FLOAT128 FltS12;
        [FieldOffset(0x200)] public FLOAT128 FltS13;
        [FieldOffset(0x210)] public FLOAT128 FltS14;
        [FieldOffset(0x220)] public FLOAT128 FltS15;
        [FieldOffset(0x230)] public FLOAT128 FltS16;
        [FieldOffset(0x240)] public FLOAT128 FltS17;
        [FieldOffset(0x250)] public FLOAT128 FltS18;
        [FieldOffset(0x260)] public FLOAT128 FltS19;
        [FieldOffset(0x270)] public FLOAT128 FltF32;
        [FieldOffset(0x280)] public FLOAT128 FltF33;
        [FieldOffset(0x290)] public FLOAT128 FltF34;
        [FieldOffset(0x2a0)] public FLOAT128 FltF35;
        [FieldOffset(0x2b0)] public FLOAT128 FltF36;
        [FieldOffset(0x2c0)] public FLOAT128 FltF37;
        [FieldOffset(0x2d0)] public FLOAT128 FltF38;
        [FieldOffset(0x2e0)] public FLOAT128 FltF39;
        [FieldOffset(0x2f0)] public FLOAT128 FltF40;
        [FieldOffset(0x300)] public FLOAT128 FltF41;
        [FieldOffset(0x310)] public FLOAT128 FltF42;
        [FieldOffset(0x320)] public FLOAT128 FltF43;
        [FieldOffset(0x330)] public FLOAT128 FltF44;
        [FieldOffset(0x340)] public FLOAT128 FltF45;
        [FieldOffset(0x350)] public FLOAT128 FltF46;
        [FieldOffset(0x360)] public FLOAT128 FltF47;
        [FieldOffset(0x370)] public FLOAT128 FltF48;
        [FieldOffset(0x380)] public FLOAT128 FltF49;
        [FieldOffset(0x390)] public FLOAT128 FltF50;
        [FieldOffset(0x3a0)] public FLOAT128 FltF51;
        [FieldOffset(0x3b0)] public FLOAT128 FltF52;
        [FieldOffset(0x3c0)] public FLOAT128 FltF53;
        [FieldOffset(0x3d0)] public FLOAT128 FltF54;
        [FieldOffset(0x3e0)] public FLOAT128 FltF55;
        [FieldOffset(0x3f0)] public FLOAT128 FltF56;
        [FieldOffset(0x400)] public FLOAT128 FltF57;
        [FieldOffset(0x410)] public FLOAT128 FltF58;
        [FieldOffset(0x420)] public FLOAT128 FltF59;
        [FieldOffset(0x430)] public FLOAT128 FltF60;
        [FieldOffset(0x440)] public FLOAT128 FltF61;
        [FieldOffset(0x450)] public FLOAT128 FltF62;
        [FieldOffset(0x460)] public FLOAT128 FltF63;
        [FieldOffset(0x470)] public FLOAT128 FltF64;
        [FieldOffset(0x480)] public FLOAT128 FltF65;
        [FieldOffset(0x490)] public FLOAT128 FltF66;
        [FieldOffset(0x4a0)] public FLOAT128 FltF67;
        [FieldOffset(0x4b0)] public FLOAT128 FltF68;
        [FieldOffset(0x4c0)] public FLOAT128 FltF69;
        [FieldOffset(0x4d0)] public FLOAT128 FltF70;
        [FieldOffset(0x4e0)] public FLOAT128 FltF71;
        [FieldOffset(0x4f0)] public FLOAT128 FltF72;
        [FieldOffset(0x500)] public FLOAT128 FltF73;
        [FieldOffset(0x510)] public FLOAT128 FltF74;
        [FieldOffset(0x520)] public FLOAT128 FltF75;
        [FieldOffset(0x530)] public FLOAT128 FltF76;
        [FieldOffset(0x540)] public FLOAT128 FltF77;
        [FieldOffset(0x550)] public FLOAT128 FltF78;
        [FieldOffset(0x560)] public FLOAT128 FltF79;
        [FieldOffset(0x570)] public FLOAT128 FltF80;
        [FieldOffset(0x580)] public FLOAT128 FltF81;
        [FieldOffset(0x590)] public FLOAT128 FltF82;
        [FieldOffset(0x5a0)] public FLOAT128 FltF83;
        [FieldOffset(0x5b0)] public FLOAT128 FltF84;
        [FieldOffset(0x5c0)] public FLOAT128 FltF85;
        [FieldOffset(0x5d0)] public FLOAT128 FltF86;
        [FieldOffset(0x5e0)] public FLOAT128 FltF87;
        [FieldOffset(0x5f0)] public FLOAT128 FltF88;
        [FieldOffset(0x600)] public FLOAT128 FltF89;
        [FieldOffset(0x610)] public FLOAT128 FltF90;
        [FieldOffset(0x620)] public FLOAT128 FltF91;
        [FieldOffset(0x630)] public FLOAT128 FltF92;
        [FieldOffset(0x640)] public FLOAT128 FltF93;
        [FieldOffset(0x650)] public FLOAT128 FltF94;
        [FieldOffset(0x660)] public FLOAT128 FltF95;
        [FieldOffset(0x670)] public FLOAT128 FltF96;
        [FieldOffset(0x680)] public FLOAT128 FltF97;
        [FieldOffset(0x690)] public FLOAT128 FltF98;
        [FieldOffset(0x6a0)] public FLOAT128 FltF99;
        [FieldOffset(0x6b0)] public FLOAT128 FltF100;
        [FieldOffset(0x6c0)] public FLOAT128 FltF101;
        [FieldOffset(0x6d0)] public FLOAT128 FltF102;
        [FieldOffset(0x6e0)] public FLOAT128 FltF103;
        [FieldOffset(0x6f0)] public FLOAT128 FltF104;
        [FieldOffset(0x700)] public FLOAT128 FltF105;
        [FieldOffset(0x710)] public FLOAT128 FltF106;
        [FieldOffset(0x720)] public FLOAT128 FltF107;
        [FieldOffset(0x730)] public FLOAT128 FltF108;
        [FieldOffset(0x740)] public FLOAT128 FltF109;
        [FieldOffset(0x750)] public FLOAT128 FltF110;
        [FieldOffset(0x760)] public FLOAT128 FltF111;
        [FieldOffset(0x770)] public FLOAT128 FltF112;
        [FieldOffset(0x780)] public FLOAT128 FltF113;
        [FieldOffset(0x790)] public FLOAT128 FltF114;
        [FieldOffset(0x7a0)] public FLOAT128 FltF115;
        [FieldOffset(0x7b0)] public FLOAT128 FltF116;
        [FieldOffset(0x7c0)] public FLOAT128 FltF117;
        [FieldOffset(0x7d0)] public FLOAT128 FltF118;
        [FieldOffset(0x7e0)] public FLOAT128 FltF119;
        [FieldOffset(0x7f0)] public FLOAT128 FltF120;
        [FieldOffset(0x800)] public FLOAT128 FltF121;
        [FieldOffset(0x810)] public FLOAT128 FltF122;
        [FieldOffset(0x820)] public FLOAT128 FltF123;
        [FieldOffset(0x830)] public FLOAT128 FltF124;
        [FieldOffset(0x840)] public FLOAT128 FltF125;
        [FieldOffset(0x850)] public FLOAT128 FltF126;
        [FieldOffset(0x860)] public FLOAT128 FltF127;
        // This section is specified/returned if the ContextFlags word contains
        // the flag CONTEXT_LOWER_FLOATING_POINT | CONTEXT_HIGHER_FLOATING_POINT | CONTEXT_CONTROL.

        [FieldOffset(0x870)] public UInt64 publicStFPSR; // FP status

        // This section is specified/returned if the ContextFlags word contains
        // the flag CONTEXT_INTEGER.
        [FieldOffset(0x870)] public ulong IntGp; // r1 = 0x, volatile
        [FieldOffset(0x880)] public ulong IntT0; // r2-r3 = 0x; volatile
        [FieldOffset(0x888)] public ulong IntT1; //
        [FieldOffset(0x890)] public ulong IntS0; // r4-r7 = 0x; preserved
        [FieldOffset(0x898)] public ulong IntS1;
        [FieldOffset(0x8a0)] public ulong IntS2;
        [FieldOffset(0x8a8)] public ulong IntS3;
        [FieldOffset(0x8b0)] public ulong IntV0; // r8 = 0x; volatile
        [FieldOffset(0x8b8)] public ulong IntT2; // r9-r11 = 0x; volatile
        [FieldOffset(0x8c0)] public ulong IntT3;
        [FieldOffset(0x8c8)] public ulong IntT4;
        [FieldOffset(0x8d0)] public ulong IntSp; // stack pointer (r12) = 0x; special
        [FieldOffset(0x8d8)] public ulong IntTeb; // teb (r13) = 0x; special
        [FieldOffset(0x8e0)] public ulong IntT5; // r14-r31 = 0x; volatile
        [FieldOffset(0x8e8)] public ulong IntT6;
        [FieldOffset(0x8f0)] public ulong IntT7;
        [FieldOffset(0x8f8)] public ulong IntT8;
        [FieldOffset(0x900)] public ulong IntT9;
        [FieldOffset(0x908)] public ulong IntT10;
        [FieldOffset(0x910)] public ulong IntT11;
        [FieldOffset(0x918)] public ulong IntT12;
        [FieldOffset(0x920)] public ulong IntT13;
        [FieldOffset(0x928)] public ulong IntT14;
        [FieldOffset(0x930)] public ulong IntT15;
        [FieldOffset(0x938)] public ulong IntT16;
        [FieldOffset(0x940)] public ulong IntT17;
        [FieldOffset(0x948)] public ulong IntT18;
        [FieldOffset(0x950)] public ulong IntT19;
        [FieldOffset(0x958)] public ulong IntT20;
        [FieldOffset(0x960)] public ulong IntT21;
        [FieldOffset(0x968)] public ulong IntT22;
        [FieldOffset(0x970)] public ulong IntNats; // Nat bits for r1-r31
        // r1-r31 in bits 1 thru 31.

        [FieldOffset(0x978)] public ulong Preds; // predicates = 0x; preserved
        [FieldOffset(0x980)] public ulong BrRp; // return pointer = 0x; b0 = 0x; preserved
        [FieldOffset(0x988)] public ulong BrS0; // b1-b5 = 0x; preserved
        [FieldOffset(0x990)] public ulong BrS1;
        [FieldOffset(0x998)] public ulong BrS2;
        [FieldOffset(0x9a0)] public ulong BrS3;
        [FieldOffset(0x9a8)] public ulong BrS4;
        [FieldOffset(0x9b0)] public ulong BrT0; // b6-b7 = 0x; volatile
        [FieldOffset(0x9b8)] public ulong BrT1;

        // This section is specified/returned if the ContextFlags word contains
        // the flag CONTEXT_CONTROL.
        // Other application registers
        [FieldOffset(0x9c0)] public ulong ApUNAT; // User Nat collection register = 0x; preserved
        [FieldOffset(0x9c8)] public ulong ApLC; // Loop counter register = 0x; preserved
        [FieldOffset(0x9d0)] public ulong ApEC; // Epilog counter register = 0x; preserved
        [FieldOffset(0x9d8)] public ulong ApCCV; // CMPXCHG value register = 0x; volatile
        [FieldOffset(0x9e0)] public ulong ApDCR; // Default control register (TBD)

        // Register stack info
        [FieldOffset(0x9e8)] public ulong RsPFS; // Previous function state = 0x; preserved
        [FieldOffset(0x9f0)] public ulong RsBSP; // Backing store pointer = 0x; preserved
        [FieldOffset(0x9f8)] public ulong RsBSPSTORE;
        [FieldOffset(0xa00)] public ulong RsRSC; // RSE configuration = 0x; volatile
        [FieldOffset(0xa08)] public ulong RsRNAT; // RSE Nat collection register = 0x; preserved

        // Trap Status Information
        [FieldOffset(0xa10)] public ulong StIPSR; // Interruption Processor Status
        [FieldOffset(0xa18)] public ulong StIIP; // Interruption IP
        [FieldOffset(0xa20)] public ulong StIFS; // Interruption Function State

        // iA32 related control registers
        [FieldOffset(0xa28)] public ulong StFCR; // copy of Ar21
        [FieldOffset(0xa30)] public ulong Eflag; // Eflag copy of Ar24
        [FieldOffset(0xa38)] public ulong SegCSD; // iA32 CSDescriptor (Ar25)
        [FieldOffset(0xa40)] public ulong SegSSD; // iA32 SSDescriptor (Ar26)
        [FieldOffset(0xa48)] public ulong Cflag; // Cr0+Cr4 copy of Ar27
        [FieldOffset(0xa50)] public ulong StFSR; // x86 FP status (copy of AR28)
        [FieldOffset(0xa58)] public ulong StFIR; // x86 FP status (copy of AR29)
        [FieldOffset(0xa60)] public ulong StFDR; // x86 FP status (copy of AR30)
        [FieldOffset(0xa68)] public ulong UNUSEDPACK; // alignment padding
    }

}