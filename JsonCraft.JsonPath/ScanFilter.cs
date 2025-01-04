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
            foreach (var c in current)
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

        private IEnumerable<JsonElement> ExecuteFilterSingle(JsonElement current)
        {
            if (current.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in current.EnumerateArray())
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
            else if (current.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in current.EnumerateObject())
                {
                    if (Name == property.Name || Name == null)
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