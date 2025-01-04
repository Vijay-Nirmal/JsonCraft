using System.Globalization;
using System.Text.Json;

namespace JsonCraft.JsonPath
{
    internal abstract class PathFilter
    {
        public abstract IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings);

        protected static JsonElement? GetTokenIndex(JsonElement t, JsonSelectSettings? settings, int index)
        {
            if (t.ValueKind == JsonValueKind.Array)
            {
                if (t.GetArrayLength() <= index)
                {
                    if (settings?.ErrorWhenNoMatch ?? false)
                    {
                        throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Index {0} outside the bounds of JArray.", index));
                    }

                    return null;
                }

                return t[index];
            }
            else
            {
                if (settings?.ErrorWhenNoMatch ?? false)
                {
                    throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Index {0} not valid on {1}.", index, t.GetType().Name));
                }

                return null;
            }
        }
    }
}