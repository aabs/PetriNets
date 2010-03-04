using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;

namespace PetriNetCore
{
    public static class CollectionExtensions
    {
        public static V SafeGet<K,V>(this Dictionary<K, V> d, K k)
        {
            if (d.ContainsKey(k))
            {
                return d[k];
            }
            return default(V);
        }

        public static IEnumerable<TResult> Map<TInput, TResult>(this IEnumerable<TInput> seq, Func<TInput, TResult> mapping)
        {
            foreach (var item in seq)
            {
                yield return mapping(item);
            }
        }
        public static void Foreach<TInput>(this IEnumerable<TInput> seq, Action<TInput> job)
        {
#if USING_CONTRACTS
            Contract.Requires(seq != (IEnumerable<Tuple<int, string, int>>)null);
#endif
            foreach (var item in seq)
            {
                job(item);
            }
        }
    }
}