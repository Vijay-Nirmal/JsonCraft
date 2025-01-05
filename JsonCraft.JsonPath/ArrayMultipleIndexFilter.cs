using System.Text.Json;

namespace JsonCraft.JsonPath
{
    internal class ArrayMultipleIndexFilter : PathFilter
    {
        internal List<int> Indexes;

        public ArrayMultipleIndexFilter(List<int> indexes)
        {
            Indexes = indexes;
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, JsonElement current, JsonSelectSettings? settings)
        {
            foreach (int i in Indexes)
            {
                var v = GetTokenIndex(current, settings, i);

                if (v != null)
                {
                    yield return v.Value;
                }
            }
        }

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            foreach (var item in current)
            {
                // Note: Not calling ExecuteFilter with yield return because that approach is slower and uses more memory. So we have duplicated code here.
                foreach (int i in Indexes)
                {
                    var v = GetTokenIndex(item, settings, i);

                    if (v != null)
                    {
                        yield return v.Value;
                    }
                }
            }
        }
    }
}