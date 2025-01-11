using System.Globalization;
using System.Text.Json;

namespace JsonCraft.JsonPath
{
    /// <summary>
    /// Represents an abstract base class for path filters used in JSON selection.
    /// </summary>
    public abstract class PathFilter
    {
        /// <summary>
        /// Executes the filter on the specified JSON element.
        /// </summary>
        /// <param name="root">The root JSON element.</param>
        /// <param name="current">The current JSON element.</param>
        /// <param name="settings">The settings used for JSON selection.</param>
        /// <returns>An enumerable collection of JSON elements that match the filter.</returns>
        public abstract IEnumerable<JsonElement> ExecuteFilter(JsonElement root, JsonElement current, JsonSelectSettings? settings);

        /// <summary>
        /// Executes the filter on the specified collection of JSON elements.
        /// </summary>
        /// <param name="root">The root JSON element.</param>
        /// <param name="current">The collection of current JSON elements.</param>
        /// <param name="settings">The settings used for JSON selection.</param>
        /// <returns>An enumerable collection of JSON elements that match the filter.</returns>
        public abstract IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings);

        /// <summary>
        /// Gets the JSON element at the specified index in an array.
        /// </summary>
        /// <param name="t">The JSON element.</param>
        /// <param name="index">The index of the element to retrieve.</param>
        /// <param name="errorWhenNoMatch">A flag indicating whether an error should be thrown if the index is out of bounds.</param>
        /// <returns>The JSON element at the specified index, or null if the index is out of bounds and <paramref name="errorWhenNoMatch"/> is false.</returns>
        /// <exception cref="JsonException">Thrown if the index is out of bounds and <paramref name="errorWhenNoMatch"/> is true.</exception>
        protected static JsonElement? GetTokenIndex(JsonElement t, int index, bool errorWhenNoMatch = false)
        {
            if (t.ValueKind == JsonValueKind.Array)
            {
                if (t.GetArrayLength() <= index)
                {
                    if (errorWhenNoMatch)
                    {
                        throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Index {0} outside the bounds of JArray.", index));
                    }

                    return null;
                }

                return t[index];
            }
            else
            {
                if (errorWhenNoMatch)
                {
                    throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Index {0} not valid on {1}.", index, t.GetType().Name));
                }

                return null;
            }
        }
    }
}