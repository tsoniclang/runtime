using Xunit;

namespace Tsonic.CSharp.Runtime.Tests;

public sealed class ErrorTests
{
    [Fact]
    public void ErrorSourcePropertiesRemainExactAndWritable()
    {
        var error = new Error("original");

        error.name = "NamedError";
        error.message = "updated";
        error.stack = "authored stack";

        Assert.Equal("NamedError", error.name);
        Assert.Equal("updated", error.message);
        Assert.Equal("updated", error.Message);
        Assert.Equal("authored stack", error.stack);
    }

    [Fact]
    public void SpecializedErrorsKeepWritableSourceNames()
    {
        var range = new RangeError("range");
        var type = new TypeError("type");
        var uri = new URIError("uri");

        Assert.Equal("RangeError", range.name);
        Assert.Equal("TypeError", type.name);
        Assert.Equal("URIError", uri.name);

        range.name = "CustomRange";
        type.name = "CustomType";
        uri.name = "CustomUri";

        Assert.Equal("CustomRange", range.name);
        Assert.Equal("CustomType", type.name);
        Assert.Equal("CustomUri", uri.name);
    }
}
