using Tsonic.Runtime;
using Xunit;

namespace Tsonic.Runtime.Tests
{
    public class OperatorsTests
    {
        [Fact]
        public void typeof_Null_ReturnsUndefined()
        {
            Assert.Equal("undefined", Operators.@typeof(null));
        }

        [Fact]
        public void typeof_String_ReturnsString()
        {
            Assert.Equal("string", Operators.@typeof("hello"));
        }

        [Fact]
        public void typeof_Int_ReturnsNumber()
        {
            Assert.Equal("number", Operators.@typeof(42));
        }

        [Fact]
        public void typeof_Double_ReturnsNumber()
        {
            Assert.Equal("number", Operators.@typeof(3.14));
        }

        [Fact]
        public void typeof_Float_ReturnsNumber()
        {
            Assert.Equal("number", Operators.@typeof(1.5f));
        }

        [Fact]
        public void typeof_Long_ReturnsNumber()
        {
            Assert.Equal("number", Operators.@typeof(100L));
        }

        [Fact]
        public void typeof_Decimal_ReturnsNumber()
        {
            Assert.Equal("number", Operators.@typeof(99.99m));
        }

        [Fact]
        public void typeof_Bool_ReturnsBoolean()
        {
            Assert.Equal("boolean", Operators.@typeof(true));
            Assert.Equal("boolean", Operators.@typeof(false));
        }

        [Fact]
        public void typeof_Delegate_ReturnsFunction()
        {
            System.Action action = () => { };
            Assert.Equal("function", Operators.@typeof(action));
        }

        [Fact]
        public void typeof_Object_ReturnsObject()
        {
            Assert.Equal("object", Operators.@typeof(new object()));
        }

        [Fact]
        public void typeof_Array_ReturnsObject()
        {
            Assert.Equal("object", Operators.@typeof(new int[] { 1, 2, 3 }));
        }

        [Fact]
        public void instanceof_NullObject_ReturnsFalse()
        {
            Assert.False(Operators.instanceof(null, typeof(string)));
        }

        [Fact]
        public void instanceof_CorrectType_ReturnsTrue()
        {
            Assert.True(Operators.instanceof("hello", typeof(string)));
        }

        [Fact]
        public void instanceof_WrongType_ReturnsFalse()
        {
            Assert.False(Operators.instanceof("hello", typeof(int)));
        }

        [Fact]
        public void instanceof_DerivedType_ReturnsTrue()
        {
            Assert.True(Operators.instanceof("hello", typeof(object)));
        }
    }
}
