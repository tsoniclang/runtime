using System;
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
}
