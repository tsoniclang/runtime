using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Runtime.Tests
{
    public sealed class LocationTests
    {
        [Fact]
        public void CreateAliasesExistingStorage()
        {
            var storage = 10;
            var location = Location<int>.Create(
                () => storage,
                value => storage = value);

            location.Store(11);

            Assert.Equal(11, storage);
            Assert.Equal(11, location.Load());
        }

        [Fact]
        public void AllocateCreatesIndependentLocations()
        {
            var first = Location<int>.Allocate(1);
            var second = Location<int>.Allocate(1);

            first.Store(2);

            Assert.Equal(2, first.Load());
            Assert.Equal(1, second.Load());
        }

        [Fact]
        public void CreateWithStateCapturesTheOriginalReceiver()
        {
            var first = new Box { Value = 1 };
            var second = new Box { Value = 2 };
            var receiver = first;
            var location = Location<int>.Create(
                receiver,
                box => box.Value,
                (box, value) => box.Value = value);

            receiver = second;
            location.Store(3);

            Assert.Equal(3, first.Value);
            Assert.Equal(2, second.Value);
        }

        [Fact]
        public void CreateWithTwoStatesCapturesArrayAndIndex()
        {
            var first = new[] { 1, 2 };
            var second = new[] { 3, 4 };
            var array = first;
            var index = 0;
            var location = Location<int>.Create(
                array,
                index,
                (values, offset) => values[offset],
                (values, offset, value) => values[offset] = value);

            array = second;
            index = 1;
            location.Store(5);

            Assert.Equal(5, first[0]);
            Assert.Equal(4, second[1]);
        }

        [Fact]
        public void ProjectWritesAValueTypeMemberBackToItsOwner()
        {
            var storage = new Pair { Left = 1, Right = 2 };
            var owner = Location<Pair>.Create(
                () => storage,
                value => storage = value);
            var left = owner.Project(
                pair => pair.Left,
                (pair, value) =>
                {
                    pair.Left = value;
                    return pair;
                });

            left.Store(3);

            Assert.Equal(3, storage.Left);
            Assert.Equal(2, storage.Right);
        }

        private sealed class Box
        {
            public int Value { get; set; }
        }

        private struct Pair
        {
            public int Left;
            public int Right;
        }
    }
}
