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

        public override IEnumerable<JsonElement> ExecuteFilter(JsonElement root, IEnumerable<JsonElement> current, JsonSelectSettings? settings)
        {
            foreach (var t in current)
            {
                foreach (int i in Indexes)
                {
                    var v = GetTokenIndex(t, settings, i);

                    if (v != null)
                    {
                        yield return v.Value;
                    }
                }
            }
        }
    }
}