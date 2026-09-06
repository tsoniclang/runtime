using System;
using System.Runtime.CompilerServices;

namespace Tsonic.CSharp.Runtime
{
    internal abstract class LocationIdentity
    {
        internal abstract LocationIdentity? Parent { get; }

        internal abstract bool SegmentEquals(LocationIdentity other);

        internal abstract int SegmentHash();

        internal uint Hash()
        {
            uint hash = 2166136261;
            for (LocationIdentity? current = this; current is not null; current = current.Parent)
            {
                hash = unchecked((hash ^ (uint)current.SegmentHash()) * 16777619);
            }
            return hash;
        }

        internal bool Same(LocationIdentity other)
        {
            LocationIdentity? left = this;
            LocationIdentity? right = other;
            while (left is not null && right is not null)
            {
                if (!left.SegmentEquals(right))
                {
                    return false;
                }
                left = left.Parent;
                right = right.Parent;
            }
            return left is null && right is null;
        }
    }

    internal sealed class ReferenceLocationIdentity : LocationIdentity
    {
        private readonly object _storage;

        internal ReferenceLocationIdentity(object storage)
        {
            ArgumentNullException.ThrowIfNull(storage);
            _storage = storage;
        }

        internal override LocationIdentity? Parent => null;

        internal override bool SegmentEquals(LocationIdentity other) =>
            other is ReferenceLocationIdentity reference &&
            ReferenceEquals(_storage, reference._storage);

        internal override int SegmentHash() => RuntimeHelpers.GetHashCode(_storage);
    }

    internal sealed class NativeLocationIdentity : LocationIdentity
    {
        private readonly RawPointer _pointer;

        internal NativeLocationIdentity(RawPointer pointer) => _pointer = pointer;
        internal override LocationIdentity? Parent => null;
        internal override bool SegmentEquals(LocationIdentity other) =>
            other is NativeLocationIdentity native && RawPointer.Same(_pointer, native._pointer);
        internal override int SegmentHash() => unchecked((int)(uint)RawPointer.Hash(_pointer));
    }

    internal sealed class StaticLocationIdentity : LocationIdentity
    {
        private readonly string _storage;

        internal StaticLocationIdentity(string storage)
        {
            ArgumentException.ThrowIfNullOrEmpty(storage);
            _storage = storage;
        }

        internal override LocationIdentity? Parent => null;

        internal override bool SegmentEquals(LocationIdentity other) =>
            other is StaticLocationIdentity identity &&
            StringComparer.Ordinal.Equals(_storage, identity._storage);

        internal override int SegmentHash() => StringComparer.Ordinal.GetHashCode(_storage);
    }

    internal sealed class MemberLocationIdentity : LocationIdentity
    {
        private readonly string _member;

        internal MemberLocationIdentity(LocationIdentity parent, string member)
        {
            ArgumentNullException.ThrowIfNull(parent);
            ArgumentException.ThrowIfNullOrEmpty(member);
            Parent = parent;
            _member = member;
        }

        internal override LocationIdentity Parent { get; }

        internal override bool SegmentEquals(LocationIdentity other) =>
            other is MemberLocationIdentity identity &&
            StringComparer.Ordinal.Equals(_member, identity._member);

        internal override int SegmentHash() => StringComparer.Ordinal.GetHashCode(_member);
    }

    internal sealed class ArrayElementLocationIdentity : LocationIdentity
    {
        private readonly long _index;

        internal ArrayElementLocationIdentity(
            LocationIdentity parent,
            long index)
        {
            ArgumentNullException.ThrowIfNull(parent);
            Parent = parent;
            _index = index;
        }

        internal override LocationIdentity Parent { get; }

        internal override bool SegmentEquals(LocationIdentity other) =>
            other is ArrayElementLocationIdentity identity &&
            _index == identity._index;

        internal override int SegmentHash() => _index.GetHashCode();
    }

    public sealed class Location<T>
    {
        internal RawPointer? RawBacking { get; private init; }
        private readonly LocationIdentity _identity;
        private readonly Func<T> _load;
        private readonly Action<T> _store;

        private Location(
            LocationIdentity identity,
            Func<T> load,
            Action<T> store)
        {
            ArgumentNullException.ThrowIfNull(identity);
            ArgumentNullException.ThrowIfNull(load);
            ArgumentNullException.ThrowIfNull(store);
            _identity = identity;
            _load = load;
            _store = store;
        }

        public T Load() => _load();

        public T Value { get => _load(); set => _store(value); }

        internal static Location<T> CreateNative(RawPointer pointer, Func<T> read, Action<T> write) =>
            new Location<T>(new NativeLocationIdentity(pointer), read, write) { RawBacking = pointer };

        public void Store(T value) => _store(value);

        public static double Hash(Location<T>? pointer) => pointer?._identity.Hash() ?? 0;

        public static Location<T> Bind(object identity, Func<T> read, Action<T> write) =>
            new Location<T>(new ReferenceLocationIdentity(identity), read, write);

        public static Location<T> Project<TSource>(
            Location<TSource> source,
            Func<TSource, T> read,
            Func<T, TSource> write)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(read);
            ArgumentNullException.ThrowIfNull(write);
            return new Location<T>(source._identity,
                () => read(source.Load()),
                value => source.Store(write(value)));
        }

        public static Location<T>? ProjectOptional<TSource>(
            Location<TSource>? source,
            Func<TSource, T> read,
            Func<T, TSource> write) =>
            source is null ? null : Project(source, read, write);

        public static bool Same(Location<T>? left, Location<T>? right)
        {
            if (left is null || right is null)
            {
                return left is null && right is null;
            }
            return ReferenceEquals(left, right) ||
                left._identity.Same(right._identity);
        }

        public static Location<T> CreateLocal(
            object storageIdentity,
            Func<T> load,
            Action<T> store) =>
            new Location<T>(
                new ReferenceLocationIdentity(storageIdentity),
                load,
                store);

        public static Location<T> CreateStatic(
            string storageIdentity,
            Func<T> load,
            Action<T> store) =>
            new Location<T>(
                new StaticLocationIdentity(storageIdentity),
                load,
                store);

        public static Location<T> CreateMember<TState>(
            TState state,
            string memberIdentity,
            Func<TState, T> load,
            Action<TState, T> store)
            where TState : class
        {
            ArgumentNullException.ThrowIfNull(state);
            ArgumentNullException.ThrowIfNull(load);
            ArgumentNullException.ThrowIfNull(store);
            return new Location<T>(
                new MemberLocationIdentity(
                    new ReferenceLocationIdentity(state),
                    memberIdentity),
                () => load(state),
                value => store(state, value));
        }

        public static Location<T> CreateArrayElement(T[] storage, int index) =>
            CreateArrayElementCore(storage, index);

        public static Location<T> CreateArrayElement(T[] storage, uint index) =>
            CreateArrayElementCore(storage, index);

        public static Location<T> CreateArrayElement(T[] storage, long index) =>
            CreateArrayElementCore(storage, index);

        public static Location<T> CreateArrayElement(T[] storage, ulong index)
        {
            ArgumentNullException.ThrowIfNull(storage);
            if (index >= (ulong)storage.LongLength)
            {
                throw new IndexOutOfRangeException();
            }
            return CreateArrayElementCore(storage, (long)index);
        }

        public static Location<T> Allocate(T initial)
        {
            var cell = new Cell(initial);
            return new Location<T>(
                new ReferenceLocationIdentity(cell),
                () => cell.Value,
                value => cell.Value = value);
        }

        public Location<TValue> ProjectMember<TValue>(
            string memberIdentity,
            Func<T, TValue> load,
            Func<T, TValue, T> store)
        {
            ArgumentNullException.ThrowIfNull(load);
            ArgumentNullException.ThrowIfNull(store);
            return new Location<TValue>(
                new MemberLocationIdentity(_identity, memberIdentity),
                () => load(Load()),
                value => Store(store(Load(), value)));
        }

        private static Location<T> CreateArrayElementCore(
            T[] storage,
            long index)
        {
            ArgumentNullException.ThrowIfNull(storage);
            if (index < 0 || index >= storage.LongLength)
            {
                throw new IndexOutOfRangeException();
            }
            var exactIndex = checked((int)index);
            return new Location<T>(
                new ArrayElementLocationIdentity(
                    new ReferenceLocationIdentity(storage),
                    index),
                () => storage[exactIndex],
                value => storage[exactIndex] = value);
        }

        private sealed class Cell
        {
            public Cell(T value)
            {
                Value = value;
            }

            public T Value { get; set; }
        }
    }
}
