# `@tsonic/csharp-runtime`

Base C# runtime substrate for Tsonic-generated C#. It owns closed carriers and
helpers required independently of the JavaScript surface and Node capability,
including undefined/union values, finite broad values, generators, resource
management, and typed locations.

Canonical product documentation:

- [C# interop and safety](https://github.com/tsoniclang/tsonic/blob/main/docs/manual/targets/csharp/interop-and-safety.md)
- [C# type mapping](https://github.com/tsoniclang/tsonic/blob/main/docs/reference/targets/csharp/type-mapping.md)
- [Provider and runtime ownership](https://github.com/tsoniclang/tsonic/blob/main/docs/architecture/provider-and-runtime-ownership.md)

The npm package contains the canonical runtime artifacts under `runtimes/`.
The C# target references them directly; it does not copy runtime source.
