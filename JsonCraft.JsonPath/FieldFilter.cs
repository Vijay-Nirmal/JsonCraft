using System.Globalization;
using System.Text.Json;

namespace JsonCraft.JsonPath
{
    internal class FieldFilter : PathFilter
    {
        internal ReadOnlyMemory<char>? Name;

        public FieldFilter(ReadOnlyMemory<char>? name)
        {
            Name = name;
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, JsonElement current, JsonSelectSettings? settings)
        {
            if (current.ValueKind == JsonValueKind.Object)
            {
                if (Name.HasValue)
                {
                    if (current.TryGetProperty(Name.Value.Span, out var v))
                    {
                        return [v];
                    }
                    else if (settings?.ErrorWhenNoMatch ?? false)
                    {
                        throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Property '{0}' does not exist on JObject.", Name));
                    }
                }
                else
                {
                    return current.EnumerateObject().Select(x => x.Value);
                }
            }
            else
            {
                if (settings?.ErrorWhenNoMatch ?? false)
                {
                    throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Property '{0}' not valid on {1}.", Name.HasValue ? Name.Value.Span.ToString() : "*", current.ValueKind));
                }
            }

            return Enumerable.Empty<JsonElement>();
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            if (current.TryGetNonEnumeratedCount(out int count) && count == 1)
            {
                return ExecuteFilter(root, current.First(), settings);
            }
            else
            {
                return ExecuteFilterMultiple(current, settings?.ErrorWhenNoMatch ?? false);
            }
        }

        private IEnumerable<JsonElement> ExecuteFilterMultiple(IEnumerable<JsonElement> current, bool errorWhenNoMatch)
        {
            foreach (var item in current)
            {
                // Note: Not calling ExecuteFilter with yield return because that approach is slower and uses more memory. So we have duplicated code here.
                if (item.ValueKind == JsonValueKind.Object)
                {
                    if (Name.HasValue)
                    {
                        if (item.TryGetProperty(Name.Value.Span, out var v))
                        {
                            yield return v;
                        }
                        else if (errorWhenNoMatch)
                        {
                            throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Property '{0}' does not exist on JObject.", Name.Value.Span.ToString()));
                        }
                    }
                    else
                    {
                        foreach (var p in item.EnumerateObject())
                        {
                            yield return p.Value;
                        }
                    }
                }
                else
                {
                    if (errorWhenNoMatch)
                    {
                        throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Property '{0}' not valid on {1}.", Name.HasValue ? Name.Value.Span.ToString() : "*", item.ValueKind));
                    }
                }
            }
        }
    }
}