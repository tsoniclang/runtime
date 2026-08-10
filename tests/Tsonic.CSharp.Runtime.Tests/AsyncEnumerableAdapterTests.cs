using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Runtime.Tests;

public sealed class AsyncEnumerableAdapterTests
{
    [Fact]
    public async Task FromSyncPreservesOrderAndDisposesOnEarlyExit()
    {
        var disposals = 0;

        IEnumerable<int> Values()
        {
            try
            {
                yield return 1;
                yield return 2;
            }
            finally
            {
                disposals++;
            }
        }

        var observed = new List<int>();
        await foreach (var value in AsyncEnumerableAdapters.FromSync(Values()))
        {
            observed.Add(value);
            break;
        }

        Assert.Equal([1], observed);
        Assert.Equal(1, disposals);
    }

    [Fact]
    public async Task FromSyncObservesCancellationBeforePublishingAnElement()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in AsyncEnumerableAdapters.FromSync(
                new[] { 1 },
                cancellation.Token))
            {
            }
        });
    }
}
