using System;

namespace Tsonic.CSharp.Runtime
{
    public sealed class Location<T>
    {
        private readonly Func<T> _load;
        private readonly Action<T> _store;

        private Location(Func<T> load, Action<T> store)
        {
            ArgumentNullException.ThrowIfNull(load);
            ArgumentNullException.ThrowIfNull(store);
            _load = load;
            _store = store;
        }

        public T Load() => _load();

        public void Store(T value) => _store(value);

        public static Location<T> Create(Func<T> load, Action<T> store) =>
            new Location<T>(load, store);

        public static Location<T> Create<TState>(
            TState state,
            Func<TState, T> load,
            Action<TState, T> store)
        {
            ArgumentNullException.ThrowIfNull(load);
            ArgumentNullException.ThrowIfNull(store);
            return new Location<T>(() => load(state), value => store(state, value));
        }

        public static Location<T> Create<TState1, TState2>(
            TState1 state1,
            TState2 state2,
            Func<TState1, TState2, T> load,
            Action<TState1, TState2, T> store)
        {
            ArgumentNullException.ThrowIfNull(load);
            ArgumentNullException.ThrowIfNull(store);
            return new Location<T>(
                () => load(state1, state2),
                value => store(state1, state2, value));
        }

        public static Location<T> Allocate(T initial)
        {
            var cell = new Cell(initial);
            return new Location<T>(() => cell.Value, value => cell.Value = value);
        }

        public Location<TValue> Project<TValue>(
            Func<T, TValue> load,
            Func<T, TValue, T> store)
        {
            ArgumentNullException.ThrowIfNull(load);
            ArgumentNullException.ThrowIfNull(store);
            return Location<TValue>.Create(
                () => load(Load()),
                value => Store(store(Load(), value)));
        }

        public Location<TValue> Project<TState, TValue>(
            TState state,
            Func<T, TState, TValue> load,
            Func<T, TState, TValue, T> store)
        {
            ArgumentNullException.ThrowIfNull(load);
            ArgumentNullException.ThrowIfNull(store);
            return Location<TValue>.Create(
                () => load(Load(), state),
                value => Store(store(Load(), state, value)));
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
