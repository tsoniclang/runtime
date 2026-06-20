using System;
using System.Collections.Generic;
using System.Linq;

namespace Tsonic.CSharp.Runtime;

/// <summary>
/// Helper methods for array operations in compiled Tsonic code.
/// </summary>
public static class ArrayHelpers
{
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
}
