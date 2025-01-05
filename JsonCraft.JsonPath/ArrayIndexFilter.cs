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
                var v = GetTokenIndex(current, settings, Index.GetValueOrDefault());

                if (v != null)
                {
                    yield return v.Value;
                }
            }
            else
            {
                if (current.ValueKind == JsonValueKind.Array)
                {
                    foreach (var v in current.EnumerateArray())
                    {
                        yield return v;
                    }
                }
                else
                {
                    if (settings?.ErrorWhenNoMatch ?? false)
                    {
                        throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Index * not valid on {0}.", current.ValueKind));
                    }
                }
            }
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            return current.SelectMany(x => ExecuteFilter(root, x, settings));
        }
    }
}