namespace Tsonic.CSharp.Runtime
{
    public sealed class Undefined
    {
        public static readonly Undefined value = new();

        private Undefined()
        {
        }

        public override string ToString()
        {
            return "undefined";
        }
    }
}
