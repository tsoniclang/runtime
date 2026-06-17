# Tsonic.CSharp.Runtime

TypeScript language runtime primitives for the Tsonic compiler.

## Overview

Tsonic.CSharp.Runtime contains primitives used by Tsonic-generated C# in
JavaScript-surface, Node-style, and ASP.NET Core projects.

The runtime provides:

- **Union carriers** - `Union<T1, T2, ...>` for TypeScript unions like
  `string | number`
- **Structural carriers** - `Structural.Clone<T>()`,
  `Structural.CloneFromDictionary<T>()`, and `DictionaryAdapter<T>` for closed
  generated structural conversion
- **Dynamic object carrier** - `DynamicObject` for compiler-owned indexed
  access and dictionary-shaped projections
- **Operator helpers** - deterministic `typeof` and `instanceof` support
- **JSON helpers** - `JSON.parse<T>()` and `JSON.stringify(...)` for generated
  code paths that use closed structural metadata

## When to Use

This library is referenced by Tsonic projects that compile to the C# target.

- C# target projects use `Tsonic.CSharp.Runtime`
- First-party source surfaces such as `@tsonic/js`, `@tsonic/nodejs`, and `@tsonic/express` also use `Tsonic.CSharp.Runtime`

## What's NOT Here

JavaScript and Node surface behavior is authored in first-party source packages, not in this runtime package:
- Array methods (push, pop, map, filter, etc.)
- String methods (toUpperCase, slice, includes, etc.)
- Math, console, JS-surface `JSON`, and other global functions

`Tsonic.CSharp.Runtime` owns the compiler runtime carriers. It does not define the
user-facing `@tsonic/js` or `@tsonic/nodejs` API surface. The public JS and Node
APIs live in their first-party source packages and compile down to deterministic
runtime calls where needed.

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

Published as `Tsonic.CSharp.Runtime` on NuGet.

## License

MIT License - see LICENSE file for details.
