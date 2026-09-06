using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Runtime.Tests
{
    public sealed unsafe class ExternalMemoryTests
    {
        private static int Width => IntPtr.Size * 8;

        [Fact]
        public void TypedAndOffsetAliasesRetainTheProviderLeaseUntilTheirLastUse()
        {
            var released = new ReleaseCount();
            ExerciseLease(released);
            Collect();
            Assert.Equal(1, released.Value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ExerciseLease(ReleaseCount released)
        {
            var original = CreateRegion(released);
            var raw = RawPointer.Offset(original, 4, Width)!;
            var typed = NativeLocation.Reinterpret<uint>(raw, 4, 4, Width, BitConverter.IsLittleEndian)!;
            var duplicate = NativeLocation.Reinterpret<uint>(raw, 4, 4, Width, BitConverter.IsLittleEndian)!;
            original = null!;
            raw = null!;
            Collect();
            Assert.Equal(0, released.Value);
            typed.Store(17);
            Assert.Equal(17U, duplicate.Load());
            Assert.True(Location<uint>.Same(typed, duplicate));
            Assert.Equal(Location<uint>.Hash(typed), Location<uint>.Hash(duplicate));
            duplicate.Store(23);
            Assert.Equal(23U, typed.Load());
        }

        [Fact]
        public void AddressExtractionDoesNotRetainOrRecoverTheLease()
        {
            var released = new ReleaseCount();
            var bits = AddressOnly(released);
            var unowned = RawPointer.FromAddress(bits, Width);
            Collect();
            Assert.Equal(1, released.Value);
            Assert.Equal(bits, RawPointer.Address(unowned, Width));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ulong AddressOnly(ReleaseCount released) => RawPointer.Address(CreateRegion(released), Width);

        [Fact]
        public void ExternalAliasesCheckTheOriginalRegionAndSelectedAlignment()
        {
            var raw = CreateRegion(new ReleaseCount());
            var after = RawPointer.Offset(raw, 8, Width);
            var before = RawPointer.Offset(raw, -1, Width);
            Assert.Throws<IndexOutOfRangeException>(() => NativeLocation.Reinterpret<uint>(after, 4, 4, Width, BitConverter.IsLittleEndian));
            Assert.Throws<IndexOutOfRangeException>(() => NativeLocation.Reinterpret<byte>(before, 1, 1, Width, BitConverter.IsLittleEndian));
            Assert.Throws<ArgumentException>(() => NativeLocation.Reinterpret<uint>(RawPointer.Offset(raw, 1, Width), 4, 4, Width, BitConverter.IsLittleEndian));
            Assert.Throws<ArgumentException>(() => RawPointer.FromExternal(null, 1, new object()));
            Assert.Throws<ArgumentOutOfRangeException>(() => RawPointer.FromExternal((void*)nuint.MaxValue, 1, new object()));
            Assert.Throws<ArgumentNullException>(() => RawPointer.FromExternal(null, 0, null!));
            Assert.Null(RawPointer.FromExternal(null, 0, new object()));
        }

        [Fact]
        public void RetainedDescriptorCopyDoesNotRetargetWhenItsContainerChanges()
        {
            var first = CreateRegion(new ReleaseCount());
            var second = CreateRegion(new ReleaseCount());
            var descriptors = new[] { (Address: first, Length: 2) };
            var copied = descriptors[0];
            descriptors[0] = (second, 1);
            var oldView = NativeLocation.Reinterpret<uint>(copied.Address, 4, 4, Width, BitConverter.IsLittleEndian)!;
            var newView = NativeLocation.Reinterpret<uint>(descriptors[0].Address, 4, 4, Width, BitConverter.IsLittleEndian)!;
            oldView.Store(37);
            Assert.Equal(0U, newView.Load());
            newView.Store(41);
            Assert.Equal(37U, oldView.Load());
            Assert.Equal(2, copied.Length);
            Assert.False(Location<uint>.Same(oldView, newView));
        }

        [Fact]
        public void ExplicitProviderPinSharesTheOriginalManagedArrayStorage()
        {
            var values = new uint[] { 3, 5 };
            var raw = Pin(values);
            var second = NativeLocation.Reinterpret<uint>(RawPointer.Offset(raw, 4, Width), 4, 4, Width, BitConverter.IsLittleEndian)!;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            second.Store(17);
            Assert.Equal(17U, values[1]);
            values[1] = 23;
            Assert.Equal(23U, second.Load());
            Assert.Equal(3U, values[0]);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static RawPointer Pin(uint[] values)
        {
            var owner = new PinnedArray(values);
            return RawPointer.FromExternal(owner.Address, checked((nuint)values.Length * 4), owner)!;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static RawPointer CreateRegion(ReleaseCount released)
        {
            var owner = new ProviderAllocation(released);
            return RawPointer.FromExternal(owner.Address, 8, owner)!;
        }

        private static void Collect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private sealed class ReleaseCount
        {
            internal int Value;
        }

        private sealed class PinnedArray
        {
            private GCHandle _pin;
            internal void* Address => _pin.AddrOfPinnedObject().ToPointer();

            internal PinnedArray(uint[] values) => _pin = GCHandle.Alloc(values, GCHandleType.Pinned);

            ~PinnedArray()
            {
                if (_pin.IsAllocated) _pin.Free();
            }
        }

        private sealed class ProviderAllocation
        {
            internal void* Address { get; }
            private readonly ReleaseCount _released;

            internal ProviderAllocation(ReleaseCount released)
            {
                _released = released;
                Address = NativeMemory.AllocZeroed(8);
                if (Address == null) throw new OutOfMemoryException();
            }

            ~ProviderAllocation()
            {
                NativeMemory.Free(Address);
                System.Threading.Interlocked.Increment(ref _released.Value);
            }
        }
    }
}
