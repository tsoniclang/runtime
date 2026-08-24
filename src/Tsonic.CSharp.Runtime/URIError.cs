using System;

namespace Tsonic.CSharp.Runtime
{
    public class URIError : Error
    {
        public URIError()
        {
        }

        public URIError(string? message)
            : base(message)
        {
        }

        public URIError(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        public override string name { get; set; } = nameof(URIError);
    }
}
