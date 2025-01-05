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

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, JsonElement current, JsonSelectSettings? settings)
        {
            return ExecuteFilterSingle(current);
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            return current.SelectMany(x => ExecuteFilterSingle(x));
        }

        // TODO: In .Net 10, Replace the second parameter with isObject bool and use GetRawUtf8PropertyName to compare the names
        private IEnumerable<JsonElement> ExecuteFilterSingle(JsonElement current, JsonProperty propertyName = default)
        {
            if (Name is null || propertyName.NameEquals(Name))
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
                    foreach (var result in ExecuteFilterSingle(property.Value, property))
                    {
                        yield return result;
                    }
                }
            }
        }
    }
}