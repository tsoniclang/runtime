# Tsonic.Runtime

TypeScript language runtime primitives for the Tsonic compiler.

## Overview

Tsonic.Runtime contains mode-independent primitives that are used by emitted NativeAOT-compatible C#:

- **Union Types** - `Union<T1, T2, ...>` for TypeScript unions like `string | number`
- **Array Helpers** - deterministic helpers for generated array operations
- **Generators** - iterator support for generated generator state machines
- **Symbols** - closed symbol handles for compiler-managed symbol values

## When to Use

This library is referenced by **all** Tsonic projects.

- CLR/default-surface projects use `Tsonic.Runtime`
- First-party source surfaces such as `@tsonic/js`, `@tsonic/nodejs`, and `@tsonic/express` also use `Tsonic.Runtime`

## What's NOT Here

JavaScript and Node surface behavior is authored in first-party TypeScript source packages, not in separate CLR runtime packages:
- Array methods (push, pop, map, filter, etc.)
- String methods (toUpperCase, slice, includes, etc.)
- Math, console, JSON, global functions

## Building

```bash
dotnet build
```

## Testing

```bash
dotnet test
```

## NativeAOT Compatibility

This library is compatible with .NET NativeAOT. Runtime reflection, dynamic object bags, JavaScript `typeof`, dynamic JSON carriers, and broad JavaScript value helpers are not part of this package.

## Package

Published as `Tsonic.Runtime` on NuGet.

## License

MIT License - see LICENSE file for details.
