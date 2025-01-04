using System.Globalization;
using System.Text.Json;

namespace JsonCraft.JsonPath
{
    internal class FieldMultipleFilter : PathFilter
    {
        internal List<string> Names;

        public FieldMultipleFilter(List<string> names)
        {
            Names = names;
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            foreach (var t in current)
            {
                if (t.ValueKind == JsonValueKind.Object)
                {
                    foreach (string name in Names)
                    {
                        if (t.TryGetProperty(name, out var v))
                        {
                            yield return v;
                        }

                        if (settings?.ErrorWhenNoMatch ?? false)
                        {
                            throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Property '{0}' does not exist on JObject.", name));
                        }
                    }
                }
                else
                {
                    if (settings?.ErrorWhenNoMatch ?? false)
                    {
                        throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Properties {0} not valid on {1}.", string.Join(", ", Names.Select(n => "'" + n + "'")), t.GetType().Name));
                    }
                }
            }
        }
    }
}