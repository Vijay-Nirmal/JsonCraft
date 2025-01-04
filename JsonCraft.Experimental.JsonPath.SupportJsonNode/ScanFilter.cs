using System.Text.Json.Nodes;

namespace JsonCraft.Experimental.JsonPath.SupportJsonNode
{
    internal class ScanFilter : PathFilter
    {
        internal string? Name;

        public ScanFilter(string? name)
        {
            Name = name;
        }

        public override IEnumerable<JsonNode?> ExecuteFilter(JsonNode root, IEnumerable<JsonNode> current, JsonSelectSettings? settings)
        {
            foreach (JsonNode c in current)
            {
                if (Name == null)
                {
                    yield return c;
                }

                foreach (var result in ExecuteFilterSingle(c))
                {
                    yield return result;
                }
            }
        }

        private IEnumerable<JsonNode?> ExecuteFilterSingle(JsonNode? current)
        {
            if (current is JsonArray currArr)
            {
                foreach (var item in currArr)
                {
                    if (Name == null)
                    {
                        yield return item;
                    }

                    foreach (var result in ExecuteFilterSingle(item))
                    {
                        yield return result;
                    }
                }
            }
            else if (current is JsonObject currObj)
            {
                foreach (var property in currObj)
                {
                    if (Name == property.Key || Name == null)
                    {
                        yield return property.Value;
                    }

                    foreach (var result in ExecuteFilterSingle(property.Value))
                    {
                        yield return result;
                    }
                }
            }
        }
    }
}