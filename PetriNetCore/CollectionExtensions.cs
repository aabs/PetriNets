using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PetriNetCore
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds the specs in textual shorthand
        /// </summary>
        /// <returns>a modified version of the graph with any transitions added</returns>
        /// <example>
        /// the examples below show the possible cases. Assume that place and transition names <i>must</i> be alphanumeric.
        /// <list type="table">
        /// <item>
        /// <term><c>t]-(p</c></term>
        /// <description>a simple links from a transition to a place</description>
        /// </item>
        /// <item><term><c>p)-[t</c></term>
        /// <description>a simple link from a place to a transition</description>
        /// </item>
        /// <item><term><c>p)-o[t</c></term>
        /// <description>An inhibiting link from a place to a transition</description>
        /// </item>

        /// <item><term><c>t]-2-(p</c></term>
        /// <description>a weighted link from a transition to a place (weight: 2)</description>
        /// </item>
        /// <item><term><c>p)-2-[t</c></term>
        /// <description>a weighted link from a place to a transition (weight: 2)</description>
        /// </item>
        /// <item><term><c>p)-2-o[t</c></term>
        /// <description>a weighted inhibition link from a place to a transition</description>
        /// </item>
        /// </list>
        /// </example>
        public static CreatePetriNet WithArcs(this CreatePetriNet builder, params string[] specs)
        {
            var b = builder;
            foreach (string spec in specs)
            {
                b = b.AddSpec(spec);
            }
            return b;
        }

        /// <summary>
        /// Adds the spec in textual shorthand
        /// </summary>
        /// <returns>a modified version of the graph with any transitions added</returns>
        /// <example>
        /// the examples below show the possible cases. Assume that place and transition names <i>must</i> be alphanumeric.
        /// <list type="table">
        /// <item>
        /// <term><c>t]-(p</c></term>
        /// <description>a simple links from a transition to a place</description>
        /// </item>
        /// <item><term><c>p)-[t</c></term>
        /// <description>a simple link from a place to a transition</description>
        /// </item>
        /// <item><term><c>p)-o[t</c></term>
        /// <description>An inhibiting link from a place to a transition</description>
        /// </item>

        /// <item><term><c>t]-2-(p</c></term>
        /// <description>a weighted link from a transition to a place (weight: 2)</description>
        /// </item>
        /// <item><term><c>p)-2-[t</c></term>
        /// <description>a weighted link from a place to a transition (weight: 2)</description>
        /// </item>
        /// <item><term><c>p)-2-o[t</c></term>
        /// <description>a weighted inhibition link from a place to a transition</description>
        /// </item>
        /// </list>
        /// </example>
        public static CreatePetriNet AddSpec(this CreatePetriNet builder, string spec)
        {
            Parser parser = new Parser(new Scanner(new MemoryStream(ASCIIEncoding.Default.GetBytes(spec))));
            parser.Builder = builder;
            parser.Parse();
            return builder;
            char[] chars = spec
                            .Trim()
                            .ToCharArray();
            StringBuilder src = new StringBuilder();
            StringBuilder arc = new StringBuilder();
            StringBuilder dst = new StringBuilder();
            int pos = 0;

            while(pos < chars.Length && Char.IsWhiteSpace(chars[pos])) pos++;
            while (pos < chars.Length && Char.IsLetterOrDigit(chars[pos]))
            {
                src.Append(chars[pos]);
                pos++;
            }
            while(pos < chars.Length && Char.IsWhiteSpace(chars[pos])) pos++;
            bool hasReachedEndPort = false;
            while (pos < chars.Length && !hasReachedEndPort && !Char.IsWhiteSpace(chars[pos]))
            {
                if (chars[pos] == '[' || chars[pos] == '(')
                {
                    hasReachedEndPort = true;
                }
                arc.Append(chars[pos]);
                pos++;
            }
            while(pos < chars.Length && Char.IsWhiteSpace(chars[pos])) pos++;
            while (pos < chars.Length && Char.IsLetterOrDigit(chars[pos]))
            {
                dst.Append(chars[pos]);
                pos++;
            }
            bool isIntoTransition = true;
            bool isInhibitor = false;
            int weight = 1;
            
            // interpret the arc specification
            pos = 0;
            if (pos < arc.Length && arc[pos] == ']')
            {
                isIntoTransition = false;
            }
            else if (pos < arc.Length && arc[pos] != ')')
            {
                throw new ApplicationException("unrecognised arc specification (src type)");
            }
            pos++;
            while (pos < arc.Length && arc[pos] == '-')
            {
                pos++;
            }
            StringBuilder sbWeight = new StringBuilder();
            if (pos < arc.Length && Char.IsDigit(arc[pos]))
            {
                while (pos < arc.Length && Char.IsDigit(arc[pos]))
                {
                    sbWeight.Append(arc[pos]);
                    pos++;
                }
            }
            while (pos < arc.Length && arc[pos] == '-')
            {
                pos++;
            }
            if (pos < arc.Length && arc[pos] == 'o')
            {
                isInhibitor = true;
                pos++;
            }
            while (pos < arc.Length && arc[pos] == '-')
            {
                pos++;
            }
            if (isIntoTransition)
            {
                if (pos < arc.Length && arc[pos] != '[')
                {
                    throw new ApplicationException("malformed arc spec");
                }
            }
            if (sbWeight.Length > 0)
            {
                int.TryParse(sbWeight.ToString(),
                             out weight);
            }
            string t = isIntoTransition ? dst.ToString() : src.ToString();
            string p = isIntoTransition ? src.ToString() : dst.ToString();
            return GenerateArc(builder,
                        p,
                        t,
                        weight,
                        isInhibitor,
                        isIntoTransition);
        }

        private static CreatePetriNet GenerateArc(CreatePetriNet builder,
                                                 string p,
                                                 string t,
                                                 int weight,
                                                 bool isInhibitor,
                                                 bool isIntoTransition)
        {
            builder.WithPlaces(p).WithTransitions(t);
            var connectionBuilder = builder.With(t).Weight(weight);
            if (isInhibitor)
            {
                connectionBuilder.AsInhibitor();
            }
            if (isIntoTransition)
            {
                connectionBuilder.FedBy(p);
            }
            else
            {
                connectionBuilder.Feeding(p);
            }
            return connectionBuilder.Done();
        }

        public static V SafeGet<K, V>(this Dictionary<K, V> d, K k)
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