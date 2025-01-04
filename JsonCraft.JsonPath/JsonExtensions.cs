using System.Text.Json;

namespace JsonCraft.JsonPath
{
    public static class JsonExtensions
    {
        public static JsonElement? SelectToken(this JsonElement jsonElement, string path, JsonSelectSettings? settings = null)
        {
            JPath p = new JPath(path);

            JsonElement? token = default;
            foreach (var t in p.Evaluate(jsonElement, jsonElement, settings))
            {
                if (token != null)
                {
                    throw new JsonException("Path returned multiple tokens.");
                }

                token = t;
            }

            return token;
        }

        public static IEnumerable<JsonElement> SelectTokens(this JsonElement jsonElement, string path, JsonSelectSettings? settings = null)
        {
            JPath p = new JPath(path);
            return p.Evaluate(jsonElement, jsonElement, settings);
        }

        public static JsonElement? SelectToken(this JsonDocument document, string path, JsonSelectSettings? settings = null)
        {
            return document.RootElement.SelectToken(path, settings);
        }

        public static IEnumerable<JsonElement> SelectTokens(this JsonDocument document, string path, JsonSelectSettings? settings = null)
        {
            return document.RootElement.SelectTokens(path, settings);
        }
    }
}