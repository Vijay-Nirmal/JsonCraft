using System.Text.Json;

namespace JsonCraft.JsonPath
{
    internal class QueryFilter : PathFilter
    {
        internal QueryExpression Expression;

        public QueryFilter(QueryExpression expression)
        {
            Expression = expression;
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            foreach (var t in current)
            {
                if (t.ValueKind == JsonValueKind.Array)
                {
                    foreach (var v in t.EnumerateArray())
                    {
                        if (Expression.IsMatch(root, v, settings))
                        {
                            yield return v;
                        }
                    }
                }
                else if (t.ValueKind == JsonValueKind.Object)
                {
                    foreach (var v in t.EnumerateObject())
                    {
                        if (Expression.IsMatch(root, v.Value, settings))
                        {
                            yield return v.Value;
                        }
                    }
                }
            }
        }
    }
}