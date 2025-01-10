using System.Runtime.CompilerServices;
using System.Text.Json;

namespace JsonCraft.JsonPath
{
    /// <summary>
    /// Provides extension methods for selecting JSON tokens.
    /// </summary>
    public static class JsonExtensions
    {
        /// <summary>
        /// Selects a single JSON token that matches the specified path.
        /// </summary>
        /// <param name="jsonElement">The JSON element to search within.</param>
        /// <param name="path">The JSON path to search for.</param>
        /// <param name="settings">Optional settings for selecting the JSON token.</param>
        /// <returns>The selected JSON token, or <c>null</c> if no token is found.</returns>
        /// <exception cref="JsonException">Thrown if the path returns multiple tokens.</exception>
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

        /// <summary>
        /// Selects a single JSON token that matches the specified path.
        /// </summary>
        /// <param name="jsonElement">The JSON element to search within.</param>
        /// <param name="path">The JSON path to search for.</param>
        /// <param name="errorWhenNoMatch">Whether to throw an error if no match is found.</param>
        /// <returns>The selected JSON token, or <c>null</c> if no token is found.</returns>
        public static JsonElement? SelectToken(this JsonElement jsonElement, string path, bool errorWhenNoMatch = false)
        {
            return jsonElement.SelectToken(path, new JsonSelectSettings { ErrorWhenNoMatch = errorWhenNoMatch });
        }

        /// <summary>
        /// Selects multiple JSON tokens that match the specified path.
        /// </summary>
        /// <param name="jsonElement">The JSON element to search within.</param>
        /// <param name="path">The JSON path to search for.</param>
        /// <param name="settings">Optional settings for selecting the JSON tokens.</param>
        /// <returns>An enumerable collection of selected JSON tokens.</returns>
        public static IEnumerable<JsonElement> SelectTokens(this JsonElement jsonElement, string path, JsonSelectSettings? settings = null)
        {
            JPath p = new JPath(path);
            return p.Evaluate(jsonElement, jsonElement, settings);
        }

        /// <summary>
        /// Selects a single JSON token that matches the specified path from a JSON document.
        /// </summary>
        /// <param name="document">The JSON document to search within.</param>
        /// <param name="path">The JSON path to search for.</param>
        /// <param name="settings">Optional settings for selecting the JSON token.</param>
        /// <returns>The selected JSON token, or <c>null</c> if no token is found.</returns>
        [OverloadResolutionPriority(999)]
        public static JsonElement? SelectToken(this JsonDocument document, string path, JsonSelectSettings? settings = null)
        {
            return document.RootElement.SelectToken(path, settings);
        }

        /// <summary>
        /// Selects a single JSON token that matches the specified path from a JSON document.
        /// </summary>
        /// <param name="document">The JSON document to search within.</param>
        /// <param name="path">The JSON path to search for.</param>
        /// <param name="errorWhenNoMatch">Whether to throw an error if no match is found.</param>
        /// <returns>The selected JSON token, or <c>null</c> if no token is found.</returns>
        public static JsonElement? SelectToken(this JsonDocument document, string path, bool errorWhenNoMatch = false)
        {
            return document.RootElement.SelectToken(path, new JsonSelectSettings { ErrorWhenNoMatch = errorWhenNoMatch });
        }

        /// <summary>
        /// Selects multiple JSON tokens that match the specified path from a JSON document.
        /// </summary>
        /// <param name="document">The JSON document to search within.</param>
        /// <param name="path">The JSON path to search for.</param>
        /// <param name="settings">Optional settings for selecting the JSON tokens.</param>
        /// <returns>An enumerable collection of selected JSON tokens.</returns>
        public static IEnumerable<JsonElement> SelectTokens(this JsonDocument document, string path, JsonSelectSettings? settings = null)
        {
            return document.RootElement.SelectTokens(path, settings);
        }
    }
}