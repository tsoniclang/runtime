using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Runtime.Tests
{
    public class ArrayHelpersTests
    {
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
