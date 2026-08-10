using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Runtime.Tests;

public sealed class ResourceManagementTests
{
    [Fact]
    public void SuppressedErrorRetainsBothFailuresInSpecificationOrder()
    {
        var body = new InvalidOperationException("body");
        var disposal = new InvalidOperationException("dispose");

        var error = new SuppressedError(disposal, body);

        Assert.Same(disposal, error.Error);
        Assert.Same(body, error.Suppressed);
        Assert.Same(disposal, error.InnerException);
    }

    [Fact]
    public void NestedSuppressionRetainsReverseDisposalOrder()
    {
        var body = new InvalidOperationException("body");
        var second = new InvalidOperationException("second");
        var first = new InvalidOperationException("first");

        var inner = new SuppressedError(second, body);
        var outer = new SuppressedError(first, inner);

        Assert.Same(first, outer.Error);
        Assert.Same(inner, outer.Suppressed);
    }

    [Fact]
    public void ResourceStackDisposesInReverseOrder()
    {
        var order = new List<int>();
        var resources = new ResourceStack();
        resources.Add(1, value => order.Add(value));
        resources.Add(2, value => order.Add(value));

        resources.DisposeAndThrow(null);

        Assert.Equal([2, 1], order);
    }

    [Fact]
    public void ResourceStackComposesBodyAndDisposalFailures()
    {
        var body = new InvalidOperationException("body");
        var disposal = new InvalidOperationException("dispose");
        var resources = new ResourceStack();
        resources.Add(1, _ => throw disposal);

        var error = Assert.Throws<SuppressedError>(
            () => resources.DisposeAndThrow(body));

        Assert.Same(disposal, error.Error);
        Assert.Same(body, error.Suppressed);
    }

    [Fact]
    public void ResourceStackContinuesDisposalAndComposesEveryFailure()
    {
        var order = new List<int>();
        var body = new InvalidOperationException("body");
        var first = new InvalidOperationException("first");
        var second = new InvalidOperationException("second");
        var resources = new ResourceStack();
        resources.Add(1, value =>
        {
            order.Add(value);
            throw first;
        });
        resources.Add(2, value =>
        {
            order.Add(value);
            throw second;
        });

        var outer = Assert.Throws<SuppressedError>(
            () => resources.DisposeAndThrow(body));
        var inner = Assert.IsType<SuppressedError>(outer.Suppressed);

        Assert.Equal([2, 1], order);
        Assert.Same(first, outer.Error);
        Assert.Same(second, inner.Error);
        Assert.Same(body, inner.Suppressed);
    }

    [Fact]
    public void ResourceStackIgnoresNullResourcesAndClosesExactlyOnce()
    {
        var calls = 0;
        var resources = new ResourceStack();
        resources.Add<string>(null, _ => calls++);

        resources.DisposeAndThrow(null);

        Assert.Equal(0, calls);
        Assert.Throws<InvalidOperationException>(
            () => resources.DisposeAndThrow(null));
    }

    [Fact]
    public async Task AsyncResourceStackMixesSyncAndAsyncDisposalInReverseOrder()
    {
        var order = new List<int>();
        var resources = new AsyncResourceStack();
        resources.Add(1, value => order.Add(value));
        resources.AddAsync(2, async value =>
        {
            await Task.Yield();
            order.Add(value);
        });

        await resources.DisposeAndThrowAsync(null);

        Assert.Equal([2, 1], order);
    }

    [Fact]
    public async Task AsyncResourceStackAwaitsValueTasksAndComposesFailures()
    {
        var body = new InvalidOperationException("body");
        var disposal = new InvalidOperationException("dispose");
        var completed = false;
        var resources = new AsyncResourceStack();
        resources.AddAsync(1, async _ =>
        {
            await Task.Yield();
            completed = true;
            throw disposal;
        });

        var error = await Assert.ThrowsAsync<SuppressedError>(async () =>
            await resources.DisposeAndThrowAsync(body));

        Assert.True(completed);
        Assert.Same(disposal, error.Error);
        Assert.Same(body, error.Suppressed);
    }
}
