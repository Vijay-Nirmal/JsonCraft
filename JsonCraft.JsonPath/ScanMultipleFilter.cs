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

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, JsonElement current, JsonSelectSettings? settings)
        {
            foreach (var result in ExecuteFilterSingle(current))
            {
                yield return result;
            }
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            return current.SelectMany(x => ExecuteFilterSingle(x));
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
                    // TODO: Using NameEquals instead of Contains with foreach load reduces allocation but increases CPU time, so don't doing that. In Net 10, Try Use GetRawUtf8PropertyName and use AlternateLookup with HashSet(if it doesn't increase the memory usage)
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