using System;
using System.Collections.Generic;
using System.Linq;

namespace Tsonic.Runtime;

/// <summary>
/// Helper methods for array operations in compiled Tsonic code.
/// </summary>
public static class ArrayHelpers
{
    /// <summary>
    /// Creates a slice of an array starting at the given index.
    /// Used for array rest patterns: const [first, ...rest] = arr
    /// </summary>
    /// <typeparam name="T">Element type of the array</typeparam>
    /// <param name="source">Source array to slice</param>
    /// <param name="startIndex">Index to start slicing from</param>
    /// <returns>New array containing elements from startIndex to end</returns>
    public static T[] Slice<T>(T[] source, int startIndex)
    {
        if (startIndex >= source.Length) return [];
        var length = source.Length - startIndex;
        var result = new T[length];
        Array.Copy(source, startIndex, result, 0, length);
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
}
