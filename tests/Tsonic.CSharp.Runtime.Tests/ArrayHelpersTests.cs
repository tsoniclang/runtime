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
        public void PredicateHelpers_UseJavaScriptCallbackArguments()
        {
            int[] values = [1, 20, 3];
            Assert.True(ArrayHelpers.Some(values, (value, index) => index == 1 && value == 20));
            Assert.False(ArrayHelpers.Every(values, (value, index, source) => source[index] == value && value < 10));
            Assert.Equal(1, ArrayHelpers.FindIndex(values, (value, index) => index > 0 && value > 10));
            Assert.Equal(2, ArrayHelpers.FindLastIndex(values, (value, index, source) => source.Length == 3 && index >= 1));
        }

        [Fact]
        public void ForEach_UsesJavaScriptCallbackArguments()
        {
            int[] values = [2, 4, 6];
            var total = 0;
            ArrayHelpers.ForEach(values, (value, index, source) =>
            {
                total += value + index + source.Length;
            });

            Assert.Equal(24, total);
        }

        [Fact]
        public void Concat_CopiesChunksWithoutLinqIterators()
        {
            var result = ArrayHelpers.Concat([1, 2], [], [3], [4, 5]);

            Assert.Collection(
                result,
                item => Assert.Equal(1, item),
                item => Assert.Equal(2, item),
                item => Assert.Equal(3, item),
                item => Assert.Equal(4, item),
                item => Assert.Equal(5, item));
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
