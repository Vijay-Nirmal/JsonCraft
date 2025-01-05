using System.Text.Json;

namespace JsonCraft.JsonPath
{
    internal class ScanFilter : PathFilter
    {
        internal string? Name;

        public ScanFilter(string? name)
        {
            Name = name;
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            /*foreach (var c in current)
            {
                foreach (var result in ExecuteFilterSingle(c))
                {
                    yield return result;
                }
            }*/
            return current.SelectMany(x => ExecuteFilterSingle(x));
            /*if (current.TryGetNonEnumeratedCount(out var count) && count == 1)
            {
                return ExecuteFilterSingle(current.First());
            }
            else
            {
                return current.SelectMany(x => ExecuteFilterSingle(x));
            }*/
        }

        private IEnumerable<JsonElement> ExecuteFilterSingle(JsonElement current, string? propertyName = null)
        {
            if (Name is null || Name == propertyName)
            {
                yield return current;
            }

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
                    foreach (var result in ExecuteFilterSingle(property.Value, property.Name))
                    {
                        yield return result;
                    }
                }
            }
        }
    }
}