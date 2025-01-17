using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonCraft.Experimental.JsonPath.SupportJsonNode
{
    public static class JsonExtensions
    {
        public static bool TrySelectElement(this JsonNode jsonNode, string path, JsonSelectSettings? settings, out JsonNode? resultJsonNode)
        {
            JPath p = new JPath(path);

            resultJsonNode = null;
            var count = 0;
            foreach (var t in p.Evaluate(jsonNode, jsonNode, settings))
            {
                count++;

                if (count != 1)
                {
                    throw new JsonException("Path returned multiple elements.");
                }

                resultJsonNode = t;
            }

            return count == 0 ? false : true;
        }

        public static bool TrySelectElement(this JsonNode jsonNode, string path, out JsonNode? resultJsonNode)
        {
            return jsonNode.TrySelectElement(path, null, out resultJsonNode);
        }

        public static IEnumerable<JsonNode?> SelectElements(this JsonNode jsonNode, string path, JsonSelectSettings? settings = null)
        {
            JPath p = new JPath(path);
            return p.Evaluate(jsonNode, jsonNode, settings);
        }

        public static JsonElement? SelectElement(this JsonElement document, string path, JsonSelectSettings? settings = null)
        {
            var documentNode = document.AsNode() ?? throw new ArgumentException("Argument can't be converted into JsonNode", nameof(document));
            return documentNode.TrySelectElement(path, settings, out var jsonNode) ? jsonNode.ToJsonDocument().RootElement : null;
        }

        public static IEnumerable<JsonElement> SelectElements(this JsonElement document, string path, JsonSelectSettings? settings = null)
        {
            var documentNode = document.AsNode() ?? throw new ArgumentException("Argument can't be converted into JsonNode", nameof(document));
            return SelectElements(documentNode, path, settings).Select(x => x.ToJsonDocument().RootElement);
        }

        [OverloadResolutionPriority(999)]
        public static JsonElement? SelectElement(this JsonDocument document, string path, JsonSelectSettings? settings = null)
        {
            var documentNode = document.RootElement.AsNode() ?? throw new ArgumentException("Argument can't be converted into JsonNode", nameof(document));
            return documentNode.TrySelectElement(path, settings, out var jsonNode) ? jsonNode.ToJsonDocument().RootElement : null;
        }

        public static IEnumerable<JsonElement> SelectElements(this JsonDocument document, string path, JsonSelectSettings? settings = null)
        {
            return document.RootElement.SelectElements(path, settings);
        }

        public static JsonElement? SelectElement(this JsonDocument document, string path, bool errorWhenNoMatch = false)
        {
            return document.RootElement.SelectElement(path, new JsonSelectSettings { ErrorWhenNoMatch = errorWhenNoMatch });
        }

        public static JsonNode? AsNode(this JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Array => JsonArray.Create(element),
                JsonValueKind.Object => JsonObject.Create(element),
                _ => JsonValue.Create(element)
            };
        }

        public static JsonDocument ToJsonDocument<T>(this T value, JsonSerializerOptions? options = null)
        {
            if (value is JsonDocument doc) return doc;

            return JsonDocument.Parse(JsonSerializer.Serialize(value, options));
        }
    }
}