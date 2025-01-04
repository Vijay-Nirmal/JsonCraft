using System.Text.Json;

namespace JsonCraft.JsonPath
{
    internal class ScanMultipleFilter : PathFilter
    {
        private List<string> _names;

        public ScanMultipleFilter(List<string> names)
        {
            _names = names;
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            foreach (var c in current)
            {
                foreach (var result in ExecuteFilterSingle(c))
                {
                    yield return result;
                }
            }
        }

        private IEnumerable<JsonElement> ExecuteFilterSingle(JsonElement current)
        {
            if (current.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in current.EnumerateArray())
                {
                    foreach (var result in ExecuteFilterSingle(item))
                    {
                        yield return result;
                    }
                }
            }
            else if (current.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in current.EnumerateObject())
                {
                    if (_names.Contains(property.Name))
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