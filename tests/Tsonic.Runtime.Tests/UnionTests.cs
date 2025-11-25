using Tsonic.Runtime;
using Xunit;

namespace Tsonic.Runtime.Tests
{
    public class UnionTests
    {
        [Fact]
        public void Union2_From1_CreatesUnionWithFirstType()
        {
            Union<int, string> union = Union<int, string>.From1(42);

            Assert.True(union.Is1());
            Assert.False(union.Is2());
            Assert.Equal(42, union.As1());
        }

        [Fact]
        public void Union2_From2_CreatesUnionWithSecondType()
        {
            Union<int, string> union = Union<int, string>.From2("hello");

            Assert.False(union.Is1());
            Assert.True(union.Is2());
            Assert.Equal("hello", union.As2());
        }

        [Fact]
        public void Union2_ImplicitConversion_FromFirstType()
        {
            Union<int, string> union = 42;

            Assert.True(union.Is1());
            Assert.Equal(42, union.As1());
        }

        [Fact]
        public void Union2_ImplicitConversion_FromSecondType()
        {
            Union<int, string> union = "hello";

            Assert.True(union.Is2());
            Assert.Equal("hello", union.As2());
        }

        [Fact]
        public void Union2_TryAs1_ReturnsTrueWhenCorrectType()
        {
            Union<int, string> union = 42;

            Assert.True(union.TryAs1(out var value));
            Assert.Equal(42, value);
        }

        [Fact]
        public void Union2_TryAs1_ReturnsFalseWhenWrongType()
        {
            Union<int, string> union = "hello";

            Assert.False(union.TryAs1(out _));
        }

        [Fact]
        public void Union2_Match_CallsCorrectFunction()
        {
            Union<int, string> intUnion = 42;
            Union<int, string> stringUnion = "hello";

            var intResult = intUnion.Match(
                i => $"int:{i}",
                s => $"string:{s}"
            );

            var stringResult = stringUnion.Match(
                i => $"int:{i}",
                s => $"string:{s}"
            );

            Assert.Equal("int:42", intResult);
            Assert.Equal("string:hello", stringResult);
        }

        [Fact]
        public void Union2_ToString_ReturnsValueString()
        {
            Union<int, string> union = 42;
            Assert.Equal("42", union.ToString());
        }

        [Fact]
        public void Union2_Equals_ComparesCorrectly()
        {
            Union<int, string> union1 = 42;
            Union<int, string> union2 = 42;
            Union<int, string> union3 = "hello";

            Assert.Equal(union1, union2);
            Assert.NotEqual(union1, union3);
        }

        [Fact]
        public void Union3_SupportsThreeTypes()
        {
            Union<int, string, bool> union1 = 42;
            Union<int, string, bool> union2 = "hello";
            Union<int, string, bool> union3 = true;

            Assert.True(union1.Is1());
            Assert.True(union2.Is2());
            Assert.True(union3.Is3());

            Assert.Equal(42, union1.As1());
            Assert.Equal("hello", union2.As2());
            Assert.True(union3.As3());
        }

        [Fact]
        public void Union3_Match_CallsCorrectFunction()
        {
            Union<int, string, bool> union = true;

            var result = union.Match(
                i => "int",
                s => "string",
                b => "bool"
            );

            Assert.Equal("bool", result);
        }

        [Fact]
        public void Union4_SupportsFourTypes()
        {
            Union<int, string, bool, double> union = 3.14;

            Assert.True(union.Is4());
            Assert.Equal(3.14, union.As4());
        }

        [Fact]
        public void Union5_SupportsFiveTypes()
        {
            Union<int, string, bool, double, char> union = 'x';

            Assert.True(union.Is5());
            Assert.Equal('x', union.As5());
        }

        [Fact]
        public void Union6_SupportsSixTypes()
        {
            Union<int, string, bool, double, char, long> union = 100L;

            Assert.True(union.Is6());
            Assert.Equal(100L, union.As6());
        }

        [Fact]
        public void Union7_SupportsSevenTypes()
        {
            Union<int, string, bool, double, char, long, float> union = 1.5f;

            Assert.True(union.Is7());
            Assert.Equal(1.5f, union.As7());
        }

        [Fact]
        public void Union8_SupportsEightTypes()
        {
            Union<int, string, bool, double, char, long, float, decimal> union = 99.99m;

            Assert.True(union.Is8());
            Assert.Equal(99.99m, union.As8());
        }
    }
}
