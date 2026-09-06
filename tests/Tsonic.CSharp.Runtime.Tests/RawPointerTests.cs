using System;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Runtime.Tests
{
    public sealed class RawPointerTests
    {
        private static int Width => IntPtr.Size * 8;

        [Fact]
        public void NativeRoundTripMutatesTheOriginalAndRetainsItsOwner()
        {
            var original = NativeLocation.Allocate(7U, 4, 4);
            var raw = NativeLocation.ToRaw(original, 4, 4);
            var restored = NativeLocation.Reinterpret<uint>(raw, 4, 4)!;
            Assert.True(Location<uint>.Same(original, restored));
            Assert.Equal(Location<uint>.Hash(original), Location<uint>.Hash(restored));
            restored.Store(11);
            Assert.Equal(11U, original.Load());
            original = null!;
            raw = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            restored.Store(13);
            Assert.Equal(13U, restored.Load());
        }

        [Fact]
        public void ByteOffsetsAliasBytesRatherThanScalingByPointeeSize()
        {
            var original = NativeLocation.Allocate(0U, 4, 4);
            var raw = NativeLocation.ToRaw(original, 4, 4);
            var bytePointer = NativeLocation.Reinterpret<byte>(RawPointer.Offset(raw, 1, Width), 1, 1)!;
            bytePointer.Store(7);
            Assert.Equal(BitConverter.IsLittleEndian ? 7U << 8 : 7U << 16, original.Load());
            Assert.Equal(RawPointer.Address(raw, Width) + 1,
                RawPointer.Address(RawPointer.Offset(raw, 1, Width), Width));
            Assert.True(RawPointer.Same(raw, RawPointer.Offset(RawPointer.Offset(raw, 1, Width), -1, Width)));
        }

        [Fact]
        public void AddressBitsRemainExactAndDoNotCreateOwners()
        {
            var address = Width == 64 ? 9007199254740993UL : uint.MaxValue;
            var pointer = RawPointer.FromAddress(address, Width);
            Assert.Equal(address, RawPointer.Address(pointer, Width));
            Assert.Equal(Width == 64 ? ulong.MaxValue : uint.MaxValue,
                RawPointer.Address(RawPointer.FromAddress(Width == 64 ? ulong.MaxValue : uint.MaxValue, Width), Width));
            Assert.Equal(RawPointer.Hash(pointer), RawPointer.Hash(RawPointer.FromAddress(address, Width)));
            Assert.Throws<OverflowException>(() => RawPointer.Offset(RawPointer.FromAddress(1, Width), -2, Width));
            Assert.Throws<OverflowException>(() => RawPointer.Offset(
                RawPointer.FromAddress(Width == 64 ? ulong.MaxValue : uint.MaxValue, Width), 1, Width));
            Assert.Throws<PlatformNotSupportedException>(() => RawPointer.FromAddress(1, Width == 64 ? 32 : 64));
        }

        [Fact]
        public void MissingPointersAndZeroAddressesRemainMissing()
        {
            Assert.Null(NativeLocation.ToRaw<uint>(null, 4, 4));
            Assert.Null(NativeLocation.Reinterpret<uint>(null, 4, 4));
            Assert.Null(RawPointer.FromAddress(0, Width));
            Assert.Null(RawPointer.Offset(null, 0, Width));
            Assert.True(RawPointer.Same(null, null));
            Assert.Equal(0UL, RawPointer.Address(null, Width));
            Assert.Equal(0, RawPointer.Hash(null));
        }

        [Fact]
        public void InvalidExtentsAndAccessorOnlyLocationsCannotAcquireBacking()
        {
            var original = NativeLocation.Allocate(1U, 4, 4);
            var raw = NativeLocation.ToRaw(original, 4, 4);
            Assert.Throws<IndexOutOfRangeException>(() => NativeLocation.Reinterpret<uint>(RawPointer.Offset(raw, 4, Width), 4, 4));
            Assert.Throws<ArgumentException>(() => NativeLocation.Reinterpret<uint>(RawPointer.Offset(raw, 1, Width), 4, 4));
            Assert.Throws<ArgumentException>(() => NativeLocation.Reinterpret<ulong>(raw, 4, 4));
            Assert.Throws<InvalidOperationException>(() => NativeLocation.ToRaw(Location<uint>.Allocate(1), 4, 4));
            var shifted = Location<uint>.Project(original, value => value + 1, value => value - 1);
            Assert.Throws<InvalidOperationException>(() => NativeLocation.ToRaw(shifted, 4, 4));
            Assert.Equal(2U, shifted.Load());
            Assert.Equal(1U, original.Load());
        }
    }
}
