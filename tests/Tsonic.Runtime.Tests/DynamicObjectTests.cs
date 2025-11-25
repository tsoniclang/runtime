using System.Collections.Generic;
using Tsonic.Runtime;
using Xunit;

namespace Tsonic.Runtime.Tests
{
    public class DynamicObjectTests
    {
        [Fact]
        public void SetProperty_GetProperty_RoundTrips()
        {
            var obj = new DynamicObject();
            obj.SetProperty("name", "test");

            Assert.Equal("test", obj.GetProperty<string>("name"));
        }

        [Fact]
        public void GetProperty_NonExistent_ReturnsDefault()
        {
            var obj = new DynamicObject();

            Assert.Null(obj.GetProperty<string>("missing"));
            Assert.Equal(0, obj.GetProperty<int>("missing"));
        }

        [Fact]
        public void HasProperty_Exists_ReturnsTrue()
        {
            var obj = new DynamicObject();
            obj.SetProperty("name", "test");

            Assert.True(obj.HasProperty("name"));
        }

        [Fact]
        public void HasProperty_NotExists_ReturnsFalse()
        {
            var obj = new DynamicObject();

            Assert.False(obj.HasProperty("missing"));
        }

        [Fact]
        public void GetKeys_ReturnsAllKeys()
        {
            var obj = new DynamicObject();
            obj.SetProperty("a", 1);
            obj.SetProperty("b", 2);
            obj.SetProperty("c", 3);

            var keys = obj.GetKeys();

            Assert.Equal(3, keys.Length);
            Assert.Contains("a", keys);
            Assert.Contains("b", keys);
            Assert.Contains("c", keys);
        }

        [Fact]
        public void GetValues_ReturnsAllValues()
        {
            var obj = new DynamicObject();
            obj.SetProperty("a", 1);
            obj.SetProperty("b", 2);

            var values = obj.GetValues();

            Assert.Equal(2, values.Length);
            Assert.Contains(1, values);
            Assert.Contains(2, values);
        }

        [Fact]
        public void Indexer_Get_ReturnsValue()
        {
            var obj = new DynamicObject();
            obj.SetProperty("name", "test");

            Assert.Equal("test", obj["name"]);
        }

        [Fact]
        public void Indexer_Get_NonExistent_ReturnsNull()
        {
            var obj = new DynamicObject();

            Assert.Null(obj["missing"]);
        }

        [Fact]
        public void Indexer_Set_SetsValue()
        {
            var obj = new DynamicObject();
            obj["name"] = "test";

            Assert.Equal("test", obj.GetProperty<string>("name"));
        }

        [Fact]
        public void FromDictionary_CreatesObjectWithProperties()
        {
            var dict = new Dictionary<string, object?>
            {
                ["name"] = "test",
                ["count"] = 42
            };

            var obj = DynamicObject.FromDictionary(dict);

            Assert.Equal("test", obj.GetProperty<string>("name"));
            Assert.Equal(42, obj.GetProperty<int>("count"));
        }

        [Fact]
        public void ToDictionary_ReturnsDictionaryWithProperties()
        {
            var obj = new DynamicObject();
            obj.SetProperty("name", "test");
            obj.SetProperty("count", 42);

            var dict = obj.ToDictionary();

            Assert.Equal(2, dict.Count);
            Assert.Equal("test", dict["name"]);
            Assert.Equal(42, dict["count"]);
        }

        [Fact]
        public void GetProperty_TypeConversion_ConvertsValue()
        {
            var obj = new DynamicObject();
            obj.SetProperty("value", 42);

            // Int can be converted to double
            Assert.Equal(42.0, obj.GetProperty<double>("value"));
        }
    }
}
