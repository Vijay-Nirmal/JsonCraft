using System.Text.Json;

namespace JsonCraft.JsonPath
{
    internal class QueryScanFilter : PathFilter
    {
        internal QueryExpression Expression;

        public QueryScanFilter(QueryExpression expression)
        {
            Expression = expression;
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, JsonElement current, JsonSelectSettings? settings)
        {
            if (Expression.IsMatch(root, current, settings))
            {
                yield return current;
            }

            if (current.ValueKind == JsonValueKind.Object)
            {
                foreach (var d in current.EnumerateObject())
                {
                    if (Expression.IsMatch(root, d.Value, settings))
                    {
                        yield return d.Value;
                    }
                }
            }
            else if (current.ValueKind == JsonValueKind.Array)
            {
                foreach (var d in current.EnumerateArray())
                {
                    if (Expression.IsMatch(root, d, settings))
                    {
                        yield return d;
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