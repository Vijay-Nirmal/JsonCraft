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

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, JsonElement current, JsonSelectSettings? settings)
        {
            if (current.ValueKind == JsonValueKind.Array)
            {
                foreach (var v in current.EnumerateArray())
                {
                    if (Expression.IsMatch(root, v, settings))
                    {
                        yield return v;
                    }
                }
            }
            else if (current.ValueKind == JsonValueKind.Object)
            {
                foreach (var v in current.EnumerateObject())
                {
                    if (Expression.IsMatch(root, v.Value, settings))
                    {
                        yield return v.Value;
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