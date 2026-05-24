using System;

namespace Tsonic.Runtime
{
    /// <summary>
    /// TypeScript operators that need runtime support
    /// </summary>
    public static class Operators
    {
        private const double TwoPow32 = 4294967296.0;
        private const int ShiftCountMask = 31;

        /// <summary>
        /// typeof operator - returns TypeScript/JavaScript type string
        /// </summary>
        public static string @typeof(object? value)
        {
            if (value == null)
            {
                return "undefined";
            }

            if (value is string)
            {
                return "string";
            }

            if (value is double || value is int || value is float || value is long || value is decimal)
            {
                return "number";
            }

            if (value is bool)
            {
                return "boolean";
            }

            if (value is Delegate)
            {
                return "function";
            }

            return "object";
        }

        /// <summary>
        /// instanceof operator - checks if object is instance of type
        /// </summary>
        public static bool instanceof(object? obj, Type type)
        {
            if (obj == null)
            {
                return false;
            }

            return type.IsAssignableFrom(obj.GetType());
        }

        /// <summary>
        /// ECMAScript ToUint32 abstract operation for number bitwise operators.
        /// </summary>
        public static uint ToUint32(double value)
        {
            if (value == 0 || double.IsNaN(value) || double.IsInfinity(value))
            {
                return 0U;
            }

            var integer = Math.Truncate(value);
            var modulo = integer % TwoPow32;
            if (modulo < 0)
            {
                modulo += TwoPow32;
            }

            return unchecked((uint)modulo);
        }

        /// <summary>
        /// ECMAScript ToInt32 abstract operation for number bitwise operators.
        /// </summary>
        public static int ToInt32(double value)
        {
            return unchecked((int)ToUint32(value));
        }

        public static int BitwiseNot(double value)
        {
            return ~ToInt32(value);
        }

        public static int BitwiseAnd(double left, double right)
        {
            return ToInt32(left) & ToInt32(right);
        }

        public static int BitwiseOr(double left, double right)
        {
            return ToInt32(left) | ToInt32(right);
        }

        public static int BitwiseXor(double left, double right)
        {
            return ToInt32(left) ^ ToInt32(right);
        }

        public static int LeftShift(double left, double right)
        {
            return ToInt32(left) << (int)(ToUint32(right) & ShiftCountMask);
        }

        public static int SignedRightShift(double left, double right)
        {
            return ToInt32(left) >> (int)(ToUint32(right) & ShiftCountMask);
        }

        public static uint UnsignedRightShift(double left, double right)
        {
            return ToUint32(left) >> (int)(ToUint32(right) & ShiftCountMask);
        }
    }
}
