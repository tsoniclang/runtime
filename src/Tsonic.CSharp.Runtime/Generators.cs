using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Runtime;

public readonly record struct IteratorResult<TYield, TReturn>(
    Union<TYield, TReturn> Value,
    bool Done)
{
    public Union<TYield, TReturn> value => Value;
    public bool done => Done;
}

public sealed class Generator<TYield, TReturn, TNext> : IEnumerable<TYield>, IDisposable
{
    private readonly Func<Generator<TYield, TReturn, TNext>, IEnumerable<TYield>> _bodyFactory;
    private IEnumerator<TYield>? _iterator;
    private TNext? _nextValue;
    private TReturn? _returnValue;
    private bool _hasNextValue;
    private bool _hasReturnValue;
    private bool _started;
    private bool _completed;
    private bool _advancing;

    private Generator(
        Func<Generator<TYield, TReturn, TNext>, IEnumerable<TYield>> bodyFactory)
    {
        _bodyFactory = bodyFactory ?? throw new ArgumentNullException(nameof(bodyFactory));
    }

    public static Generator<TYield, TReturn, TNext> Create(
        Func<Generator<TYield, TReturn, TNext>, IEnumerable<TYield>> bodyFactory)
        => new(bodyFactory);

    public IteratorResult<TYield, TReturn> Next()
        => Advance(false, default!);

    public IteratorResult<TYield, TReturn> Next(TNext value)
        => Advance(true, value);

    public IteratorResult<TYield, TReturn> Return(TReturn value)
    {
        EnterOperation();
        try
        {
            _returnValue = value;
            _hasReturnValue = true;
            _completed = true;
            CloseIterator();
            return CompletedResult(value);
        }
        finally
        {
            _advancing = false;
        }
    }

    public IteratorResult<TYield, TReturn> Throw(Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        EnterOperation();
        try
        {
            _completed = true;
            CloseIterator();
            ExceptionDispatchInfo.Capture(error).Throw();
            throw new InvalidOperationException("Unreachable generator throw continuation.");
        }
        finally
        {
            _advancing = false;
        }
    }

    public TNext ConsumeNext()
    {
        var value = _hasNextValue ? _nextValue : default;
        _nextValue = default;
        _hasNextValue = false;
        return value!;
    }

    public void Complete(TReturn value)
    {
        if (_hasReturnValue)
        {
            throw new InvalidOperationException("The generator return value was already completed.");
        }
        _returnValue = value;
        _hasReturnValue = true;
    }

    public IEnumerator<TYield> GetEnumerator()
        => new EnumerableAdapter(this);

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public void Dispose()
    {
        if (_completed && _iterator is null)
        {
            return;
        }
        EnterOperation();
        try
        {
            _completed = true;
            CloseIterator();
        }
        finally
        {
            _advancing = false;
        }
    }

    private IteratorResult<TYield, TReturn> Advance(bool hasValue, TNext value)
    {
        EnterOperation();
        try
        {
            if (_completed)
            {
                return CompletionResult();
            }
            if (_started && hasValue)
            {
                _nextValue = value;
                _hasNextValue = true;
            }
            else
            {
                _nextValue = default;
                _hasNextValue = false;
            }
            _started = true;
            _iterator ??= _bodyFactory(this).GetEnumerator();
            if (_iterator.MoveNext())
            {
                if (_hasReturnValue)
                {
                    throw new InvalidOperationException(
                        "A generator yielded after completing its return value.");
                }
                return YieldedResult(_iterator.Current);
            }
            _completed = true;
            var returnValue = RequireReturnValue();
            CloseIterator();
            return CompletedResult(returnValue);
        }
        catch
        {
            _completed = true;
            _iterator = null;
            throw;
        }
        finally
        {
            _advancing = false;
        }
    }

    private void EnterOperation()
    {
        if (_advancing)
        {
            throw new InvalidOperationException("A generator operation cannot be re-entered.");
        }
        _advancing = true;
    }

    private TReturn RequireReturnValue()
        => _hasReturnValue
            ? _returnValue!
            : throw new InvalidOperationException(
                "The generated iterator ended without publishing its return value.");

    private IteratorResult<TYield, TReturn> CompletionResult()
        => CompletedResult(_hasReturnValue ? _returnValue! : default!);

    private void CloseIterator()
    {
        var iterator = _iterator;
        _iterator = null;
        iterator?.Dispose();
    }

    private static IteratorResult<TYield, TReturn> YieldedResult(TYield value)
        => new(Union<TYield, TReturn>.From1(value), false);

    private static IteratorResult<TYield, TReturn> CompletedResult(TReturn value)
        => new(Union<TYield, TReturn>.From2(value), true);

    private sealed class EnumerableAdapter : IEnumerator<TYield>
    {
        private readonly Generator<TYield, TReturn, TNext> _generator;

        public EnumerableAdapter(Generator<TYield, TReturn, TNext> generator)
        {
            _generator = generator;
        }

        public TYield Current { get; private set; } = default!;

        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            var result = _generator.Next();
            if (result.Done)
            {
                Current = default!;
                return false;
            }
            Current = result.Value.As1();
            return true;
        }

        public void Reset()
            => throw new NotSupportedException();

        public void Dispose()
            => _generator.Dispose();
    }
}

public sealed class AsyncGenerator<TYield, TReturn, TNext> : IAsyncEnumerable<TYield>, IAsyncDisposable
{
    private readonly Func<AsyncGenerator<TYield, TReturn, TNext>, IAsyncEnumerable<TYield>> _bodyFactory;
    private readonly object _queueGate = new();
    private IAsyncEnumerator<TYield>? _iterator;
    private Task _queueTail = Task.CompletedTask;
    private TNext? _nextValue;
    private TReturn? _returnValue;
    private bool _hasNextValue;
    private bool _hasReturnValue;
    private bool _started;
    private bool _completed;

    private AsyncGenerator(
        Func<AsyncGenerator<TYield, TReturn, TNext>, IAsyncEnumerable<TYield>> bodyFactory)
    {
        _bodyFactory = bodyFactory ?? throw new ArgumentNullException(nameof(bodyFactory));
    }

    public static AsyncGenerator<TYield, TReturn, TNext> Create(
        Func<AsyncGenerator<TYield, TReturn, TNext>, IAsyncEnumerable<TYield>> bodyFactory)
        => new(bodyFactory);

    public Task<IteratorResult<TYield, TReturn>> NextAsync()
        => Enqueue(() => AdvanceAsync(false, default!));

    public Task<IteratorResult<TYield, TReturn>> NextAsync(TNext value)
        => Enqueue(() => AdvanceAsync(true, value));

    public Task<IteratorResult<TYield, TReturn>> ReturnAsync(TReturn value)
        => Enqueue(async () =>
        {
            _returnValue = value;
            _hasReturnValue = true;
            _completed = true;
            await CloseIteratorAsync().ConfigureAwait(false);
            return CompletedResult(value);
        });

    public Task<IteratorResult<TYield, TReturn>> ThrowAsync(Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return Enqueue(async () =>
        {
            _completed = true;
            await CloseIteratorAsync().ConfigureAwait(false);
            ExceptionDispatchInfo.Capture(error).Throw();
            throw new InvalidOperationException("Unreachable async-generator throw continuation.");
        });
    }

    public TNext ConsumeNext()
    {
        var value = _hasNextValue ? _nextValue : default;
        _nextValue = default;
        _hasNextValue = false;
        return value!;
    }

    public void Complete(TReturn value)
    {
        if (_hasReturnValue)
        {
            throw new InvalidOperationException("The async-generator return value was already completed.");
        }
        _returnValue = value;
        _hasReturnValue = true;
    }

    public IAsyncEnumerator<TYield> GetAsyncEnumerator(
        CancellationToken cancellationToken = default)
        => new AsyncEnumerableAdapter(this, cancellationToken);

    public async ValueTask DisposeAsync()
        => await Enqueue(async () =>
        {
            _completed = true;
            await CloseIteratorAsync().ConfigureAwait(false);
            return CompletedResult(_hasReturnValue ? _returnValue! : default!);
        }).ConfigureAwait(false);

    private Task<IteratorResult<TYield, TReturn>> Enqueue(
        Func<ValueTask<IteratorResult<TYield, TReturn>>> operation)
    {
        lock (_queueGate)
        {
            var queued = ExecuteAfterAsync(_queueTail, operation);
            _queueTail = queued;
            return queued;
        }
    }

    private static async Task<IteratorResult<TYield, TReturn>> ExecuteAfterAsync(
        Task predecessor,
        Func<ValueTask<IteratorResult<TYield, TReturn>>> operation)
    {
        try
        {
            await predecessor.ConfigureAwait(false);
        }
        catch
        {
        }
        return await operation().ConfigureAwait(false);
    }

    private async ValueTask<IteratorResult<TYield, TReturn>> AdvanceAsync(
        bool hasValue,
        TNext value)
    {
        if (_completed)
        {
            return CompletionResult();
        }
        if (_started && hasValue)
        {
            _nextValue = value;
            _hasNextValue = true;
        }
        else
        {
            _nextValue = default;
            _hasNextValue = false;
        }
        _started = true;
        _iterator ??= _bodyFactory(this).GetAsyncEnumerator();
        try
        {
            if (await _iterator.MoveNextAsync().ConfigureAwait(false))
            {
                if (_hasReturnValue)
                {
                    throw new InvalidOperationException(
                        "An async generator yielded after completing its return value.");
                }
                return YieldedResult(_iterator.Current);
            }
            _completed = true;
            var returnValue = RequireReturnValue();
            await CloseIteratorAsync().ConfigureAwait(false);
            return CompletedResult(returnValue);
        }
        catch
        {
            _completed = true;
            _iterator = null;
            throw;
        }
    }

    private TReturn RequireReturnValue()
        => _hasReturnValue
            ? _returnValue!
            : throw new InvalidOperationException(
                "The generated async iterator ended without publishing its return value.");

    private IteratorResult<TYield, TReturn> CompletionResult()
        => CompletedResult(_hasReturnValue ? _returnValue! : default!);

    private async ValueTask CloseIteratorAsync()
    {
        var iterator = _iterator;
        _iterator = null;
        if (iterator is not null)
        {
            await iterator.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static IteratorResult<TYield, TReturn> YieldedResult(TYield value)
        => new(Union<TYield, TReturn>.From1(value), false);

    private static IteratorResult<TYield, TReturn> CompletedResult(TReturn value)
        => new(Union<TYield, TReturn>.From2(value), true);

    private sealed class AsyncEnumerableAdapter : IAsyncEnumerator<TYield>
    {
        private readonly AsyncGenerator<TYield, TReturn, TNext> _generator;
        private readonly CancellationToken _cancellationToken;

        public AsyncEnumerableAdapter(
            AsyncGenerator<TYield, TReturn, TNext> generator,
            CancellationToken cancellationToken)
        {
            _generator = generator;
            _cancellationToken = cancellationToken;
        }

        public TYield Current { get; private set; } = default!;

        public async ValueTask<bool> MoveNextAsync()
        {
            _cancellationToken.ThrowIfCancellationRequested();
            var result = await _generator.NextAsync().ConfigureAwait(false);
            if (result.Done)
            {
                Current = default!;
                return false;
            }
            Current = result.Value.As1();
            return true;
        }

        public async ValueTask DisposeAsync()
            => await _generator.DisposeAsync().ConfigureAwait(false);
    }
}

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
