#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonCraft.Experimental.JsonPath.SupportJsonNode
{
    public static class JsonExtensions
    {
        public static JsonNode? SelectToken(this JsonNode jsonNode, string path, JsonSelectSettings? settings = null)
        {
            JPath p = new JPath(path);

            JsonNode? token = null;
            foreach (var t in p.Evaluate(jsonNode, jsonNode, settings))
            {
                if (token != null)
                {
                    throw new JsonException("Path returned multiple tokens.");
                }

                token = t;
            }

            return token;
        }

        public static IEnumerable<JsonNode?> SelectTokens(this JsonNode jsonNode, string path, JsonSelectSettings? settings = null)
        {
            JPath p = new JPath(path);
            return p.Evaluate(jsonNode, jsonNode, settings);
        }

        public static JsonElement SelectToken(this JsonElement document, string path, JsonSelectSettings? settings = null)
        {
            var documentNode = document.AsNode() ?? throw new ArgumentException("Argument can't be converted into JsonNode", nameof(document));
            return SelectToken(documentNode, path, settings).ToJsonDocument().RootElement;
        }

        public static IEnumerable<JsonElement> SelectTokens(this JsonElement document, string path, JsonSelectSettings? settings = null)
        {
            var documentNode = document.AsNode() ?? throw new ArgumentException("Argument can't be converted into JsonNode", nameof(document));
            return SelectTokens(documentNode, path, settings).Select(x => x.ToJsonDocument().RootElement);
        }

        public static JsonDocument SelectToken(this JsonDocument document, string path, JsonSelectSettings? settings = null)
        {
            var documentNode = document.RootElement.AsNode() ?? throw new ArgumentException("Argument can't be converted into JsonNode", nameof(document));
            return document.RootElement.SelectToken(path, settings).ToJsonDocument();
        }

        public static IEnumerable<JsonDocument> SelectTokens(this JsonDocument document, string path, JsonSelectSettings? settings = null)
        {
            return document.RootElement.SelectTokens(path, settings).Select(x => x.ToJsonDocument());
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