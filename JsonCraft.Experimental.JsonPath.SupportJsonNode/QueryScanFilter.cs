using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace JsonCraft.Experimental.JsonPath.SupportJsonNode
{
    internal class QueryScanFilter : PathFilter
    {
        internal QueryExpression Expression;

        public QueryScanFilter(QueryExpression expression)
        {
            Expression = expression;
        }

        public override IEnumerable<JsonNode?> ExecuteFilter(JsonNode root, IEnumerable<JsonNode> current, JsonSelectSettings? settings)
        {
            foreach (JsonNode t in current)
            {
                if (t is JsonObject obj)
                {
                    if (Expression.IsMatch(root, obj, settings))
                    {
                        yield return obj;
                    }

                    foreach (var d in obj)
                    {
                        if (Expression.IsMatch(root, d.Value, settings))
                        {
                            yield return d.Value;
                        }
                    }
                }
                else if (t is JsonArray arr)
                {
                    if (Expression.IsMatch(root, arr, settings))
                    {
                        yield return arr;
                    }

                    foreach (var d in arr)
                    {
                        if (Expression.IsMatch(root, d, settings))
                        {
                            yield return d;
                        }
                    }
                }
                else
                {
                    if (Expression.IsMatch(root, t, settings))
                    {
                        yield return t;
                    }
                }
            }
        }
    }
}