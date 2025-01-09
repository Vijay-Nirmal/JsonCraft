using System.Collections;
using System.Text.Json;
using System.Xml.Linq;

namespace JsonCraft.JsonPath
{
    internal class ScanMultipleFilter : PathFilter
    {
        private List<string> _names;

        public ScanMultipleFilter(List<string> names)
        {
            _names = names;
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, JsonElement current, JsonSelectSettings? settings)
        {
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
                try
                {
                    while (true)
                    {
                        if (enumerator.MoveNext())
                        {
                            JsonElement innerElement = default;
                            if (enumerator is JsonElement.ArrayEnumerator arrayEnumerator)
                            {
                                var element = arrayEnumerator.Current;
                                innerElement = element;
                                stack.Push(enumerator);
                            }
                            else if (enumerator is JsonElement.ObjectEnumerator objectEnumerator)
                            {
                                var element = objectEnumerator.Current;
                                innerElement = element.Value;
                                // TODO: Using NameEquals instead of Contains with foreach load reduces allocation but increases CPU time, so don't doing that. In Net 10, Try Use GetRawUtf8PropertyName and use AlternateLookup with HashSet(if it doesn't increase the memory usage)
                                if (_names.Contains(element.Name))
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
                            (enumerator as IDisposable)?.Dispose();
                            enumerator = stack.Pop();
                        }
                        else
                        {
                            yield break;
                        }
                    }
                }
                finally
                {
                    (enumerator as IDisposable)?.Dispose();

                    while (stack.Count > 0) // Clean up in case of an exception.
                    {
                        enumerator = stack.Pop();
                        (enumerator as IDisposable)?.Dispose();
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