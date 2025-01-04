using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonCraft.Experimental.JsonPath.SupportJsonNode
{
    internal class FieldMultipleFilter : PathFilter
    {
        internal List<string> Names;

        public FieldMultipleFilter(List<string> names)
        {
            Names = names;
        }

        public override IEnumerable<JsonNode?> ExecuteFilter(JsonNode root, IEnumerable<JsonNode> current, JsonSelectSettings? settings)
        {
            foreach (JsonNode t in current)
            {
                if (t is JsonObject o)
                {
                    foreach (string name in Names)
                    {
                        var v = o[name];

                        if (v != null)
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