# RazorSharp

[![nuget](https://img.shields.io/nuget/v/RazorSharp.svg?logo=NuGet)](https://www.nuget.org/packages/RazorSharp/)
[![nuget dl](https://img.shields.io/nuget/dt/RazorSharp.svg?logo=NuGet)](https://www.nuget.org/packages/RazorSharp/)

![Icon](https://github.com/Decimation/RazorSharp/raw/master/icon64.png)

Low-level utilities and tools for working with the CLR, CLR internal structures, and memory.

# Notice

**For a new and improved .NET Core implementation, see [NeoCore](https://github.com/Decimation/NeoCore).**

# Goals

`RazorSharp` aims to provide functionality similar to that of `ClrMD`, `WinDbg SOS`, and `Reflection` but in a more detailed fashion while also exposing more underlying metadata and CLR functionality.

`RazorSharp` also allows for manipulation of the CLR and low-level operations with managed objects. Additionally, `RazorSharp` doesn't require attachment of a debugger to the process to acquire metadata. All metadata is acquired through memory or low-level functions.

# Usage

Some structures of `RazorSharp` must be set up to use certain features. Their respective type initializers and constructors should do this automatically, but this is still being tested as static initialization can be tricky.

# Features

* Calculating heap size of managed objects
* Taking the address of managed objects
* Pointer to managed types
* Pinning unblittable objects
* And much more

# Compatibility
* 64-bit (only partial 32-bit support)
* Windows
* .NET CLR
* .NET Framework 4.7.2
* Workstation GC

# License

Icons made by <a href="https://www.freepik.com/" title="Freepik">Freepik</a>
