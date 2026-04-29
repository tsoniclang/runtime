using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tsonic.Runtime;

public class JSArray<T> : IEnumerable<T>
{
    private readonly List<T> _list;

    public JSArray()
    {
        _list = new List<T>();
    }

    public JSArray(int capacity)
    {
        _list = new List<T>(capacity);
    }

    public JSArray(T[] source)
    {
        _list = new List<T>(source);
    }

    public JSArray(List<T> source)
    {
        _list = new List<T>(source);
    }

    public JSArray(IEnumerable<T> source)
    {
        _list = new List<T>(source);
    }

    public int length => _list.Count;

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _list.Count)
            {
                return default!;
            }

            return _list[index];
        }
        set
        {
            if (index < 0)
            {
                throw new ArgumentException("Array index cannot be negative", nameof(index));
            }

            while (_list.Count <= index)
            {
                _list.Add(default!);
            }

            _list[index] = value;
        }
    }

    public void setLength(int newLength)
    {
        if (newLength < 0)
        {
            throw new ArgumentException("Invalid array length", nameof(newLength));
        }

        if (newLength < _list.Count)
        {
            _list.RemoveRange(newLength, _list.Count - newLength);
        }
        else if (newLength > _list.Count)
        {
            var toAdd = newLength - _list.Count;
            for (var i = 0; i < toAdd; i++)
            {
                _list.Add(default!);
            }
        }
    }

    public int push(T item)
    {
        _list.Add(item);
        return _list.Count;
    }

    public int push(params T[] items)
    {
        _list.AddRange(items);
        return _list.Count;
    }

    public T pop()
    {
        if (_list.Count == 0)
        {
            return default!;
        }

        var item = _list[^1];
        _list.RemoveAt(_list.Count - 1);
        return item;
    }

    public T shift()
    {
        if (_list.Count == 0)
        {
            return default!;
        }

        var item = _list[0];
        _list.RemoveAt(0);
        return item;
    }

    public int unshift(T item)
    {
        _list.Insert(0, item);
        return _list.Count;
    }

    public int unshift(params T[] items)
    {
        _list.InsertRange(0, items);
        return _list.Count;
    }

    public JSArray<T> slice(int start = 0, int? end = null)
    {
        var actualStart = start < 0 ? Math.Max(0, _list.Count + start) : start;
        var actualEnd = end.HasValue
            ? (end.Value < 0 ? Math.Max(0, _list.Count + end.Value) : end.Value)
            : _list.Count;

        actualStart = Math.Min(actualStart, _list.Count);
        actualEnd = Math.Min(actualEnd, _list.Count);

        if (actualStart >= actualEnd)
        {
            return new JSArray<T>();
        }

        return new JSArray<T>(_list.GetRange(actualStart, actualEnd - actualStart));
    }

    public JSArray<T> splice(int start, int? deleteCount = null, params T[] items)
    {
        var actualStart = start < 0 ? Math.Max(0, _list.Count + start) : Math.Min(start, _list.Count);
        var actualDeleteCount = deleteCount ?? (_list.Count - actualStart);
        actualDeleteCount = Math.Max(0, Math.Min(actualDeleteCount, _list.Count - actualStart));

        var deleted = new JSArray<T>();
        for (var i = 0; i < actualDeleteCount; i++)
        {
            deleted.push(_list[actualStart]);
            _list.RemoveAt(actualStart);
        }

        for (var i = 0; i < items.Length; i++)
        {
            _list.Insert(actualStart + i, items[i]);
        }

        return deleted;
    }

    public JSArray<TResult> map<TResult>(Func<T, TResult> callback)
    {
        var result = new JSArray<TResult>(_list.Count);
        for (var i = 0; i < _list.Count; i++)
        {
            result.push(callback(_list[i]));
        }
        return result;
    }

    public JSArray<TResult> map<TResult>(Func<T, int, TResult> callback)
    {
        var result = new JSArray<TResult>(_list.Count);
        for (var i = 0; i < _list.Count; i++)
        {
            result.push(callback(_list[i], i));
        }
        return result;
    }

    public JSArray<TResult> map<TResult>(Func<T, int, JSArray<T>, TResult> callback)
    {
        var result = new JSArray<TResult>(_list.Count);
        for (var i = 0; i < _list.Count; i++)
        {
            result.push(callback(_list[i], i, this));
        }
        return result;
    }

    public JSArray<T> filter(Func<T, bool> callback)
    {
        var result = new JSArray<T>();
        for (var i = 0; i < _list.Count; i++)
        {
            if (callback(_list[i]))
            {
                result.push(_list[i]);
            }
        }
        return result;
    }

    public JSArray<T> filter(Func<T, int, bool> callback)
    {
        var result = new JSArray<T>();
        for (var i = 0; i < _list.Count; i++)
        {
            if (callback(_list[i], i))
            {
                result.push(_list[i]);
            }
        }
        return result;
    }

    public JSArray<T> filter(Func<T, int, JSArray<T>, bool> callback)
    {
        var result = new JSArray<T>();
        for (var i = 0; i < _list.Count; i++)
        {
            if (callback(_list[i], i, this))
            {
                result.push(_list[i]);
            }
        }
        return result;
    }

    public TResult reduce<TResult>(Func<TResult, T, TResult> callback, TResult initialValue)
    {
        var accumulator = initialValue;
        for (var i = 0; i < _list.Count; i++)
        {
            accumulator = callback(accumulator, _list[i]);
        }
        return accumulator;
    }

    public TResult reduce<TResult>(Func<TResult, T, int, TResult> callback, TResult initialValue)
    {
        var accumulator = initialValue;
        for (var i = 0; i < _list.Count; i++)
        {
            accumulator = callback(accumulator, _list[i], i);
        }
        return accumulator;
    }

    public TResult reduce<TResult>(Func<TResult, T, int, JSArray<T>, TResult> callback, TResult initialValue)
    {
        var accumulator = initialValue;
        for (var i = 0; i < _list.Count; i++)
        {
            accumulator = callback(accumulator, _list[i], i, this);
        }
        return accumulator;
    }

    public T reduce(Func<T, T, T> callback)
    {
        if (_list.Count == 0)
        {
            throw new InvalidOperationException("Reduce of empty array with no initial value");
        }

        var accumulator = _list[0];
        for (var i = 1; i < _list.Count; i++)
        {
            accumulator = callback(accumulator, _list[i]);
        }

        return accumulator;
    }

    public TResult reduceRight<TResult>(Func<TResult, T, TResult> callback, TResult initialValue)
    {
        var accumulator = initialValue;
        for (var i = _list.Count - 1; i >= 0; i--)
        {
            accumulator = callback(accumulator, _list[i]);
        }
        return accumulator;
    }

    public TResult reduceRight<TResult>(Func<TResult, T, int, TResult> callback, TResult initialValue)
    {
        var accumulator = initialValue;
        for (var i = _list.Count - 1; i >= 0; i--)
        {
            accumulator = callback(accumulator, _list[i], i);
        }
        return accumulator;
    }

    public TResult reduceRight<TResult>(Func<TResult, T, int, JSArray<T>, TResult> callback, TResult initialValue)
    {
        var accumulator = initialValue;
        for (var i = _list.Count - 1; i >= 0; i--)
        {
            accumulator = callback(accumulator, _list[i], i, this);
        }
        return accumulator;
    }

    public void forEach(Action<T> callback)
    {
        for (var i = 0; i < _list.Count; i++)
        {
            callback(_list[i]);
        }
    }

    public void forEach(Action<T, int> callback)
    {
        for (var i = 0; i < _list.Count; i++)
        {
            callback(_list[i], i);
        }
    }

    public void forEach(Action<T, int, JSArray<T>> callback)
    {
        for (var i = 0; i < _list.Count; i++)
        {
            callback(_list[i], i, this);
        }
    }

    public T find(Func<T, bool> callback)
    {
        for (var i = 0; i < _list.Count; i++)
        {
            if (callback(_list[i]))
            {
                return _list[i];
            }
        }
        return default!;
    }

    public T find(Func<T, int, bool> callback)
    {
        for (var i = 0; i < _list.Count; i++)
        {
            if (callback(_list[i], i))
            {
                return _list[i];
            }
        }
        return default!;
    }

    public T find(Func<T, int, JSArray<T>, bool> callback)
    {
        for (var i = 0; i < _list.Count; i++)
        {
            if (callback(_list[i], i, this))
            {
                return _list[i];
            }
        }
        return default!;
    }

    public int findIndex(Func<T, bool> callback)
    {
        for (var i = 0; i < _list.Count; i++)
        {
            if (callback(_list[i]))
            {
                return i;
            }
        }
        return -1;
    }

    public int findIndex(Func<T, int, bool> callback)
    {
        for (var i = 0; i < _list.Count; i++)
        {
            if (callback(_list[i], i))
            {
                return i;
            }
        }
        return -1;
    }

    public int findIndex(Func<T, int, JSArray<T>, bool> callback)
    {
        for (var i = 0; i < _list.Count; i++)
        {
            if (callback(_list[i], i, this))
            {
                return i;
            }
        }
        return -1;
    }

    public T findLast(Func<T, bool> callback)
    {
        for (var i = _list.Count - 1; i >= 0; i--)
        {
            if (callback(_list[i]))
            {
                return _list[i];
            }
        }
        return default!;
    }

    public T findLast(Func<T, int, bool> callback)
    {
        for (var i = _list.Count - 1; i >= 0; i--)
        {
            if (callback(_list[i], i))
            {
                return _list[i];
            }
        }
        return default!;
    }

    public T findLast(Func<T, int, JSArray<T>, bool> callback)
    {
        for (var i = _list.Count - 1; i >= 0; i--)
        {
            if (callback(_list[i], i, this))
            {
                return _list[i];
            }
        }
        return default!;
    }

    public int findLastIndex(Func<T, bool> callback)
    {
        for (var i = _list.Count - 1; i >= 0; i--)
        {
            if (callback(_list[i]))
            {
                return i;
            }
        }
        return -1;
    }

    public int findLastIndex(Func<T, int, bool> callback)
    {
        for (var i = _list.Count - 1; i >= 0; i--)
        {
            if (callback(_list[i], i))
            {
                return i;
            }
        }
        return -1;
    }

    public int findLastIndex(Func<T, int, JSArray<T>, bool> callback)
    {
        for (var i = _list.Count - 1; i >= 0; i--)
        {
            if (callback(_list[i], i, this))
            {
                return i;
            }
        }
        return -1;
    }

    public int indexOf(T searchElement, int fromIndex = 0)
    {
        for (var i = fromIndex; i < _list.Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(_list[i], searchElement))
            {
                return i;
            }
        }
        return -1;
    }

    public int lastIndexOf(T searchElement, int? fromIndex = null)
    {
        var startIndex = fromIndex ?? (_list.Count - 1);
        if (startIndex < 0)
        {
            startIndex = _list.Count + startIndex;
        }
        startIndex = Math.Min(startIndex, _list.Count - 1);

        for (var i = startIndex; i >= 0; i--)
        {
            if (EqualityComparer<T>.Default.Equals(_list[i], searchElement))
            {
                return i;
            }
        }
        return -1;
    }

    public bool includes(T searchElement)
    {
        return indexOf(searchElement) >= 0;
    }

    public bool every(Func<T, bool> callback)
    {
        for (var i = 0; i < _list.Count; i++)
        {
            if (!callback(_list[i]))
            {
                return false;
            }
        }
        return true;
    }

    public bool every(Func<T, int, JSArray<T>, bool> callback)
    {
        for (var i = 0; i < _list.Count; i++)
        {
            if (!callback(_list[i], i, this))
            {
                return false;
            }
        }
        return true;
    }

    public bool some(Func<T, bool> callback)
    {
        for (var i = 0; i < _list.Count; i++)
        {
            if (callback(_list[i]))
            {
                return true;
            }
        }
        return false;
    }

    public bool some(Func<T, int, JSArray<T>, bool> callback)
    {
        for (var i = 0; i < _list.Count; i++)
        {
            if (callback(_list[i], i, this))
            {
                return true;
            }
        }
        return false;
    }

    public JSArray<T> sort(Func<T, T, double>? compareFunc = null)
    {
        if (compareFunc != null)
        {
            _list.Sort((a, b) =>
            {
                var result = compareFunc(a, b);
                return result < 0 ? -1 : result > 0 ? 1 : 0;
            });
        }
        else
        {
            _list.Sort((a, b) =>
            {
                var aStr = a?.ToString() ?? "";
                var bStr = b?.ToString() ?? "";
                return string.Compare(aStr, bStr, StringComparison.Ordinal);
            });
        }
        return this;
    }

    public JSArray<T> reverse()
    {
        _list.Reverse();
        return this;
    }

    public string join(string separator = ",")
    {
        var parts = new List<string>();
        for (var i = 0; i < _list.Count; i++)
        {
            parts.Add(_list[i]?.ToString() ?? "");
        }
        return string.Join(separator, parts);
    }

    public override string ToString()
    {
        return join(",");
    }

    public string toLocaleString()
    {
        return join(",");
    }

    public JSArray<T> concat(params object[] items)
    {
        var result = new JSArray<T>(_list);

        foreach (var item in items)
        {
            if (item is JSArray<T> jsArr)
            {
                foreach (var val in jsArr)
                {
                    result.push(val);
                }
            }
            else if (item is IEnumerable<T> enumerable)
            {
                foreach (var val in enumerable)
                {
                    result.push(val);
                }
            }
            else if (item is T value)
            {
                result.push(value);
            }
        }

        return result;
    }

    public T[] toArray()
    {
        return _list.ToArray();
    }

    public List<T> toList()
    {
        return new List<T>(_list);
    }

    public IEnumerable<(int index, T value)> entries()
    {
        for (var i = 0; i < _list.Count; i++)
        {
            yield return (i, _list[i]);
        }
    }

    public IEnumerable<int> keys()
    {
        for (var i = 0; i < _list.Count; i++)
        {
            yield return i;
        }
    }

    public IEnumerable<T> values()
    {
        for (var i = 0; i < _list.Count; i++)
        {
            yield return _list[i];
        }
    }

    public T at(int index)
    {
        var actualIndex = index < 0 ? _list.Count + index : index;
        if (actualIndex < 0 || actualIndex >= _list.Count)
        {
            return default!;
        }
        return _list[actualIndex];
    }

    public JSArray<object> flat(int depth = 1)
    {
        var result = new JSArray<object>();
        FlattenHelper(_list, result, depth);
        return result;
    }

    private static void FlattenHelper<TSource>(IEnumerable<TSource> source, JSArray<object> result, int depth)
    {
        foreach (var item in source)
        {
            if (depth > 0 && item != null && item is IEnumerable enumerable && item is not string)
            {
                foreach (var nestedItem in enumerable)
                {
                    FlattenHelper(new List<object?> { nestedItem }, result, depth - 1);
                }
            }
            else
            {
                result.push(item!);
            }
        }
    }

    public JSArray<TResult> flatMap<TResult>(Func<T, int, JSArray<T>, object> callback)
    {
        var result = new JSArray<TResult>();
        for (var i = 0; i < _list.Count; i++)
        {
            var mapped = callback(_list[i], i, this);
            if (mapped is JSArray<TResult> jsArr)
            {
                foreach (var val in jsArr)
                {
                    result.push(val);
                }
            }
            else if (mapped is IEnumerable<TResult> enumerable)
            {
                foreach (var val in enumerable)
                {
                    result.push(val);
                }
            }
            else if (mapped is TResult singleValue)
            {
                result.push(singleValue);
            }
        }
        return result;
    }

    public JSArray<T> fill(T value, int start = 0, int? end = null)
    {
        var actualStart = start < 0 ? Math.Max(0, _list.Count + start) : start;
        var actualEnd = end.HasValue
            ? (end.Value < 0 ? _list.Count + end.Value : end.Value)
            : _list.Count;

        for (var i = actualStart; i < actualEnd && i < _list.Count; i++)
        {
            _list[i] = value;
        }
        return this;
    }

    public JSArray<T> copyWithin(int target, int start = 0, int? end = null)
    {
        var actualTarget = target < 0 ? Math.Max(0, _list.Count + target) : target;
        var actualStart = start < 0 ? Math.Max(0, _list.Count + start) : start;
        var actualEnd = end.HasValue
            ? (end.Value < 0 ? _list.Count + end.Value : end.Value)
            : _list.Count;

        var count = Math.Min(actualEnd - actualStart, _list.Count - actualTarget);
        var temp = new List<T>();
        for (var i = 0; i < count; i++)
        {
            temp.Add(_list[actualStart + i]);
        }

        for (var i = 0; i < count; i++)
        {
            _list[actualTarget + i] = temp[i];
        }

        return this;
    }

    public JSArray<T> with(int index, T value)
    {
        var actualIndex = index < 0 ? _list.Count + index : index;
        if (actualIndex < 0 || actualIndex >= _list.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var result = new JSArray<T>(_list);
        result[actualIndex] = value;
        return result;
    }

    public JSArray<T> toReversed()
    {
        var result = new JSArray<T>(_list);
        result.reverse();
        return result;
    }

    public JSArray<T> toSorted(Func<T, T, double>? compareFunc = null)
    {
        var result = new JSArray<T>(_list);
        result.sort(compareFunc);
        return result;
    }

    public JSArray<T> toSpliced(int start, int? deleteCount = null, params T[] items)
    {
        var result = new JSArray<T>(_list);
        result.splice(start, deleteCount, items);
        return result;
    }

    public static JSArray<T> from(IEnumerable<T> iterable)
    {
        return new JSArray<T>(iterable);
    }

    public static JSArray<TResult> from<TSource, TResult>(IEnumerable<TSource> iterable, Func<TSource, int, TResult> mapFunc)
    {
        var result = new JSArray<TResult>();
        var index = 0;
        foreach (var item in iterable)
        {
            result.push(mapFunc(item, index++));
        }
        return result;
    }

    public static JSArray<TResult> from<TSource, TResult>(IEnumerable<TSource> iterable, Func<TSource, TResult> mapFunc)
    {
        var result = new JSArray<TResult>();
        foreach (var item in iterable)
        {
            result.push(mapFunc(item));
        }
        return result;
    }

    public static JSArray<T> of(params T[] items)
    {
        return new JSArray<T>(items);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
