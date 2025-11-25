using System.Collections.Generic;
using System.Linq;
using Tsonic.Runtime;
using Xunit;

namespace Tsonic.Runtime.Tests
{
    public class StructuralTests
    {
        public class SourceClass
        {
            public string Name { get; set; } = "";
            public int Value { get; set; }
        }

        public class TargetClass
        {
            public string Name { get; set; } = "";
            public int Value { get; set; }
        }

        [Fact]
        public void Clone_CopiesProperties()
        {
            var source = new SourceClass { Name = "test", Value = 42 };

            var target = Structural.Clone<TargetClass>(source);

            Assert.NotNull(target);
            Assert.Equal("test", target!.Name);
            Assert.Equal(42, target.Value);
        }

        [Fact]
        public void Clone_NullSource_ReturnsDefault()
        {
            var target = Structural.Clone<TargetClass>(null);

            Assert.Null(target);
        }

        [Fact]
        public void CloneFromDictionary_CopiesProperties()
        {
            var source = new Dictionary<string, object?>
            {
                ["Name"] = "test",
                ["Value"] = 42
            };

            var target = Structural.CloneFromDictionary<TargetClass>(source);

            Assert.NotNull(target);
            Assert.Equal("test", target!.Name);
            Assert.Equal(42, target.Value);
        }

        [Fact]
        public void ToDictionary_ConvertsObject()
        {
            var source = new SourceClass { Name = "test", Value = 42 };

            var dict = Structural.ToDictionary(source);

            Assert.Equal(2, dict.Count);
            Assert.Equal("test", dict["Name"]);
            Assert.Equal(42, dict["Value"]);
        }

        [Fact]
        public void ToDictionary_NullSource_ReturnsEmptyDictionary()
        {
            var dict = Structural.ToDictionary(null);

            Assert.Empty(dict);
        }

        [Fact]
        public void CreateDictionaryAdapter_ProvidesTypedAccess()
        {
            var dict = new Dictionary<string, object?>
            {
                ["a"] = 1,
                ["b"] = 2,
                ["c"] = 3
            };

            var adapter = Structural.CreateDictionaryAdapter<int>(dict);

            Assert.Equal(1, adapter["a"]);
            Assert.Equal(2, adapter["b"]);
            Assert.Equal(3, adapter["c"]);
        }
    }

    public class DictionaryAdapterTests
    {
        [Fact]
        public void Indexer_Get_ReturnsTypedValue()
        {
            var dict = new Dictionary<string, object?> { ["value"] = 42 };
            var adapter = new DictionaryAdapter<int>(dict);

            Assert.Equal(42, adapter["value"]);
        }

        [Fact]
        public void Indexer_Get_NonExistent_ReturnsDefault()
        {
            var dict = new Dictionary<string, object?>();
            var adapter = new DictionaryAdapter<int>(dict);

            Assert.Equal(0, adapter["missing"]);
        }

        [Fact]
        public void Indexer_Set_SetsValue()
        {
            var dict = new Dictionary<string, object?>();
            var adapter = new DictionaryAdapter<int>(dict);

            adapter["value"] = 42;

            Assert.Equal(42, dict["value"]);
        }

        [Fact]
        public void Keys_ReturnsAllKeys()
        {
            var dict = new Dictionary<string, object?>
            {
                ["a"] = 1,
                ["b"] = 2
            };
            var adapter = new DictionaryAdapter<int>(dict);

            var keys = new List<string>(adapter.Keys);

            Assert.Equal(2, keys.Count);
            Assert.Contains("a", keys);
            Assert.Contains("b", keys);
        }

        [Fact]
        public void Values_ReturnsTypedValues()
        {
            var dict = new Dictionary<string, object?>
            {
                ["a"] = 1,
                ["b"] = 2
            };
            var adapter = new DictionaryAdapter<int>(dict);

            var values = adapter.Values.ToList();

            Assert.Equal(2, values.Count);
            Assert.Contains(values, v => v == 1);
            Assert.Contains(values, v => v == 2);
        }

        [Fact]
        public void ContainsKey_Exists_ReturnsTrue()
        {
            var dict = new Dictionary<string, object?> { ["key"] = "value" };
            var adapter = new DictionaryAdapter<string>(dict);

            Assert.True(adapter.ContainsKey("key"));
        }

        [Fact]
        public void ContainsKey_NotExists_ReturnsFalse()
        {
            var dict = new Dictionary<string, object?>();
            var adapter = new DictionaryAdapter<string>(dict);

            Assert.False(adapter.ContainsKey("missing"));
        }

        [Fact]
        public void GetDictionary_ReturnsCopy()
        {
            var dict = new Dictionary<string, object?> { ["key"] = "value" };
            var adapter = new DictionaryAdapter<string>(dict);

            var copy = adapter.GetDictionary();
            copy["newKey"] = "newValue";

            Assert.False(adapter.ContainsKey("newKey"));
        }
    }
}
