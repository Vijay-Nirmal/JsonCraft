using System.Runtime.CompilerServices;
using System.Text.Json;

namespace JsonCraft.JsonPath
{
    public static class JsonExtensions
    {
        [OverloadResolutionPriority(999)]
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

        public static JsonElement? SelectToken(this JsonElement jsonElement, string path, bool errorWhenNoMatch = false)
        {
            return jsonElement.SelectToken(path, new JsonSelectSettings { ErrorWhenNoMatch = errorWhenNoMatch });
        }

        public static IEnumerable<JsonElement> SelectTokens(this JsonElement jsonElement, string path, JsonSelectSettings? settings = null)
        {
            JPath p = new JPath(path);
            return p.Evaluate(jsonElement, jsonElement, settings);
        }

        [OverloadResolutionPriority(999)]
        public static JsonElement? SelectToken(this JsonDocument document, string path, JsonSelectSettings? settings = null)
        {
            return document.RootElement.SelectToken(path, settings);
        }

        public static JsonElement? SelectToken(this JsonDocument document, string path, bool errorWhenNoMatch = false)
        {
            // TODO: Have a static instance of JsonSelectSettings to avoid creating a new instance every time.
            return document.RootElement.SelectToken(path, new JsonSelectSettings { ErrorWhenNoMatch = errorWhenNoMatch });
        }

        public static IEnumerable<JsonElement> SelectTokens(this JsonDocument document, string path, JsonSelectSettings? settings = null)
        {
            return document.RootElement.SelectTokens(path, settings);
        }
    }
}