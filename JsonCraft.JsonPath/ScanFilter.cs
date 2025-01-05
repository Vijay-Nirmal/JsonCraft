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

            // Using while loop is faster than foreach loop here
            if (current.ValueKind == JsonValueKind.Array)
            {
                var enumerator = current.EnumerateArray();
                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;
                    var resultEnumerator = ExecuteFilterSingle(item).GetEnumerator();
                    while (resultEnumerator.MoveNext())
                    {
                        yield return resultEnumerator.Current;
                    }
                }
            }
            else if (current.ValueKind == JsonValueKind.Object)
            {
                var enumerator = current.EnumerateObject();
                while (enumerator.MoveNext())
                {
                    var property = enumerator.Current;
                    var resultEnumerator = ExecuteFilterSingle(property.Value, property).GetEnumerator();
                    while (resultEnumerator.MoveNext())
                    {
                        yield return resultEnumerator.Current;
                    }
                }
            }
        }
    }
}