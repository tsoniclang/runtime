namespace Tsonic.Runtime;

public static class Symbol
{
    public static object Create(object? description = null)
    {
        return new Handle(description);
    }

    private sealed class Handle
    {
        public Handle(object? description)
        {
            Description = description;
        }

        public object? Description { get; }
    }
}
