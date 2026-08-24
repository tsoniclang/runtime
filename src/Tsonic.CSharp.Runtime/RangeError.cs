using System;

namespace Tsonic.CSharp.Runtime
{
    /// <summary>
    /// JavaScript-style RangeError.
    /// </summary>
    public class RangeError : Error
    {
        public RangeError()
        {
        }

        public RangeError(string? message)
            : base(message)
        {
        }

        public RangeError(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        public override string name { get; set; } = nameof(RangeError);
    }
}
