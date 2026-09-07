using System;

namespace Tsonic.CSharp.Runtime
{
    public sealed class NativeLayout<T>
    {
        public nuint Size { get; }
        public nuint Alignment { get; }
        public int AddressWidth { get; }
        public bool LittleEndian { get; }
        private readonly Func<RawPointer, T> _read;
        private readonly Action<RawPointer, T> _write;

        public NativeLayout(nuint size, nuint alignment, int addressWidth, bool littleEndian,
            Func<RawPointer, T> read, Action<RawPointer, T> write)
        {
            if (alignment == 0 || (alignment & (alignment - 1)) != 0)
                throw new ArgumentOutOfRangeException(nameof(alignment));
            ArgumentNullException.ThrowIfNull(read);
            ArgumentNullException.ThrowIfNull(write);
            Size = size;
            Alignment = alignment;
            AddressWidth = addressWidth;
            LittleEndian = littleEndian;
            _read = read;
            _write = write;
        }

        internal void RequireAbi()
        {
            RawPointer.RequireAddressWidth(AddressWidth);
            if (LittleEndian != BitConverter.IsLittleEndian)
                throw new PlatformNotSupportedException("The selected memory byte order differs from the native process.");
        }

        internal T Read(RawPointer pointer)
        {
            pointer.RequireLayout(Size, Alignment);
            return _read(pointer);
        }

        internal void Write(RawPointer pointer, T value)
        {
            pointer.RequireLayout(Size, Alignment);
            _write(pointer, value);
        }
    }

    public static class NativeLayout
    {
        public static unsafe NativeLayout<T> Scalar<T>(nuint size, nuint alignment, int width, bool littleEndian)
            where T : unmanaged
        {
            if (size != (nuint)sizeof(T))
                throw new ArgumentException("The selected layout differs from the closed native scalar representation.");
            return new NativeLayout<T>(size, alignment, width, littleEndian,
                pointer => pointer.Read<T>(), (pointer, value) => pointer.Write(value));
        }
    }
}
