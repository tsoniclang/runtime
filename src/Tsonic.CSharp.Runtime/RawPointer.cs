using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Runtime
{
    public sealed unsafe class RawPointer : IEquatable<RawPointer>
    {
        private readonly void* _address;
        private readonly NativeBacking? _backing;

        private RawPointer(void* address, NativeBacking? backing)
        {
            _address = address;
            _backing = backing;
        }

        internal static RawPointer Allocate(nuint size, nuint alignment)
        {
            var allocation = new NativeAllocation(size, alignment);
            return new RawPointer(allocation.Address, allocation);
        }

        public static RawPointer? FromAddress(ulong address, int width)
        {
            RequireAddressWidth(width);
            return address == 0 ? null : new RawPointer((void*)checked((nuint)address), null);
        }

        public static RawPointer? FromExternal(void* address, nuint size, object owner)
        {
            ArgumentNullException.ThrowIfNull(owner);
            if (address == null)
            {
                if (size != 0) throw new ArgumentException("A null external address cannot describe a nonempty region.");
                return null;
            }
            if (size > nuint.MaxValue - (nuint)address)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
            return new RawPointer(address, new ExternalBacking(address, size, owner));
        }

        public static ulong Address(RawPointer? pointer, int width)
        {
            RequireAddressWidth(width);
            return pointer is null ? 0 : (ulong)(nuint)pointer._address;
        }

        public static RawPointer? Offset(RawPointer? pointer, Int128 offset, int width)
        {
            RequireAddressWidth(width);
            var address = checked((nuint)((Int128)(pointer is null ? 0 : (nuint)pointer._address) + offset));
            return address == 0 ? null : new RawPointer((void*)address, pointer?._backing);
        }

        public static bool Same(RawPointer? left, RawPointer? right) =>
            (left is null ? null : left._address) == (right is null ? null : right._address);

        public static RawPointer? OffsetUnsigned(RawPointer? pointer, UInt128 offset, int width) =>
            Offset(pointer, checked((Int128)offset), width);

        public bool Equals(RawPointer? other) => Same(this, other);

        public override bool Equals(object? other) => other is RawPointer pointer && Equals(pointer);

        public override int GetHashCode() => unchecked((int)(uint)Hash(this));

        public static double Hash(RawPointer? pointer)
        {
            var address = pointer is null ? 0UL : (ulong)(nuint)pointer._address;
            return (uint)(address ^ (address >> 32));
        }

        internal T Read<T>() where T : unmanaged
        {
            RequireExtent((nuint)sizeof(T));
            var value = Unsafe.ReadUnaligned<T>(_address);
            GC.KeepAlive(_backing);
            return value;
        }

        internal void Write<T>(T value) where T : unmanaged
        {
            RequireExtent((nuint)sizeof(T));
            Unsafe.WriteUnaligned(_address, value);
            GC.KeepAlive(_backing);
        }

        internal void RequireLayout(nuint size, nuint alignment)
        {
            if (alignment == 0 || (alignment & (alignment - 1)) != 0 ||
                ((nuint)_address & (alignment - 1)) != 0)
            {
                throw new ArgumentException("The raw address does not satisfy the selected alignment.");
            }
            RequireExtent(size);
        }

        private void RequireExtent(nuint size)
        {
            if (_backing is null) return;
            var start = (nuint)_backing.Address;
            var address = (nuint)_address;
            if (address < start || address - start > _backing.Size ||
                size > _backing.Size - (address - start))
            {
                throw new IndexOutOfRangeException("The raw access exceeds its retained allocation.");
            }
        }

        internal static void RequireAddressWidth(int width)
        {
            if (width != IntPtr.Size * 8)
            {
                throw new PlatformNotSupportedException("The selected address ABI differs from the native process.");
            }
        }

        private abstract class NativeBacking
        {
            internal void* Address { get; }
            internal nuint Size { get; }

            protected NativeBacking(void* address, nuint size)
            {
                Address = address;
                Size = size;
            }
        }

        private sealed class ExternalBacking : NativeBacking
        {
            internal object Owner { get; }

            internal ExternalBacking(void* address, nuint size, object owner) : base(address, size)
            {
                Owner = owner;
            }
        }

        private sealed class NativeAllocation : NativeBacking
        {
            internal NativeAllocation(nuint size, nuint alignment) : base(AllocateAddress(size, alignment), size) { }

            private static void* AllocateAddress(nuint size, nuint alignment)
            {
                if (alignment == 0 || (alignment & (alignment - 1)) != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(alignment));
                }
                var nativeAlignment = nuint.Max(alignment, (nuint)IntPtr.Size);
                var allocatedSize = checked((nuint.Max(size, 1) + nativeAlignment - 1) & ~(nativeAlignment - 1));
                var address = NativeMemory.AlignedAlloc(allocatedSize, nativeAlignment);
                if (address == null) throw new OutOfMemoryException();
                NativeMemory.Clear(address, allocatedSize);
                return address;
            }

            ~NativeAllocation() => NativeMemory.AlignedFree(Address);
        }
    }

    public static unsafe class NativeLocation
    {
        public static Location<T> Allocate<T>(T initial, nuint size, nuint alignment, int width, bool littleEndian) where T : unmanaged
        {
            RequireAbi(width, littleEndian);
            RequireSize<T>(size);
            var pointer = RawPointer.Allocate(size, alignment);
            pointer.Write(initial);
            return Create<T>(pointer, size, alignment);
        }

        public static RawPointer? ToRaw<T>(Location<T>? pointer, nuint size, nuint alignment, int width, bool littleEndian) where T : unmanaged
        {
            RequireAbi(width, littleEndian);
            RequireSize<T>(size);
            if (pointer is null) return null;
            var raw = pointer.RawBacking ?? throw new InvalidOperationException(
                "The selected location has no proven physical backing.");
            raw.RequireLayout(size, alignment);
            return raw;
        }

        public static Location<T>? Reinterpret<T>(RawPointer? pointer, nuint size, nuint alignment, int width, bool littleEndian) where T : unmanaged
        {
            RequireAbi(width, littleEndian);
            RequireSize<T>(size);
            return pointer is null ? null : Create<T>(pointer, size, alignment);
        }

        private static void RequireAbi(int width, bool littleEndian)
        {
            RawPointer.RequireAddressWidth(width);
            if (littleEndian != BitConverter.IsLittleEndian)
            {
                throw new PlatformNotSupportedException("The selected memory byte order differs from the native process.");
            }
        }

        private static Location<T> Create<T>(RawPointer pointer, nuint size, nuint alignment) where T : unmanaged
        {
            RequireSize<T>(size);
            pointer.RequireLayout(size, alignment);
            return Location<T>.CreateNative(pointer, pointer.Read<T>, pointer.Write<T>);
        }

        private static void RequireSize<T>(nuint size) where T : unmanaged
        {
            if (size != (nuint)sizeof(T))
            {
                throw new ArgumentException("The selected layout differs from the closed native value representation.");
            }
        }
    }
}
