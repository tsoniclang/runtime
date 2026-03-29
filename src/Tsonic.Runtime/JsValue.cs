using System;
using System.Globalization;

namespace Tsonic.Runtime;

public static class JsValue
{
    public static string Stringify(object? value)
    {
        if (value == null)
        {
            return "undefined";
        }

        if (value is string s)
        {
            return s;
        }

        if (value is bool b)
        {
            return b ? "true" : "false";
        }

        if (value is double d)
        {
            if (double.IsNaN(d))
            {
                return "NaN";
            }

            if (double.IsPositiveInfinity(d))
            {
                return "Infinity";
            }

            if (double.IsNegativeInfinity(d))
            {
                return "-Infinity";
            }

            return Convert.ToString(d, CultureInfo.InvariantCulture) ?? "";
        }

        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
    }
}
