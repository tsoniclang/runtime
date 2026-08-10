using System;
using System.Collections.Generic;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Runtime.Tests;

public sealed class GeneratorTests
{
    [Fact]
    public void NextCarriesValuesAndPreservesTheTypedReturn()
    {
        static IEnumerable<int> Body(Generator<int, string, int> generator)
        {
            yield return 1;
            var next = generator.ConsumeNext();
            yield return next;
            generator.Complete("done");
        }

        var generator = Generator<int, string, int>.Create(Body);

        var first = generator.Next();
        var second = generator.Next(7);
        var completed = generator.Next();

        Assert.False(first.Done);
        Assert.Equal(1, first.Value.As1());
        Assert.False(second.Done);
        Assert.Equal(7, second.Value.As1());
        Assert.True(completed.Done);
        Assert.Equal("done", completed.Value.As2());
    }

    [Fact]
    public void ConstructionIsLazyAndReturnClosesTheNativeIterator()
    {
        var starts = 0;
        var closes = 0;

        IEnumerable<int> Body(Generator<int, string, int> generator)
        {
            starts++;
            try
            {
                yield return 1;
                generator.ConsumeNext();
                yield return 2;
            }
            finally
            {
                closes++;
            }
            generator.Complete("natural");
        }

        var generator = Generator<int, string, int>.Create(Body);
        Assert.Equal(0, starts);

        Assert.False(generator.Next().Done);
        Assert.Equal(1, starts);
        var returned = generator.Return("stopped");

        Assert.True(returned.Done);
        Assert.Equal("stopped", returned.Value.As2());
        Assert.Equal(1, closes);
        Assert.True(generator.Next().Done);
    }

    [Fact]
    public void ThrowClosesTheNativeIteratorBeforePropagating()
    {
        var closes = 0;

        IEnumerable<int> Body(Generator<int, string, int> generator)
        {
            try
            {
                yield return 1;
                generator.ConsumeNext();
            }
            finally
            {
                closes++;
            }
            generator.Complete("done");
        }

        var generator = Generator<int, string, int>.Create(Body);
        generator.Next();
        var expected = new InvalidOperationException("stop");

        var actual = Assert.Throws<InvalidOperationException>(() => generator.Throw(expected));
        Assert.Same(expected, actual);
        Assert.Equal(1, closes);
        Assert.True(generator.Next().Done);
    }

    [Fact]
    public void EnumerableAdapterClosesOnEarlyForeachExit()
    {
        var closes = 0;

        IEnumerable<int> Body(Generator<int, string, object?> generator)
        {
            try
            {
                yield return 1;
                generator.ConsumeNext();
                yield return 2;
            }
            finally
            {
                closes++;
            }
            generator.Complete("done");
        }

        var generator = Generator<int, string, object?>.Create(Body);
        foreach (var value in generator)
        {
            Assert.Equal(1, value);
            break;
        }

        Assert.Equal(1, closes);
    }
}
