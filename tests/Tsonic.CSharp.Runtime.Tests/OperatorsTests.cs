using System;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Runtime.Tests
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

        [Fact]
        public void ToInt32_UsesEcmascriptNumberBitwiseCoercion()
        {
            Assert.Equal(0, Operators.ToInt32(double.NaN));
            Assert.Equal(0, Operators.ToInt32(double.PositiveInfinity));
            Assert.Equal(0, Operators.ToInt32(double.NegativeInfinity));
            Assert.Equal(-2147483648, Operators.ToInt32(Math.Pow(2, 31)));
            Assert.Equal(-1, Operators.ToInt32(Math.Pow(2, 32) - 1));
            Assert.Equal(1, Operators.ToInt32(1.9));
            Assert.Equal(-1, Operators.ToInt32(-1.9));
        }

        [Fact]
        public void ToUint32_UsesEcmascriptNumberBitwiseCoercion()
        {
            Assert.Equal(0U, Operators.ToUint32(double.NaN));
            Assert.Equal(0U, Operators.ToUint32(double.PositiveInfinity));
            Assert.Equal(0U, Operators.ToUint32(double.NegativeInfinity));
            Assert.Equal(2147483648U, Operators.ToUint32(Math.Pow(2, 31)));
            Assert.Equal(4294967295U, Operators.ToUint32(-1));
            Assert.Equal(1U, Operators.ToUint32(1.9));
            Assert.Equal(4294967295U, Operators.ToUint32(-1.9));
        }

        [Fact]
        public void BitwiseOperators_UseEcmascriptCoercionAndShiftCounts()
        {
            Assert.Equal(0, Operators.BitwiseOr(double.NaN, 0));
            Assert.Equal(0, Operators.BitwiseOr(double.PositiveInfinity, 0));
            Assert.Equal(-1, Operators.BitwiseNot(0));
            Assert.Equal(0b1000, Operators.BitwiseAnd(0b1100, 0b1010));
            Assert.Equal(0b1110, Operators.BitwiseOr(0b1100, 0b1010));
            Assert.Equal(0b0110, Operators.BitwiseXor(0b1100, 0b1010));
            Assert.Equal(16, Operators.LeftShift(1, 4));
            Assert.Equal(2, Operators.LeftShift(1, 33));
            Assert.Equal(-4, Operators.SignedRightShift(-16, 2));
            Assert.Equal(4294967295U, Operators.UnsignedRightShift(-1, 0));
        }
    }
}
