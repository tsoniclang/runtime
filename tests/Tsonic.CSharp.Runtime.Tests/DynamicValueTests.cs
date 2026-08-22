using System;
using System.Collections.Generic;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Runtime.Tests
{
    public class DynamicValueTests
    {
        [Fact]
        public void CoreRuntime_HasNoJsSurfaceAssemblyDependency()
        {
            Assert.DoesNotContain(
                typeof(TsValue).Assembly.GetReferencedAssemblies(),
                assembly => assembly.Name == "Tsonic.CSharp.Js");
        }

        [Fact]
        public void ObjectCarrier_PreservesPresentAndMissingValues()
        {
            var value = TsValue.CreateDynamicObject("name", "Ada", "empty", null);

            Assert.Equal("Ada", value.ReadDynamicSlot("name").unwrap());
            Assert.Null(value.ReadDynamicSlot("empty").unwrap());
            Assert.Same(Undefined.value, value.ReadDynamicSlot("missing").unwrap());

            value.WriteDynamicElement("name", "Grace");
            Assert.Equal("Grace", value.ReadDynamicSlot("name").unwrap());
        }

        [Fact]
        public void ArrayCarrier_PreservesHolesAndLengthMutation()
        {
            var value = TsValue.from(new TsArray());

            value.WriteDynamicElement(2, "third");

            Assert.Equal(3, value.ReadDynamicSlot("length").unwrap());
            Assert.Same(Undefined.value, value.ReadDynamicElement(0).unwrap());
            Assert.Equal("third", value.ReadDynamicElement(2).unwrap());

            value.WriteDynamicSlot("length", 1);
            Assert.Equal(1, value.ReadDynamicSlot("length").unwrap());
            Assert.Same(Undefined.value, value.ReadDynamicElement(2).unwrap());
        }

        [Fact]
        public void FunctionCarrier_PreservesReceiverAndArguments()
        {
            var receiver = TsValue.CreateDynamicObject("prefix", "hello ");
            receiver.WriteDynamicSlot("greet", new TsFunction((selectedReceiver, arguments) =>
                TsValue.from(
                    (string)selectedReceiver.ReadDynamicSlot("prefix").unwrap()! +
                    (string)arguments[0].unwrap()!)));

            var result = receiver.InvokeDynamicSlot(
                "greet",
                optionalReceiver: false,
                optionalCall: false,
                () => new object?[] { "world" });

            Assert.Equal("hello world", result.unwrap());
        }

        [Fact]
        public void UnionCarrier_PreservesExactActiveArm()
        {
            var value = TsValue.from(Union<int, string>.From2("ready"));

            var union = Assert.IsType<TsUnion>(value.unwrap());
            Assert.Equal(2, union.ArmIndex);
            Assert.Equal(2, union.ArmCount);
            Assert.Equal("ready", union.asArm(2).unwrap());
            Assert.Equal("string", TsValue.ApplyDynamicTypeof(value));
        }

        [Fact]
        public void UnsupportedOpenObject_FailsBeforeDynamicUse()
        {
            Assert.Throws<NotSupportedException>(() => TsValue.from(new OpenObject()));
            Assert.Throws<NotSupportedException>(() => TsValue.ApplyDynamicTypeof(new OpenObject()));
        }

        [Fact]
        public void ThrownValueCarrier_PreservesNativeAndNonNativeValues()
        {
            var native = new InvalidOperationException("native");
            Assert.Same(native, TsThrownValueException.from(TsThrownValueException.toValue(native)));

            var nonNative = TsThrownValueException.from("source value");
            var wrapper = Assert.IsType<TsThrownValueException>(nonNative);
            Assert.Equal("source value", wrapper.value.unwrap());
        }

        [Fact]
        public void DynamicOperators_PreserveNullishAndShortCircuitSemantics()
        {
            var evaluations = 0;

            Assert.Equal(
                "fallback",
                TsValue.ApplyDynamicLogical(Undefined.value, "??", () =>
                {
                    evaluations++;
                    return "fallback";
                }).unwrap());
            Assert.Equal(
                "left",
                TsValue.ApplyDynamicLogical("left", "||", () =>
                {
                    evaluations++;
                    return "unused";
                }).unwrap());
            Assert.Equal(1, evaluations);
            Assert.True(TsValue.ApplyDynamicBinaryBoolean(null, "==", Undefined.value));
            Assert.False(TsValue.ApplyDynamicBinaryBoolean(null, "===", Undefined.value));
        }

        [Fact]
        public void ErrorCarrier_ExposesClosedSourceProperties()
        {
            var error = TsValue.from(new TypeError("invalid"));

            Assert.Equal("TypeError", error.ReadDynamicSlot("name").unwrap());
            Assert.Equal("invalid", error.ReadDynamicSlot("message").unwrap());
        }

        [Fact]
        public void DynamicObjectInterface_ExtendsCoreWithoutAJsDependency()
        {
            var target = new TestDynamicObject();
            var value = TsValue.from(target);

            value.WriteDynamicSlot("answer", 42);

            Assert.Equal(42, value.ReadDynamicSlot("answer").unwrap());
            Assert.Same(Undefined.value, value.ReadDynamicSlot("missing").unwrap());
        }

        [Fact]
        public void DynamicArrayInterface_ExtendsCoreWithoutAJsDependency()
        {
            var target = new TestDynamicArray();
            var value = TsValue.from(target);

            value.WriteDynamicElement(1, "second");

            Assert.Equal(2, value.ReadDynamicSlot("length").unwrap());
            Assert.Same(Undefined.value, value.ReadDynamicElement(0).unwrap());
            Assert.Equal("second", value.ReadDynamicElement(1).unwrap());
        }

        private sealed class OpenObject
        {
        }

        private sealed class TestDynamicObject : IDynamicObject
        {
            private readonly Dictionary<string, object?> _values = new();

            public bool TryReadDynamicSlot(string key, out object? value)
            {
                return _values.TryGetValue(key, out value);
            }

            public void WriteDynamicSlot(string key, object? value)
            {
                _values[key] = value;
            }
        }

        private sealed class TestDynamicArray : IDynamicArray
        {
            private readonly List<object?> _values = new();
            private readonly HashSet<int> _present = new();

            public int Length => _values.Count;

            public bool HasIndex(int index)
            {
                return _present.Contains(index);
            }

            public bool TryGetAt(int index, out object? value)
            {
                if (HasIndex(index))
                {
                    value = _values[index];
                    return true;
                }
                value = null;
                return false;
            }

            public bool TrySetAt(int index, object? value)
            {
                if (index < 0)
                {
                    return false;
                }
                SetLength(Math.Max(Length, index + 1));
                _values[index] = value;
                _present.Add(index);
                return true;
            }

            public int SetLength(int newLength)
            {
                if (newLength < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(newLength));
                }
                if (newLength < Length)
                {
                    _values.RemoveRange(newLength, Length - newLength);
                    _present.RemoveWhere(index => index >= newLength);
                }
                while (Length < newLength)
                {
                    _values.Add(null);
                }
                return Length;
            }

            public bool DeleteAt(int index)
            {
                return _present.Remove(index);
            }
        }
    }
}
