using System.Globalization;
using System.Text.Json;

namespace JsonCraft.JsonPath
{
    internal class ArrayIndexFilter : PathFilter
    {
        public int? Index { get; set; }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            foreach (var t in current)
            {
                if (Index != null)
                {
                    var v = GetTokenIndex(t, settings, Index.GetValueOrDefault());

                    if (v != null)
                    {
                        yield return v.Value;
                    }
                }
                else
                {
                    if (t.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var v in t.EnumerateArray())
                        {
                            yield return v;
                        }
                    }
                    else
                    {
                        if (settings?.ErrorWhenNoMatch ?? false)
                        {
                            throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Index * not valid on {0}.", t.GetType().Name));
                        }
                    }
                }
            }
        }
    }
}