using System;

namespace Tsonic.CSharp.Runtime
{
    /// <summary>
    /// JavaScript-style Error base type with lowercase property aliases.
    /// </summary>
    public class Error : Exception
    {
        private string _name = nameof(Error);
        private string _message = string.Empty;
        private string? _stack;

        public Error()
        {
        }

        public Error(string? message)
            : base(message)
        {
            _message = message ?? string.Empty;
        }

        public Error(string? message, Exception? innerException)
            : base(message, innerException)
        {
            _message = message ?? string.Empty;
        }

        public virtual string name
        {
            get => _name;
            set => _name = value;
        }

        public string message
        {
            get => _message;
            set => _message = value;
        }

        public string? stack
        {
            get => _stack ?? StackTrace;
            set => _stack = value;
        }

        public override string Message => _message;
    }
}
