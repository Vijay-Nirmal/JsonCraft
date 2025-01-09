using System.Globalization;
using System.Text.Json;

namespace JsonCraft.JsonPath
{
    public abstract class PathFilter
    {
        public abstract IEnumerable<JsonElement> ExecuteFilter(JsonElement root, JsonElement current, JsonSelectSettings? settings);
        public abstract IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings);

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