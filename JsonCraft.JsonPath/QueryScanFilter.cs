using System.Collections;
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

            IEnumerator? enumerator = null;
            if (current.ValueKind == JsonValueKind.Array)
            {
                enumerator = current.EnumerateArray();
            }
            else if (current.ValueKind == JsonValueKind.Object)
            {
                enumerator = current.EnumerateObject();
            }

            if (enumerator is not null)
            {
                var stack = new Stack<IEnumerator>();
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        JsonElement innerElement = default;
                        if (enumerator is JsonElement.ArrayEnumerator arrayEnumerator)
                        {
                            innerElement = arrayEnumerator.Current;
                            if (Expression.IsMatch(root, innerElement, settings))
                            {
                                yield return innerElement;
                            }
                            stack.Push(enumerator);
                        }
                        else if (enumerator is JsonElement.ObjectEnumerator objectEnumerator)
                        {
                            innerElement = objectEnumerator.Current.Value;
                            if (Expression.IsMatch(root, innerElement, settings))
                            {
                                yield return innerElement;
                            }
                            stack.Push(enumerator);
                        }

                        if (innerElement.ValueKind == JsonValueKind.Array)
                        {
                            enumerator = innerElement.EnumerateArray();
                        }
                        else if (innerElement.ValueKind == JsonValueKind.Object)
                        {
                            enumerator = innerElement.EnumerateObject();
                        }
                    }
                    else if (stack.Count > 0)
                    {
                        enumerator = stack.Pop();
                    }
                    else
                    {
                        yield break;
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