using System;
using System.Collections.Generic;

namespace Tsonic.Runtime;

public static class JSArrayStatics
{
    public static T[] from<T>(IEnumerable<T> iterable)
    {
        return JSArray<T>.from(iterable).toArray();
    }

    public static string[] from(string source)
    {
        var chars = new string[source.Length];
        for (var i = 0; i < source.Length; i++)
        {
            chars[i] = source[i].ToString();
        }

        return chars;
    }

    public static TResult[] from<TSource, TResult>(
        IEnumerable<TSource> iterable,
        Func<TSource, int, TResult> mapFunc
    )
    {
        return JSArray<TResult>.from(iterable, mapFunc).toArray();
    }

    public static TResult[] from<TSource, TResult>(
        IEnumerable<TSource> iterable,
        Func<TSource, TResult> mapFunc
    )
    {
        return JSArray<TResult>.from(iterable, mapFunc).toArray();
    }

    public static TResult[] from<TResult>(
        string source,
        Func<string, int, TResult> mapFunc
    )
    {
        var result = new TResult[source.Length];
        for (var i = 0; i < source.Length; i++)
        {
            result[i] = mapFunc(source[i].ToString(), i);
        }

        return result;
    }

    public static TResult[] from<TResult>(
        string source,
        Func<string, TResult> mapFunc
    )
    {
        var result = new TResult[source.Length];
        for (var i = 0; i < source.Length; i++)
        {
            result[i] = mapFunc(source[i].ToString());
        }

        return result;
    }

    public static T[] of<T>(params T[] items)
    {
        return JSArray<T>.of(items).toArray();
    }
}
