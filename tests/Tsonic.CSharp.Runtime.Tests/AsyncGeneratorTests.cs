using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Runtime.Tests;

public sealed class AsyncGeneratorTests
{
    [Fact]
    public async Task ConcurrentNextRequestsAreProcessedInFifoOrder()
    {
        static async IAsyncEnumerable<int> Body(AsyncGenerator<int, string, int> generator)
        {
            await Task.Yield();
            yield return 1;
            var first = generator.ConsumeNext();
            yield return first;
            var second = generator.ConsumeNext();
            yield return second;
            generator.Complete("done");
        }

        var generator = AsyncGenerator<int, string, int>.Create(Body);
        var firstTask = generator.NextAsync();
        var secondTask = generator.NextAsync(7);
        var thirdTask = generator.NextAsync(9);

        var first = await firstTask;
        var second = await secondTask;
        var third = await thirdTask;
        var completed = await generator.NextAsync();

        Assert.Equal(1, first.Value.As1());
        Assert.Equal(7, second.Value.As1());
        Assert.Equal(9, third.Value.As1());
        Assert.True(completed.Done);
        Assert.Equal("done", completed.Value.As2());
    }

    [Fact]
    public async Task ReturnAsyncIsLazyAndAwaitsNativeCleanup()
    {
        var starts = 0;
        var closes = 0;

        async IAsyncEnumerable<int> Body(AsyncGenerator<int, string, int> generator)
        {
            starts++;
            try
            {
                await Task.Yield();
                yield return 1;
                generator.ConsumeNext();
            }
            finally
            {
                await Task.Yield();
                closes++;
            }
            generator.Complete("natural");
        }

        var generator = AsyncGenerator<int, string, int>.Create(Body);
        Assert.Equal(0, starts);
        Assert.False((await generator.NextAsync()).Done);

        var returned = await generator.ReturnAsync("stopped");

        Assert.True(returned.Done);
        Assert.Equal("stopped", returned.Value.As2());
        Assert.Equal(1, closes);
    }

    [Fact]
    public async Task ThrowAsyncClosesBeforeRejectingTheRequest()
    {
        var closes = 0;

        async IAsyncEnumerable<int> Body(AsyncGenerator<int, string, int> generator)
        {
            try
            {
                await Task.Yield();
                yield return 1;
                generator.ConsumeNext();
            }
            finally
            {
                closes++;
            }
            generator.Complete("done");
        }

        var generator = AsyncGenerator<int, string, int>.Create(Body);
        await generator.NextAsync();
        var expected = new InvalidOperationException("stop");

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => generator.ThrowAsync(expected));
        Assert.Same(expected, actual);
        Assert.Equal(1, closes);
        Assert.True((await generator.NextAsync()).Done);
    }
}
