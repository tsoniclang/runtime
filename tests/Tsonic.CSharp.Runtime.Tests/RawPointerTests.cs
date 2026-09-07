using System;
using System.Collections.Generic;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Runtime.Tests
{
    public sealed class RawPointerTests
    {
        private static int Width => IntPtr.Size * 8;

        private struct Header
        {
            public byte Tag;
            public uint Count;
        }

        private struct Envelope
        {
            public byte Prefix;
            public Header Header;
        }

        [Fact]
        public void NestedPackedRecordsReplaceValuesButRetainPhysicalAliases()
        {
            var layout = new NativeLayout<Envelope>(9, 1, Width, BitConverter.IsLittleEndian,
                pointer => new Envelope { Prefix = pointer.ReadAt<byte>(0, 1),
                    Header = new Header { Tag = pointer.ReadAt<byte>(1, 1), Count = pointer.ReadAt<uint>(5, 1) } },
                (pointer, value) => {
                    pointer.WriteAt(0, 1, value.Prefix);
                    pointer.WriteAt(1, 1, value.Header.Tag);
                    pointer.WriteAt(5, 1, value.Header.Count);
                });
            var original = NativeLocation.Allocate(new Envelope { Prefix = 1, Header = new Header { Tag = 2, Count = 7 } }, layout);
            var saved = original.Load();
            var raw = NativeLocation.ToRaw(original, layout)!;
            var alias = NativeLocation.Reinterpret(raw, layout)!;
            var word = NativeLocation.Reinterpret(RawPointer.Offset(raw, 5, Width),
                NativeLayout.Scalar<uint>(4, 1, Width, BitConverter.IsLittleEndian))!;
            word.Store(9);
            Assert.Equal(9U, original.Load().Header.Count);
            alias.Store(new Envelope { Prefix = 3, Header = new Header { Tag = 4, Count = 11 } });
            Assert.Equal(11U, word.Load());
            Assert.Equal(3, original.Load().Prefix);
            Assert.Equal(7U, saved.Header.Count);
            Assert.True(Location<Envelope>.Same(original, alias));
            for (nuint offset = 2; offset < 5; offset++) Assert.Equal(0, raw.ReadAt<byte>(offset, 1));
            Assert.Throws<IndexOutOfRangeException>(() => raw.ReadAt<uint>(6, 1));
        }

        [Fact]
        public void NativeCollectionsUseAddressIdentityRatherThanWrapperIdentity()
        {
            var first = RawPointer.FromAddress(4096, Width)!;
            var same = RawPointer.FromAddress(4096, Width)!;
            var other = RawPointer.FromAddress(8192, Width)!;
            var values = new HashSet<RawPointer> { first, same, other };
            Assert.Equal(2, values.Count);
            Assert.True(first.Equals(same));
            Assert.False(first.Equals(other));
            Assert.False(first.Equals(null));
            Assert.Equal(first.GetHashCode(), same.GetHashCode());
        }

        [Fact]
        public void NativeRoundTripMutatesTheOriginalAndRetainsItsOwner()
        {
            var original = NativeLocation.Allocate(7U, NativeLayout.Scalar<uint>(4, 4, Width, BitConverter.IsLittleEndian));
            var raw = NativeLocation.ToRaw(original, NativeLayout.Scalar<uint>(4, 4, Width, BitConverter.IsLittleEndian));
            var restored = NativeLocation.Reinterpret<uint>(raw, NativeLayout.Scalar<uint>(4, 4, Width, BitConverter.IsLittleEndian))!;
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
            var original = NativeLocation.Allocate(0U, NativeLayout.Scalar<uint>(4, 4, Width, BitConverter.IsLittleEndian));
            var raw = NativeLocation.ToRaw(original, NativeLayout.Scalar<uint>(4, 4, Width, BitConverter.IsLittleEndian));
            var bytePointer = NativeLocation.Reinterpret<byte>(RawPointer.Offset(raw, 1, Width), NativeLayout.Scalar<byte>(1, 1, Width, BitConverter.IsLittleEndian))!;
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
            Assert.Throws<OverflowException>(() => RawPointer.OffsetUnsigned(null, UInt128.MaxValue, Width));
            Assert.Throws<OverflowException>(() => RawPointer.Offset(null, Int128.MaxValue, Width));
            Assert.Throws<OverflowException>(() => RawPointer.Offset(null, Int128.MinValue, Width));
            Assert.Equal(4UL, RawPointer.Address(RawPointer.OffsetUnsigned(null, 4, Width), Width));
        }

        [Fact]
        public void MissingPointersAndZeroAddressesRemainMissing()
        {
            Assert.Null(NativeLocation.ToRaw<uint>(null, NativeLayout.Scalar<uint>(4, 4, Width, BitConverter.IsLittleEndian)));
            Assert.Null(NativeLocation.Reinterpret<uint>(null, NativeLayout.Scalar<uint>(4, 4, Width, BitConverter.IsLittleEndian)));
            Assert.Null(RawPointer.FromAddress(0, Width));
            Assert.Null(RawPointer.Offset(null, 0, Width));
            Assert.True(RawPointer.Same(null, null));
            Assert.Equal(0UL, RawPointer.Address(null, Width));
            Assert.Equal(0, RawPointer.Hash(null));
        }

        [Fact]
        public void InvalidExtentsAndAccessorOnlyLocationsCannotAcquireBacking()
        {
            var original = NativeLocation.Allocate(1U, NativeLayout.Scalar<uint>(4, 4, Width, BitConverter.IsLittleEndian));
            var raw = NativeLocation.ToRaw(original, NativeLayout.Scalar<uint>(4, 4, Width, BitConverter.IsLittleEndian));
            Assert.Throws<IndexOutOfRangeException>(() => NativeLocation.Reinterpret<uint>(RawPointer.Offset(raw, 4, Width), NativeLayout.Scalar<uint>(4, 4, Width, BitConverter.IsLittleEndian)));
            Assert.Throws<ArgumentException>(() => NativeLocation.Reinterpret<uint>(RawPointer.Offset(raw, 1, Width), NativeLayout.Scalar<uint>(4, 4, Width, BitConverter.IsLittleEndian)));
            Assert.Throws<ArgumentException>(() => NativeLocation.Reinterpret<ulong>(raw, NativeLayout.Scalar<ulong>(4, 4, Width, BitConverter.IsLittleEndian)));
            Assert.Throws<InvalidOperationException>(() => NativeLocation.ToRaw(Location<uint>.Allocate(1), NativeLayout.Scalar<uint>(4, 4, Width, BitConverter.IsLittleEndian)));
            var shifted = Location<uint>.Project(original, value => value + 1, value => value - 1);
            Assert.Throws<InvalidOperationException>(() => NativeLocation.ToRaw(shifted, NativeLayout.Scalar<uint>(4, 4, Width, BitConverter.IsLittleEndian)));
            Assert.Equal(2U, shifted.Load());
            Assert.Equal(1U, original.Load());
        }

        [Fact]
        public void PhysicalOperationsValidateProcessAbiEvenForMissingPointers()
        {
            var otherWidth = Width == 64 ? 32 : 64;
            var little = BitConverter.IsLittleEndian;
            Assert.Throws<PlatformNotSupportedException>(() => NativeLocation.Allocate(1U, NativeLayout.Scalar<uint>(4, 4, otherWidth, little)));
            Assert.Throws<PlatformNotSupportedException>(() => NativeLocation.Allocate(1U, NativeLayout.Scalar<uint>(4, 4, Width, !little)));
            Assert.Throws<PlatformNotSupportedException>(() => NativeLocation.ToRaw<uint>(null, NativeLayout.Scalar<uint>(4, 4, otherWidth, little)));
            Assert.Throws<PlatformNotSupportedException>(() => NativeLocation.ToRaw<uint>(null, NativeLayout.Scalar<uint>(4, 4, Width, !little)));
            Assert.Throws<PlatformNotSupportedException>(() => NativeLocation.Reinterpret<uint>(null, NativeLayout.Scalar<uint>(4, 4, otherWidth, little)));
            Assert.Throws<PlatformNotSupportedException>(() => NativeLocation.Reinterpret<uint>(null, NativeLayout.Scalar<uint>(4, 4, Width, !little)));
        }

        [Fact]
        public void NativeValuePropertyAndUnalignedViewsShareTheSameBytes()
        {
            var original = NativeLocation.Allocate(0UL, NativeLayout.Scalar<ulong>(8, 1, Width, BitConverter.IsLittleEndian));
            var raw = NativeLocation.ToRaw(original, NativeLayout.Scalar<ulong>(8, 1, Width, BitConverter.IsLittleEndian));
            var alias = NativeLocation.Reinterpret<uint>(RawPointer.Offset(raw, 1, Width), NativeLayout.Scalar<uint>(4, 1, Width, BitConverter.IsLittleEndian))!;
            alias.Value = 0x01020304U;
            Assert.Equal(0x01020304U, alias.Load());
            var bytes = BitConverter.GetBytes(original.Value);
            Assert.Equal(BitConverter.GetBytes(0x01020304U), bytes.AsSpan(1, 4).ToArray());
            original.Value = 0;
            Assert.Equal(0U, alias.Value);
        }
    }
}
