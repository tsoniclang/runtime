namespace Tsonic.Runtime;

/// <summary>
/// Result type for generator iteration, matching JavaScript's IteratorResult interface.
/// Used by bidirectional generators (Generator&lt;TYield, TReturn, TNext&gt;).
/// </summary>
/// <typeparam name="T">The type of the yielded or returned value</typeparam>
/// <param name="Value">The yielded value (when done=false) or default (when done=true)</param>
/// <param name="Done">True when the generator has completed, false otherwise</param>
/// <remarks>
/// Note: In JavaScript, IteratorResult.value is TYield | TReturn on completion.
/// C# cannot represent this union, so when done=true, value is default(T).
/// Use the generator's returnValue property to access the TReturn value after completion.
/// </remarks>
public readonly record struct IteratorResult<T>(T Value, bool Done)
{
    // Keep lowercase aliases for JS/TS-style `result.value` and `result.done`.
    public T value => Value;
    public bool done => Done;
}
