using System.Text.Json.Nodes;

namespace JsonCraft.Experimental.JsonPath.SupportJsonNode;

internal class ScanMultipleFilter : PathFilter
{
    private List<string> _names;

    public ScanMultipleFilter(List<string> names)
    {
        _names = names;
    }

    public override IEnumerable<JsonNode?> ExecuteFilter(JsonNode root, IEnumerable<JsonNode> current, JsonSelectSettings? settings)
    {
        foreach (JsonNode c in current)
        {
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
                if (_names.Contains(property.Key))
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