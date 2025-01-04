using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonCraft.Experimental.JsonPath.SupportJsonNode
{
    internal class ArrayIndexFilter : PathFilter
    {
        public int? Index { get; set; }

        public override IEnumerable<JsonNode?> ExecuteFilter(JsonNode root, IEnumerable<JsonNode> current, JsonSelectSettings? settings)
        {
            foreach (JsonNode t in current)
            {
                if (Index != null)
                {
                    if (TryGetTokenIndex(t, settings, Index.GetValueOrDefault(), out var v))
                    {
                        yield return v;
                    }
                }
                else
                {
                    if (t is JsonArray arr)
                    {
                        foreach (var v in arr)
                        {
                            yield return v;
                        }
                    }
                    else
                    {
                        if (settings?.ErrorWhenNoMatch ?? false)
                        {
                            throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Index * not valid on {0}.", t.GetType().Name));
                        }
                    }
                }
            }
        }
    }
}