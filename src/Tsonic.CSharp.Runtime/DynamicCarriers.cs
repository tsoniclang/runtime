namespace Tsonic.CSharp.Runtime
{
    public interface IDynamicObject
    {
        bool TryReadDynamicSlot(string key, out object? value);

        void WriteDynamicSlot(string key, object? value);
    }

    public interface IDynamicArray
    {
        int Length { get; }

        bool HasIndex(int index);

        bool TryGetAt(int index, out object? value);

        bool TrySetAt(int index, object? value);

        int SetLength(int newLength);

        bool DeleteAt(int index);
    }
}
