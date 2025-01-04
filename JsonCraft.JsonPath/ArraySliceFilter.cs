using System.Globalization;
using System.Text.Json;

namespace JsonCraft.JsonPath
{
    internal class ArraySliceFilter : PathFilter
    {
        public int? Start { get; set; }
        public int? End { get; set; }
        public int? Step { get; set; }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            if (Step == 0)
            {
                throw new JsonException("Step cannot be zero.");
            }

            foreach (var t in current)
            {
                if (t.ValueKind == JsonValueKind.Array)
                {
                    int count = t.GetArrayLength();

                    // set defaults for null arguments
                    int stepCount = Step ?? 1;
                    int startIndex = Start ?? ((stepCount > 0) ? 0 : count - 1);
                    int stopIndex = End ?? ((stepCount > 0) ? count : -1);

                    // start from the end of the list if start is negative
                    if (Start < 0)
                    {
                        startIndex = count + startIndex;
                    }

                    // end from the start of the list if stop is negative
                    if (End < 0)
                    {
                        stopIndex = count + stopIndex;
                    }

                    // ensure indexes keep within collection bounds
                    startIndex = Math.Max(startIndex, (stepCount > 0) ? 0 : int.MinValue);
                    startIndex = Math.Min(startIndex, (stepCount > 0) ? count : count - 1);
                    stopIndex = Math.Max(stopIndex, -1);
                    stopIndex = Math.Min(stopIndex, count);

                    bool positiveStep = (stepCount > 0);

                    if (IsValid(startIndex, stopIndex, positiveStep))
                    {
                        for (int i = startIndex; IsValid(i, stopIndex, positiveStep); i += stepCount)
                        {
                            yield return t[i];
                        }
                    }
                    else
                    {
                        if (settings?.ErrorWhenNoMatch ?? false)
                        {
                            throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Array slice of {0} to {1} returned no results.",
                                Start != null ? Start.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : "*",
                                End != null ? End.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : "*"));
                        }
                    }
                }
                else
                {
                    if (settings?.ErrorWhenNoMatch ?? false)
                    {
                        throw new JsonException(string.Format(CultureInfo.InvariantCulture, "Array slice is not valid on {0}.", t.GetType().Name));
                    }
                }
            }
        }

        private bool IsValid(int index, int stopIndex, bool positiveStep)
        {
            if (positiveStep)
            {
                return (index < stopIndex);
            }

            return (index > stopIndex);
        }
    }
}