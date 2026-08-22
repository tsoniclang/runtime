using System;

namespace Tsonic.CSharp.Runtime
{
    /// <summary>
    /// JavaScript-style Error base type with lowercase property aliases.
    /// </summary>
    public class Error : Exception
    {
        public Error()
        {
        }

        public Error(string? message)
            : base(message)
        {
        }

        public Error(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        public virtual string name => nameof(Error);

        public string message => Message;

        public string? stack => StackTrace;
    }
}
