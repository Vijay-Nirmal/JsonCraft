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

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, JsonElement current, JsonSelectSettings? settings)
        {
            if (current.ValueKind == JsonValueKind.Object)
            {
                if (Name != null)
                {
                    if (current.TryGetProperty(Name, out var v))
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
                    foreach (var p in current.EnumerateObject())
                    {
                        yield return p.Value;
                    }
                }
            }
            else
            {
                if (settings?.ErrorWhenNoMatch ?? false)
                {
                    throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Property '{0}' not valid on {1}.", Name ?? "*", current.ValueKind));
                }
            }
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            return current.SelectMany(x => ExecuteFilter(root, x, settings));
        }
    }
}