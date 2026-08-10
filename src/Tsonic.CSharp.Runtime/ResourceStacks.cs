using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Runtime;

public sealed class SuppressedError : Exception
{
    public SuppressedError(Exception error, Exception suppressed)
        : base("An error was suppressed during resource disposal.", error)
    {
        Error = error ?? throw new ArgumentNullException(nameof(error));
        Suppressed = suppressed ?? throw new ArgumentNullException(nameof(suppressed));
    }

    public Exception Error { get; }

    public Exception Suppressed { get; }
}

public sealed class ResourceStack
{
    private readonly List<IEntry> _entries = [];
    private bool _closed;

    public void Add<T>([AllowNull] T resource, Action<T> dispose)
    {
        EnsureOpen();
        ArgumentNullException.ThrowIfNull(dispose);
        if (resource is not null)
        {
            _entries.Add(new Entry<T>(resource, dispose));
        }
    }

    public void DisposeAndThrow(Exception? completionError)
    {
        EnsureOpen();
        _closed = true;
        Exception? error = completionError;
        for (var index = _entries.Count - 1; index >= 0; index--)
        {
            try
            {
                _entries[index].Dispose();
            }
            catch (Exception disposalError)
            {
                error = error is null
                    ? disposalError
                    : new SuppressedError(disposalError, error);
            }
        }
        _entries.Clear();
        if (error is not null)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    private void EnsureOpen()
    {
        if (_closed)
        {
            throw new InvalidOperationException("The resource stack is already closed.");
        }
    }

    private interface IEntry
    {
        void Dispose();
    }

    private sealed class Entry<T>(T resource, Action<T> dispose) : IEntry
    {
        public void Dispose() => dispose(resource);
    }
}

public sealed class AsyncResourceStack
{
    private readonly List<IEntry> _entries = [];
    private bool _closed;

    public void Add<T>([AllowNull] T resource, Action<T> dispose)
    {
        EnsureOpen();
        ArgumentNullException.ThrowIfNull(dispose);
        if (resource is not null)
        {
            _entries.Add(new SyncEntry<T>(resource, dispose));
        }
    }

    public void AddAsync<T>([AllowNull] T resource, Func<T, ValueTask> dispose)
    {
        EnsureOpen();
        ArgumentNullException.ThrowIfNull(dispose);
        if (resource is not null)
        {
            _entries.Add(new AsyncEntry<T>(resource, dispose));
        }
    }

    public async ValueTask DisposeAndThrowAsync(Exception? completionError)
    {
        EnsureOpen();
        _closed = true;
        Exception? error = completionError;
        for (var index = _entries.Count - 1; index >= 0; index--)
        {
            try
            {
                await _entries[index].DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception disposalError)
            {
                error = error is null
                    ? disposalError
                    : new SuppressedError(disposalError, error);
            }
        }
        _entries.Clear();
        if (error is not null)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    private void EnsureOpen()
    {
        if (_closed)
        {
            throw new InvalidOperationException("The resource stack is already closed.");
        }
    }

    private interface IEntry
    {
        ValueTask DisposeAsync();
    }

    private sealed class SyncEntry<T>(T resource, Action<T> dispose) : IEntry
    {
        public ValueTask DisposeAsync()
        {
            dispose(resource);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class AsyncEntry<T>(T resource, Func<T, ValueTask> dispose) : IEntry
    {
        public ValueTask DisposeAsync() => dispose(resource);
    }
}
