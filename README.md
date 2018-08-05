# RazorSharp
Low-level utilities and tools for working with the CLR and memory.

# Goals

RazorSharp aims to provide functionality similar to that of ClrMD, WinDbg SOS, and Reflection but in a faster and more efficient way. 
RazorSharp also allows for manipulation of the CLR and low-level operations with managed objects.

# Features

- [x] Calculating heap size of managed objects
- [x] Taking the address of managed objects
- [x] Pointer to managed types
- [x] Pinning unblittable objects

# CLR structures mapping
- [x] ObjectHeader
- [x] MethodTable
- [x] EEClass
- [x] EEClassLayoutInfo
- [x] LayoutEEClass
- [x] FieldDesc
- [x] MethodDesc
- [ ] MethodDescChunk
- [ ] Module
- [ ] PackedDWORDFields

# todo
- [ ] ObjectLayout for arrays
- [ ] MethodDescChunk
- [ ] Read MethodDescs without Reflection
- [ ] Fix Canon MT for pointer arrays
- [ ] Get field names via FieldDesc
- [ ] ObjectLayout without ObjectLayoutInspector TypeLayout (very slow)
