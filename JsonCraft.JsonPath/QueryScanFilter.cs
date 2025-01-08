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
                    foreach (var item in ExecuteFilter(root, d.Value, settings))
                    {
                        yield return item;
                    }
                }
            }
            else if (current.ValueKind == JsonValueKind.Array)
            {
                foreach (var d in current.EnumerateArray())
                {
                    foreach (var item in ExecuteFilter(root, d, settings))
                    {
                        yield return item;
                    }
                }
            }
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            foreach (var item in current)
            {
                if (Expression.IsMatch(root, item, settings))
                {
                    yield return item;
                }

                if (item.ValueKind == JsonValueKind.Object)
                {
                    foreach (var d in item.EnumerateObject())
                    {
                        foreach (var objItem in ExecuteFilter(root, d.Value, settings))
                        {
                            yield return objItem;
                        }
                    }
                }
                else if (item.ValueKind == JsonValueKind.Array)
                {
                    foreach (var d in item.EnumerateArray())
                    {
                        foreach (var arrItem in ExecuteFilter(root, d, settings))
                        {
                            yield return arrItem;
                        }
                    }
                }
            }
        }
    }
}