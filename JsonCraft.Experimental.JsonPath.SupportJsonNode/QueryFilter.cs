using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace JsonCraft.Experimental.JsonPath.SupportJsonNode
{
    internal class QueryFilter : PathFilter
    {
        internal QueryExpression Expression;

        public QueryFilter(QueryExpression expression)
        {
            Expression = expression;
        }

        public override IEnumerable<JsonNode?> ExecuteFilter(JsonNode root, IEnumerable<JsonNode> current, JsonSelectSettings? settings)
        {
            foreach (JsonNode t in current)
            {
                if (t is JsonArray array)
                {
                    foreach (var v in array)
                    {
                        if (Expression.IsMatch(root, v, settings))
                        {
                            yield return v;
                        }
                    }
                }
                else if (t is JsonArray obj)
                {
                    foreach (var v in obj)
                    {
                        if (Expression.IsMatch(root, v, settings))
                        {
                            yield return v;
                        }
                    }
                }
            }
        }
    }
}