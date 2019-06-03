# RazorSharp

[![nuget](https://img.shields.io/nuget/v/RazorSharp.svg?logo=NuGet)](https://www.nuget.org/packages/RazorSharp/)
[![nuget dl](https://img.shields.io/nuget/dt/RazorSharp.svg?logo=NuGet)](https://www.nuget.org/packages/RazorSharp/)

![Icon](https://github.com/Decimation/RazorSharp/raw/master/icon64.png)

Low-level utilities and tools for working with the CLR, CLR internal structures, and memory.

# Goals

`RazorSharp` aims to provide functionality similar to that of `ClrMD`, `WinDbg SOS`, and `Reflection` but in a faster and more efficient way, while also exposing more underlying metadata. 
`RazorSharp` also allows for manipulation of the CLR and low-level operations with managed objects. Additionally, `RazorSharp` doesn't require attachment of a debugger to the process to acquire metadata. All metadata is acquired through memory; no debugging is necessary or used.

# Usage

`RazorSharp` must be set up to use certain features.

Setup:

```C#
ModuleInitializer.GlobalSetup();
```

Close:

```C#
ModuleInitializer.GlobalClose();
```

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
`RazorSharp` is tested on:
- 64-bit only (32-bit is not fully supported)
- Windows only
- .NET CLR only
- Workstation GC

# todo
- [ ] `MethodDescChunk`
- [ ] Read `MethodDescs` without `Reflection`
- [ ] `RazorSharp`, `ClrMD`, `Reflection`, `Cecil`, `dnlib`, `MetadataTools` comparison
- [x] Replace native pointers* with `Pointer<T>` for consistency
- [ ] nint
- [ ] Massive overhaul and refactoring
- [ ] Possibly use `DIA` instead of `DbgHelp`
- [ ] Possibly use `SharpPdb` instead of `DbgHelp`
- [ ] Rewrite `ToString` methods

# License

Icons made by <a href="https://www.freepik.com/" title="Freepik">Freepik</a>
