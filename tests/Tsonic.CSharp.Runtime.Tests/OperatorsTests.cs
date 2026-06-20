using System;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Runtime.Tests
{
    public class OperatorsTests
    {
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
