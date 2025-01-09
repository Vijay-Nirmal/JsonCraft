using System.Globalization;
using System.Text.Json;

namespace JsonCraft.JsonPath
{
    internal class ArrayIndexFilter : PathFilter
    {
        public int? Index { get; set; }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, JsonElement current, JsonSelectSettings? settings)
        {
            if (Index != null)
            {
                var v = GetTokenIndex(current, Index.GetValueOrDefault(), settings?.ErrorWhenNoMatch ?? false);

                if (v != null)
                {
                    return [v.Value];
                }
            }
            else
            {
                if (current.ValueKind == JsonValueKind.Array)
                {
                    return current.EnumerateArray();
                }
                else
                {
                    if (settings?.ErrorWhenNoMatch ?? false)
                    {
                        throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Index * not valid on {0}.", current.ValueKind));
                    }
                }
            }

            return Enumerable.Empty<JsonElement>();
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            if (current.TryGetNonEnumeratedCount(out var count))
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
                if (Index != null)
                {
                    var v = GetTokenIndex(item, Index.GetValueOrDefault(), errorWhenNoMatch);

                    if (v != null)
                    {
                        yield return v.Value;
                    }
                }
                else
                {
                    if (item.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var v in item.EnumerateArray())
                        {
                            yield return v;
                        }
                    }
                    else
                    {
                        if (errorWhenNoMatch)
                        {
                            throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Index * not valid on {0}.", item.ValueKind));
                        }
                    }
                }
            }
        }
    }
}