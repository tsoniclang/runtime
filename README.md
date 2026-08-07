# Tsonic C# Runtime

Base C# runtime substrate for Tsonic-generated C#.

This repository contains target-runtime primitives that emitted C# may need even when a program does not select JavaScript or Node compatibility. JavaScript/global APIs live in `csharp-js`. Node APIs live in `csharp-nodejs`.

## Typed locations

`Location<T>` is the closed runtime carrier for Tsonic's neutral `Pointer<T>`
contract. It represents one typed mutable storage location; it is not a raw
address and does not use reflection or dynamic dispatch.

For example, Tsonic lowers:

```ts
let value = 1;
const pointer = addressOf(value);
storePointer(pointer, loadPointer(pointer) + 1);
```

to the equivalent closed C# shape:

```csharp
var value = 1;
var pointer = Location<int>.Create(() => value, next => value = next);
pointer.Store(pointer.Load() + 1);
```

`Create` captures an existing location, `Allocate` creates fresh independent
storage, and `Project` preserves write-back through nested value-type storage.
All APIs are delegate-based and compatible with trimming and NativeAOT.
