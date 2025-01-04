using System.Globalization;
using System.Text.Json;

namespace JsonCraft.JsonPath
{
    internal class FieldFilter : PathFilter
    {
        internal string? Name;

        public FieldFilter(string? name)
        {
            Name = name;
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            foreach (var t in current)
            {
                if (t.ValueKind == JsonValueKind.Object)
                {
                    if (Name != null)
                    {
                        if (t.TryGetProperty(Name, out var v))
                        {
                            yield return v;
                        }
                        else if (settings?.ErrorWhenNoMatch ?? false)
                        {
                            throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Property '{0}' does not exist on JObject.", Name));
                        }
                    }
                    else
                    {
                        foreach (var p in t.EnumerateObject())
                        {
                            yield return p.Value;
                        }
                    }
                }
                else
                {
                    if (settings?.ErrorWhenNoMatch ?? false)
                    {
                        throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Property '{0}' not valid on {1}.", Name ?? "*", t.GetType().Name));
                    }
                }
            }
        }
    }
}