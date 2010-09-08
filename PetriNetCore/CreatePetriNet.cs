using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

public partial class Parser
{

}

namespace PetriNetCore
{
    public class CreatePetriNet
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
        public static CreatePetriNet Parse(string spec)
        {
            Parser parser = new Parser(new Scanner(new MemoryStream(ASCIIEncoding.Default.GetBytes(spec))));
            parser.Parse();
            if (parser.errors.count > 0)
            {
                throw new ParserException(parser.errors);
            }
            return parser.Builder;
        }

        public PetriNetCore.CreatePetriNet GenerateArc(List<string> p,
                                                    List<string> t,
                                                    int weight,
                                                    bool isInhibitor,
                                                    bool isIntoTransition)
        {
            Contract.Requires(!p.Any(x => string.IsNullOrWhiteSpace(x)));
            Contract.Requires(!t.Any(x => string.IsNullOrWhiteSpace(x)));
            Contract.Requires(p.Intersect(t).Count() == 0);
            Contract.Requires(weight >= 1);
            var places = p.ToArray();
            var transitions = t.ToArray();
            var builder = WithPlaces(places).WithTransitions(transitions);
            foreach (var tran in t)
            {
                var connectionBuilder = builder.With(tran).Weight(weight);
                if (isInhibitor)
                {
                    connectionBuilder.AsInhibitor();
                }
                if (isIntoTransition)
                {
                    connectionBuilder.FedBy(places);
                }
                else
                {
                    connectionBuilder.Feeding(places);
                }
                connectionBuilder.Done();
            }
            return builder;
        }

        public string Name { get; set; }
        public Dictionary<int, string> Places { get; set; }
        public Dictionary<int, string> Transitions { get; set; }
        public Dictionary<int, List<InArc>> InArcs { get; set; }
        public Dictionary<int, List<OutArc>> OutArcs { get; set; }
        public Dictionary<int, List<Action<GraphPetriNet>>> TransitionFunctions { get; set; }
        public CreatePetriNet(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || !name.All(Char.IsLetterOrDigit))
            {
                throw new ArgumentException("name");
            }
            Name = name;
        }

        public static CreatePetriNet Called(string name)
        {
            var result = new CreatePetriNet(name);
            return result;
        }

        public CreatePetriNet WithPlaces(params string[] placeNames)
        {
            Contract.Requires(placeNames.Count() != 0);
            Contract.Requires(placeNames.All(s1 => !string.IsNullOrWhiteSpace(s1)));
            Contract.Requires(placeNames.All(s1 => s1.All(Char.IsLetterOrDigit)));

            if (Places == null)
            {
                Places = new Dictionary<int, string>();
            }
            var tmp = placeNames.Select((s,
                                        i) => Tuple.Create(i,
                                                           s)).ToDictionary(tuple => tuple.Item1,
                                                                            tuple1 => tuple1.Item2);

            int count = (Places.Count>0? Places.Keys.Max():-1) + 1;

            foreach (var item in tmp)
            {
                if (!Places.ContainsValue(item.Value))
                {
                    Places[count] = item.Value;
                    count++;
                }
            }
            return this;
        }
        public CreatePetriNet AndPlaces(params string[] placeNames) { return WithPlaces(placeNames); }
        public CreatePetriNet WithTransitions(params string[] transitionNames)
        {
            Contract.Requires(transitionNames.Count() != 0);
            Contract.Requires(transitionNames.All(s1 => !string.IsNullOrWhiteSpace(s1)));
            Contract.Requires(transitionNames.All(s1 => s1.All(Char.IsLetterOrDigit)));

            if (Transitions == null)
            {
                Transitions = new Dictionary<int, string>();
            }

            var tmp = transitionNames.Select((s,
                                        i) => Tuple.Create(i,
                                                           s)).ToDictionary(tuple => tuple.Item1,
                                                                            tuple1 => tuple1.Item2);

            int count = (Transitions.Count > 0 ? Transitions.Keys.Max() : -1) + 1;

            foreach (var item in tmp)
            {
                if (!Transitions.ContainsValue(item.Value))
                {
                    Transitions[count] = item.Value;
                    count++;
                }
            }
            return this;
        }
        public CreatePetriNet AndTransitions(params string[] transitionNames) { return WithTransitions(transitionNames); }
        public PetriNetConnectionBuilder With(string transitionName)
        {
            var result = new PetriNetConnectionBuilder(this, transitionName);
            return result;
        }

        public T CreateNet<T>() where T : class
        {
            if (typeof(T).Equals( typeof(GraphPetriNet)))
            {
                return new GraphPetriNet(
                    Name,
                    Places,
                    Transitions,
                    InArcs,
                    OutArcs) as T;
            }
            if (typeof(T).Equals(typeof(MatrixPetriNet)))
            {
                return new MatrixPetriNet(
                    Name,
                    Places,
                    Transitions,
                    InArcs,
                    OutArcs) as T;
            }
            throw new ApplicationException("Unrecognised petri net type requested");
        }

        public Marking CreateMarking()
        {
            return new Marking(Places.Count);
        }

        public PetriNetEventBuilder WhenFiring(string transitionName)
        {
            var result = new PetriNetEventBuilder(this, transitionName);
            return result;
        }

        public int TransitionIndex(string name)
        {
            return Transitions.Where(pair => pair.Value == name).Select(valuePair => valuePair.Key).First();
        }

        public int PlaceIndex(string name)
        {
            return Places.Where(pair => pair.Value == name).Select(valuePair => valuePair.Key).First();
        }
        public void AddInArc(string placeName,
                             string transitionName,
                             bool isInhibitor,
                             int weight)
        {
            if (InArcs == null)
            {
                InArcs = new Dictionary<int, List<InArc>>();
            }
            int fromIndex = PlaceIndex(placeName);
            int toIndex = TransitionIndex(transitionName);

            if (!InArcs.ContainsKey(fromIndex))
            {
                InArcs[fromIndex] = new List<InArc>();
            }
            InArcs[fromIndex].Add(new InArc(toIndex, weight, isInhibitor));
        }

        public void AddOutArc(string transitionName, string placeName,
                             int weight = 1)
        {
            if (OutArcs == null)
            {
                OutArcs = new Dictionary<int, List<OutArc>>();
            }
            int fromIndex = TransitionIndex(transitionName); 
            int toIndex = PlaceIndex(placeName);
            if (!OutArcs.ContainsKey(fromIndex))
            {
                OutArcs[fromIndex] = new List<OutArc>();
            }
            OutArcs[fromIndex].Add(new OutArc(toIndex, weight));
        }

        public void AddEvent(string transitionName, Action<GraphPetriNet> task)
        {
            var transition = TransitionIndex(transitionName);
            if (TransitionFunctions == null)
            {
                TransitionFunctions = new Dictionary<int, List<Action<GraphPetriNet>>>();
            }
            if (!TransitionFunctions.ContainsKey(transition))
            {
                TransitionFunctions[transition] = new List<Action<GraphPetriNet>>();
            }
            TransitionFunctions[transition].Add(task);
        }
    }
}