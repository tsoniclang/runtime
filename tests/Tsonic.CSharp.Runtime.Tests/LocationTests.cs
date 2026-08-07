using System;
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
            var location = Location<int>.CreateLocal(
                new object(),
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
        public void SameComparesCanonicalStorageIdentityIncludingMissingLocations()
        {
            var localValue = 1;
            var localIdentity = new object();
            var local = Location<int>.CreateLocal(
                localIdentity,
                () => localValue,
                value => localValue = value);
            var sameLocal = Location<int>.CreateLocal(
                localIdentity,
                () => localValue,
                value => localValue = value);
            var otherLocal = Location<int>.CreateLocal(
                new object(),
                () => localValue,
                value => localValue = value);
            var staticValue = 1;
            var staticLocation = Location<int>.CreateStatic(
                "Example.StaticValue",
                () => staticValue,
                value => staticValue = value);
            var repeatedStaticLocation = Location<int>.CreateStatic(
                "Example.StaticValue",
                () => staticValue,
                value => staticValue = value);
            var otherStaticLocation = Location<int>.CreateStatic(
                "Example.OtherStaticValue",
                () => staticValue,
                value => staticValue = value);
            var box = new Box { Value = 1 };
            var sameMember = Location<int>.CreateMember(
                box,
                "Box.Value",
                value => value.Value,
                (value, next) => value.Value = next);
            var repeatedMember = Location<int>.CreateMember(
                box,
                "Box.Value",
                value => value.Value,
                (value, next) => value.Value = next);
            var otherMember = Location<int>.CreateMember(
                box,
                "Box.Other",
                value => value.Value,
                (value, next) => value.Value = next);
            var values = new[] { 1, 2 };
            var firstElement = Location<int>.CreateArrayElement(values, 0);
            var repeatedElement = Location<int>.CreateArrayElement(values, 0U);
            var secondElement = Location<int>.CreateArrayElement(values, 1L);
            var first = Location<int>.Allocate(1);
            var second = Location<int>.Allocate(1);

            Assert.True(Location<int>.Same(local, sameLocal));
            Assert.False(Location<int>.Same(local, otherLocal));
            Assert.True(Location<int>.Same(staticLocation, repeatedStaticLocation));
            Assert.False(Location<int>.Same(staticLocation, otherStaticLocation));
            Assert.True(Location<int>.Same(sameMember, repeatedMember));
            Assert.False(Location<int>.Same(sameMember, otherMember));
            Assert.True(Location<int>.Same(firstElement, repeatedElement));
            Assert.False(Location<int>.Same(firstElement, secondElement));
            Assert.True(Location<int>.Same(first, first));
            Assert.False(Location<int>.Same(first, second));
            Assert.False(Location<int>.Same(first, null));
            Assert.True(Location<int>.Same(null, null));
        }

        [Fact]
        public void CreateWithStateCapturesTheOriginalReceiver()
        {
            var first = new Box { Value = 1 };
            var second = new Box { Value = 2 };
            var receiver = first;
            var location = Location<int>.CreateMember(
                receiver,
                "Box.Value",
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
            var location = Location<int>.CreateArrayElement(array, index);

            array = second;
            index = 1;
            location.Store(5);

            Assert.Equal(5, first[0]);
            Assert.Equal(4, second[1]);
        }

        [Fact]
        public void CreateArrayElementValidatesTheAddressWhenItIsFormed()
        {
            var values = new[] { 1 };

            Assert.Throws<IndexOutOfRangeException>(() =>
                Location<int>.CreateArrayElement(values, -1));
            Assert.Throws<IndexOutOfRangeException>(() =>
                Location<int>.CreateArrayElement(values, 1U));
            Assert.Throws<IndexOutOfRangeException>(() =>
                Location<int>.CreateArrayElement(values, ulong.MaxValue));
        }

        [Fact]
        public void ProjectWritesAValueTypeMemberBackToItsOwner()
        {
            var storage = new Pair { Left = 1, Right = 2 };
            var owner = Location<Pair>.CreateLocal(
                new object(),
                () => storage,
                value => storage = value);
            var left = owner.ProjectMember(
                "Pair.Left",
                pair => pair.Left,
                (pair, value) =>
                {
                    pair.Left = value;
                    return pair;
                });
            var sameLeft = owner.ProjectMember(
                "Pair.Left",
                pair => pair.Left,
                (pair, value) =>
                {
                    pair.Left = value;
                    return pair;
                });

            left.Store(3);

            Assert.Equal(3, storage.Left);
            Assert.Equal(2, storage.Right);
            Assert.True(Location<int>.Same(left, sameLeft));
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
