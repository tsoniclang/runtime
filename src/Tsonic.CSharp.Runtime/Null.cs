namespace Tsonic.CSharp.Runtime
{
    public sealed class Null
    {
        public static readonly Null value = new();

        private Null()
        {
        }

        public override string ToString()
        {
            return "null";
        }
    }
}
