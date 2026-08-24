using System;

namespace Tsonic.CSharp.Runtime
{
    /// <summary>
    /// JavaScript-style TypeError.
    /// </summary>
    public class TypeError : Error
    {
        public TypeError()
        {
        }

        public TypeError(string? message)
            : base(message)
        {
        }

        public TypeError(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        public override string name { get; set; } = nameof(TypeError);
    }
}
