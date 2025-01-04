using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonCraft.Experimental.JsonPath.SupportJsonNode
{
    internal class FieldFilter : PathFilter
    {
        internal string? Name;

        public FieldFilter(string? name)
        {
            Name = name;
        }

        public override IEnumerable<JsonNode?> ExecuteFilter(JsonNode root, IEnumerable<JsonNode> current, JsonSelectSettings? settings)
        {
            foreach (JsonNode t in current)
            {
                if (t is JsonObject o)
                {
                    if (Name != null)
                    {
                        JsonNode? v = o[Name];

                        if (v != null)
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
                        foreach (KeyValuePair<string, JsonNode?> p in o)
                        {
                            yield return p.Value!;
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