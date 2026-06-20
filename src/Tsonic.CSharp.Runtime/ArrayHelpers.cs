using System;
using System.Collections.Generic;
using System.Linq;

namespace Tsonic.CSharp.Runtime;

/// <summary>
/// Helper methods for array operations in compiled Tsonic code.
/// </summary>
public static class ArrayHelpers
{
    public static bool Includes<T>(T[] source, T value, int fromIndex = 0)
    {
        return IndexOf(source, value, fromIndex) >= 0;
    }

    public static int IndexOf<T>(T[] source, T value, int fromIndex = 0)
    {
        var start = NormalizeForwardSearchStart(fromIndex, source.Length);
        var comparer = EqualityComparer<T>.Default;
        for (var index = start; index < source.Length; index++)
        {
            if (comparer.Equals(source[index], value))
            {
                return index;
            }
        }
        return -1;
    }

    public static int LastIndexOf<T>(T[] source, T value, int? fromIndex = null)
    {
        var start = NormalizeBackwardSearchStart(fromIndex, source.Length);
        if (start < 0)
        {
            return -1;
        }

        var comparer = EqualityComparer<T>.Default;
        for (var index = start; index >= 0; index--)
        {
            if (comparer.Equals(source[index], value))
            {
                return index;
            }
        }
        return -1;
    }

    /// <summary>
    /// Creates a JavaScript-compatible slice of an array.
    /// Used for array rest patterns and Array.prototype.slice on fixed CLR arrays.
    /// </summary>
    /// <typeparam name="T">Element type of the array</typeparam>
    /// <param name="source">Source array to slice</param>
    /// <param name="startIndex">Inclusive start index. Negative values count from the end.</param>
    /// <param name="endIndex">Exclusive end index. Negative values count from the end.</param>
    /// <returns>New array containing the selected elements.</returns>
    public static T[] Slice<T>(T[] source, int startIndex = 0, int? endIndex = null)
    {
        var start = NormalizeSliceStart(startIndex, source.Length);
        var end = NormalizeSliceEnd(endIndex, source.Length);
        var length = Math.Max(end - start, 0);
        var result = new T[length];
        if (length == 0)
        {
            return result;
        }
        Array.Copy(source, start, result, 0, length);
        return result;
    }

    /// <summary>
    /// Creates a slice of a list starting at the given index.
    /// Used for rest patterns with List&lt;T&gt; and IList&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T">Element type of the list</typeparam>
    /// <param name="source">Source list to slice</param>
    /// <param name="startIndex">Index to start slicing from</param>
    /// <returns>New array containing elements from startIndex to end</returns>
    public static T[] Slice<T>(IList<T> source, int startIndex)
    {
        if (startIndex >= source.Count) return [];
        var length = source.Count - startIndex;
        var result = new T[length];
        for (var i = 0; i < length; i++)
        {
            result[i] = source[startIndex + i];
        }
        return result;
    }

    /// <summary>
    /// Creates a slice of an enumerable starting at the given index.
    /// Used for rest patterns with any IEnumerable&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T">Element type of the enumerable</typeparam>
    /// <param name="source">Source enumerable to slice</param>
    /// <param name="startIndex">Index to start slicing from</param>
    /// <returns>New array containing elements from startIndex to end</returns>
    public static T[] Slice<T>(IEnumerable<T> source, int startIndex)
    {
        return source.Skip(startIndex).ToArray();
    }

    private static int NormalizeSliceStart(int index, int length)
    {
        return index < 0
            ? Math.Max(length + index, 0)
            : Math.Min(index, length);
    }

    private static int NormalizeSliceEnd(int? index, int length)
    {
        if (!index.HasValue)
        {
            return length;
        }
        return index.Value < 0
            ? Math.Max(length + index.Value, 0)
            : Math.Min(index.Value, length);
    }

    private static int NormalizeForwardSearchStart(int index, int length)
    {
        if (index >= length)
        {
            return length;
        }
        return index < 0
            ? Math.Max(length + index, 0)
            : index;
    }

    private static int NormalizeBackwardSearchStart(int? index, int length)
    {
        if (length == 0)
        {
            return -1;
        }
        if (!index.HasValue)
        {
            return length - 1;
        }
        return index.Value < 0
            ? length + index.Value
            : Math.Min(index.Value, length - 1);
    }
}
