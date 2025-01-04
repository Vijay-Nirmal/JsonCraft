using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace JsonCraft.Experimental.JsonPath.SupportJsonNode
{
    internal class ArrayMultipleIndexFilter : PathFilter
    {
        internal List<int> Indexes;

        public ArrayMultipleIndexFilter(List<int> indexes)
        {
            Indexes = indexes;
        }

        public override IEnumerable<JsonNode?> ExecuteFilter(JsonNode root, IEnumerable<JsonNode> current, JsonSelectSettings? settings)
        {
            foreach (JsonNode t in current)
            {
                foreach (int i in Indexes)
                {
                    if (TryGetTokenIndex(t, settings, i, out var v))
                    {
                        yield return v;
                    }
                }
            }
        }
    }
}