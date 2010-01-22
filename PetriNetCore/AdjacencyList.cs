using System.Collections.Generic;

namespace PetriNetCore
{
    public class AdjacencyList<TKey, TValue>
    {
        public TKey Key { get; set; }
        public HashSet<TValue> AdjacentItems { get; set; }
        public AdjacencyList()
        {
            AdjacentItems = new HashSet<TValue>();
        }

        public AdjacencyList(TKey key)
            : this()
        {
            Key = key;
        }

        public AdjacencyList(TKey key, IEnumerable<TValue> adjSeq)
            : this(key)
        {
            AdjacentItems.UnionWith(adjSeq);
        }

        public bool IsAdjacentTo(TValue x)
        {
            return AdjacentItems.Contains(x);
        }

        public void AddAdjacentItem(TValue x)
        {
            AdjacentItems.Add(x);
        }
    }
}