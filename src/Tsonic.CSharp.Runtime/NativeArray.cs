using System;

namespace Tsonic.CSharp.Runtime
{
    public sealed unsafe class NativeArray<T>
    {
        private readonly RawPointer _storage;
        private readonly NativeLayout<T> _layout;
        private readonly nuint _stride;
        public int Length { get; }

        public NativeArray(T[] initial, NativeLayout<T> layout, nuint stride)
        {
            ArgumentNullException.ThrowIfNull(initial);
            ArgumentNullException.ThrowIfNull(layout);
            layout.RequireAbi();
            if (stride < layout.Size || stride % layout.Alignment != 0)
                throw new ArgumentOutOfRangeException(nameof(stride));
            Length = initial.Length;
            _layout = layout;
            _stride = stride;
            _storage = RawPointer.Allocate(checked((nuint)Length * stride), layout.Alignment);
            for (var index = 0; index < Length; index++) this[index] = initial[index];
        }

        public T this[int index] { get => _layout.Read(AddressAt(index)); set => _layout.Write(AddressAt(index), value); }
        public T this[uint index] { get => _layout.Read(AddressAt(index)); set => _layout.Write(AddressAt(index), value); }
        public T this[long index] { get => _layout.Read(AddressAt(index)); set => _layout.Write(AddressAt(index), value); }
        public T this[ulong index] { get => this[CheckedIndex(index)]; set => this[CheckedIndex(index)] = value; }

        public Location<T> LocationAt(int index) => LocationAt((long)index);
        public Location<T> LocationAt(uint index) => LocationAt((long)index);
        public Location<T> LocationAt(ulong index) => LocationAt(CheckedIndex(index));

        private long CheckedIndex(ulong index)
        {
            if (index >= (ulong)Length) throw new IndexOutOfRangeException();
            return (long)index;
        }

        public Location<T> LocationAt(long index) => NativeLocation.Reinterpret(AddressAt(index), _layout)!;

        private RawPointer AddressAt(long index)
        {
            if (index < 0 || index >= Length) throw new IndexOutOfRangeException();
            var offset = checked((nuint)index * _stride);
            return RawPointer.Offset(_storage, (Int128)offset, _layout.AddressWidth)!;
        }
    }
}
