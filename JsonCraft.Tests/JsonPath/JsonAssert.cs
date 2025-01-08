using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonCraft.Tests.JsonPath
{
    public static class JsonAssert
    {
        public static void AreEqual(JsonElement? left, JsonElement? right)
        {
            Assert.AreEqual(left, right, JsonElementEqualityComparer.Instance);
        }

        public static void AreEqual(JsonDocument left, JsonElement? right)
        {
            Assert.AreEqual(left.RootElement, right, JsonElementEqualityComparer.Instance);
        }
    }

    public static class JsonTestExtensions
    {
        public static bool DeepEquals(this JsonElement left, JsonElement? right)
        {
            if (right is null)
            {
                return false;
            }
            return JsonNode.DeepEquals(JsonNode.Parse(JsonSerializer.Serialize(left)), JsonNode.Parse(JsonSerializer.Serialize(right.Value)));
        }
    }

    public class JsonElementEqualityComparer : IEqualityComparer<JsonElement?>
    {
        public static JsonElementEqualityComparer Instance { get; } = new JsonElementEqualityComparer();

        public bool Equals(JsonElement? x, JsonElement? y)
        {
            if (x is null && y is null)
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.Value.DeepEquals(y);
        }

        public int GetHashCode([DisallowNull] JsonElement? obj)
        {
            return obj is null ? 0 : obj.GetHashCode();
        }
    }
}