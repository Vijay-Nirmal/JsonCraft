using System.Collections;
using System.Text.Json;

namespace JsonCraft.JsonPath
{
    internal class ScanFilter : PathFilter
    {
        internal string? Name;

        public ScanFilter(string? name)
        {
            Name = name;
        }

        // Inspired by https://stackoverflow.com/a/30441479/7331395
        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, JsonElement current, JsonSelectSettings? settings)
        {
            if (Name is null)
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
                                if (Name is null)
                                {
                                    yield return element;
                                }
                                stack.Push(enumerator);
                            }
                            else if (enumerator is JsonElement.ObjectEnumerator objectEnumerator)
                            {
                                var element = objectEnumerator.Current;
                                innerElement = element.Value;
                                if (Name is null || element.NameEquals(Name))
                                {
                                    yield return element.Value;
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
            return current.SelectMany(x => ExecuteFilter(root, x, null));
        }
    }
}