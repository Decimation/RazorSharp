# RazorSharp

[![nuget](https://img.shields.io/nuget/v/RazorSharp.svg?logo=NuGet)](https://www.nuget.org/packages/RazorSharp/)
[![nuget dl](https://img.shields.io/nuget/dt/RazorSharp.svg?logo=NuGet)](https://www.nuget.org/packages/RazorSharp/)

![Icon](https://github.com/Decimation/RazorSharp/raw/master/icon64.png)

Low-level utilities and tools for working with the CLR, CLR internal structures, and memory.

# Goals

`RazorSharp` aims to provide functionality similar to that of `ClrMD`, `WinDbg SOS`, and `Reflection` but in a more detailed fashion while also exposing more underlying metadata and CLR functionality.

`RazorSharp` also allows for manipulation of the CLR and low-level operations with managed objects. Additionally, `RazorSharp` doesn't require attachment of a debugger to the process to acquire metadata. All metadata is acquired through memory or low-level functions.

# Usage

`RazorSharp` must be set up to use certain features. The module initializer does this automatically.

# Features

- [x] Calculating heap size of managed objects
- [x] Taking the address of managed objects
- [x] Pointer to managed types
- [x] Pinning unblittable objects

# Compatibility
`RazorSharp` is tested on:
- 64-bit or 32-bit
- Windows only
- .NET CLR only
- Workstation GC

# License

Icons made by <a href="https://www.freepik.com/" title="Freepik">Freepik</a>
