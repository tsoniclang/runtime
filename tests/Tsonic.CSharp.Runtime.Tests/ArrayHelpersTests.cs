using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Runtime.Tests
{
    public class ArrayHelpersTests
    {
        [Fact]
        public void Includes_UsesJavaScriptFromIndexSemantics()
        {
            Assert.True(ArrayHelpers.Includes([1, 2, 3, 2], 2));
            Assert.True(ArrayHelpers.Includes([1, 2, 3, 2], 2, -1));
            Assert.False(ArrayHelpers.Includes([1, 2, 3, 2], 2, 4));
        }

        [Fact]
        public void IndexOf_UsesJavaScriptFromIndexSemantics()
        {
            Assert.Equal(1, ArrayHelpers.IndexOf([1, 2, 3, 2], 2));
            Assert.Equal(3, ArrayHelpers.IndexOf([1, 2, 3, 2], 2, -1));
            Assert.Equal(1, ArrayHelpers.IndexOf([1, 2, 3, 2], 2, -99));
            Assert.Equal(-1, ArrayHelpers.IndexOf([1, 2, 3, 2], 2, 4));
        }

        [Fact]
        public void LastIndexOf_UsesJavaScriptFromIndexSemantics()
        {
            Assert.Equal(3, ArrayHelpers.LastIndexOf([1, 2, 3, 2], 2));
            Assert.Equal(1, ArrayHelpers.LastIndexOf([1, 2, 3, 2], 2, 2));
            Assert.Equal(3, ArrayHelpers.LastIndexOf([1, 2, 3, 2], 2, -1));
            Assert.Equal(-1, ArrayHelpers.LastIndexOf([1, 2, 3, 2], 2, -99));
        }

        [Fact]
        public void Slice_CopiesRangeWithPositiveIndexes()
        {
            var result = ArrayHelpers.Slice([1, 2, 3, 4, 5], 1, 4);

            Assert.Collection(
                result,
                item => Assert.Equal(2, item),
                item => Assert.Equal(3, item),
                item => Assert.Equal(4, item));
        }

        [Fact]
        public void Slice_UsesEndWhenOmitted()
        {
            var result = ArrayHelpers.Slice([1, 2, 3, 4, 5], 2);

            Assert.Collection(
                result,
                item => Assert.Equal(3, item),
                item => Assert.Equal(4, item),
                item => Assert.Equal(5, item));
        }

        [Fact]
        public void Slice_HandlesNegativeIndexesLikeJavaScript()
        {
            var result = ArrayHelpers.Slice([1, 2, 3, 4, 5], -4, -1);

            Assert.Collection(
                result,
                item => Assert.Equal(2, item),
                item => Assert.Equal(3, item),
                item => Assert.Equal(4, item));
        }

        [Fact]
        public void Slice_ReturnsEmptyWhenEndIsBeforeStart()
        {
            var result = ArrayHelpers.Slice([1, 2, 3, 4, 5], 4, 1);

            Assert.Empty(result);
        }
    }
}
