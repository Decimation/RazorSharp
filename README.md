[![nuget](https://img.shields.io/nuget/v/RazorSharp.svg?logo=NuGet)](https://www.nuget.org/packages/RazorSharp/)
![nuget dl](https://img.shields.io/nuget/dt/RazorSharp.svg?logo=NuGet)

# RazorSharp
Low-level utilities and tools for working with the CLR, CLR internal structures, and memory.

# Goals

RazorSharp aims to provide functionality similar to that of ClrMD, WinDbg SOS, and Reflection but in a faster and more efficient way, while also exposing more underlying metadata. 
RazorSharp also allows for manipulation of the CLR and low-level operations with managed objects. Additionally, RazorSharp doesn't require attachment of a debugger to the process to acquire metadata. All metadata is acquired through memory; no debugging is necessary or used.

# Features

- [x] Calculating heap size of managed objects
- [x] Taking the address of managed objects
- [x] Pointer to managed types
- [x] Pinning unblittable objects

# CLR structures mapping
- [x] `ObjectHeader`
- [x] `MethodTable`
- [x] `EEClass`
- [x] `EEClassLayoutInfo`
- [x] `LayoutEEClass`
- [x] `FieldDesc`
- [x] `MethodDesc`
- [ ] `MethodDescChunk`
- [ ] `Module`
- [x] `PackedDWORDFields`

# Compatibility
RazorSharp is tested on:
- 64-bit only (32-bit is not fully supported)
- Windows only
- .NET CLR only
- Workstation GC

# todo
- [ ] `MethodDescChunk`
- [ ] Read `MethodDescs` without Reflection
- [x] Fix Canon MT for pointer arrays
- [x] Get field names via `FieldDesc`
- [x] Fix "Function" field in `MethodDesc` for virtual functions

# License

Icons made by <a href="https://www.freepik.com/" title="Freepik">Freepik</a>
