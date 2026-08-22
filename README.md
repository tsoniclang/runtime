# Tsonic C# Runtime

Base C# runtime substrate for Tsonic-generated C#.

This repository contains target-runtime primitives that emitted C# may need even when a program does not select JavaScript or Node compatibility. JavaScript/global APIs live in `csharp-js`. Node APIs live in `csharp-nodejs`.

The always-available substrate includes `Undefined`, closed union carriers,
generator/resource-management helpers, typed locations, and the closed
`TsValue` object/array/function carriers used for TypeScript `any` and
`unknown`. Surface runtimes may implement `IDynamicObject` or `IDynamicArray`
to participate in those closed operations without making the core runtime
depend on a surface package.

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
var identity = new object();
var pointer = Location<int>.CreateLocal(
    identity,
    () => value,
    next => value = next);
pointer.Store(pointer.Load() + 1);
```

`CreateLocal` binds emitted local or parameter storage to its compiler-owned
identity token. `CreateStatic`, `CreateMember`, and `CreateArrayElement`
represent exact static, receiver/member, and array/index locations.
`ProjectMember` preserves write-back through nested value-type storage.
`Allocate` creates fresh independent storage, and `Same` compares canonical
storage identity rather than wrapper-object identity. Array address formation
validates its index immediately. The carrier is closed, reflection-free, and
compatible with trimming and NativeAOT.
