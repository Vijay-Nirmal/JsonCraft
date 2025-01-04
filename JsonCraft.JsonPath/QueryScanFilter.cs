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

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            foreach (var t in current)
            {
                if (Expression.IsMatch(root, t, settings))
                {
                    yield return t;
                }

                if (t.ValueKind == JsonValueKind.Object)
                {
                    if (Expression.IsMatch(root, t, settings))
                    {
                        yield return t;
                    }

                    foreach (var d in t.EnumerateObject())
                    {
                        if (Expression.IsMatch(root, d.Value, settings))
                        {
                            yield return d.Value;
                        }
                    }
                }
                else if (t.ValueKind == JsonValueKind.Array)
                {
                    if (Expression.IsMatch(root, t, settings))
                    {
                        yield return t;
                    }

                    foreach (var d in t.EnumerateArray())
                    {
                        if (Expression.IsMatch(root, d, settings))
                        {
                            yield return d;
                        }
                    }
                }
            }
        }
    }
}