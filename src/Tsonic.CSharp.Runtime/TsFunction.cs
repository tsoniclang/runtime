using System;
using System.Collections.Generic;
using System.Linq;

namespace Tsonic.CSharp.Runtime
{
    public sealed class TsFunction
    {
        private readonly Func<TsValue, IReadOnlyList<TsValue>, TsValue> _call;
        private readonly Func<IReadOnlyList<TsValue>, TsValue>? _construct;
        private readonly TsObject _properties = new();

        public TsFunction(Func<IReadOnlyList<TsValue>, TsValue> call, Func<IReadOnlyList<TsValue>, TsValue>? construct = null)
            : this((_, arguments) => call(arguments), construct)
        {
        }

        public TsFunction(Func<TsValue, IReadOnlyList<TsValue>, TsValue> call, Func<IReadOnlyList<TsValue>, TsValue>? construct = null)
        {
            _call = call;
            _construct = construct;
        }

        public TsValue InvokeDynamic(params object?[] arguments)
        {
            return InvokeDynamicWithThis(TsValue.undefined(), arguments);
        }

        public TsValue InvokeDynamicWithThis(TsValue receiver, params object?[] arguments)
        {
            return _call(receiver, arguments.Select(TsValue.from).ToArray());
        }

        public TsValue ConstructDynamic(params object?[] arguments)
        {
            return _construct is null
                ? throw new TypeError("Function value is not a constructor.")
                : _construct(arguments.Select(TsValue.from).ToArray());
        }

        public TsValue ReadDynamicSlot(string key)
        {
            return _properties.ReadDynamicSlot(key);
        }

        public TsValue WriteDynamicSlot(string key, object? value)
        {
            return _properties.WriteDynamicSlot(key, value);
        }
    }
}
