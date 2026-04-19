# Tsonic.Runtime

TypeScript language runtime primitives for the Tsonic compiler - provides Union types, structural typing, and other TypeScript language features in C#.

## Overview

Tsonic.Runtime contains mode-independent TypeScript language primitives that are used in **all** Tsonic compilation modes. This library provides:

- **Union Types** - `Union<T1, T2, ...>` for TypeScript unions like `string | number`
- **Structural Typing** - `Structural.Clone<T>()` and `DictionaryAdapter<T>` for duck typing
- **Dynamic Objects** - `DynamicObject` for TypeScript's `keyof` and indexed access patterns
- **Operators** - `typeof` and `instanceof` operator support

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

This library is fully compatible with .NET NativeAOT, enabling TypeScript code to be compiled to native executables.

## Package

Published as `Tsonic.Runtime` on NuGet.

## License

MIT License - see LICENSE file for details.
