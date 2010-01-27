using System;
using System.Collections.Generic;
using System.Linq;

namespace PetriNetCore
{
    public class PetriNet
    {
        #region ctors
        public PetriNet()
        {
        }

        public PetriNet(
            string id,
            IEnumerable<Tuple<int, string, int>> places,
            IEnumerable<Tuple<int, string>> transitions,
            IEnumerable<ArcDto> arcs)
        {
            Id = id;
            ImportPlaces(places);
            ImportTransitions(transitions);
            ImportArcs(arcs);
        }

        public PetriNet(string id,
                        Dictionary<int, string> placeNames,
                        Dictionary<int, int> markings,
                        Dictionary<int, string> transitionNames,
                        Dictionary<int, List<InArc>> inArcs,
                        Dictionary<int, List<OutArc>> outArcs
            )
        {
#if USING_CONTRACTS
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Requires(placeNames != null);
            Contract.Requires(placeNames.Count > 0);
            Contract.Requires(placeNames.All(pn => !string.IsNullOrEmpty(pn.Value)));
            Contract.Requires(markings != null);
            Contract.Requires(transitionNames != null);
            Contract.Requires(transitionNames.Count > 0);
            Contract.Requires(transitionNames.All(tn => !string.IsNullOrEmpty(tn.Value)));
            Contract.Requires(inArcs != null);
            Contract.Requires(inArcs.Count>0);
            Contract.Requires(outArcs!=null);
            Contract.Requires(outArcs.Count>0);

            Contract.Ensures(Places.Count == placeNames.Count);
            Contract.Ensures(Places == placeNames);
#endif
            Id = id;
            Places = placeNames;
            Markings = markings;
            Transitions = transitionNames;
            Markings = markings;
            InArcs = inArcs;
            OutArcs = outArcs;
        }

        #endregion

        #region importation of models
        private void ImportArcs(IEnumerable<ArcDto> arcs)
        {
            foreach (var arc in arcs)
            {
                if (arc.FromPlace)
                {
                    AddArcFromPlace(arc.FromId, arc.ToId);
                    AddArcIntoTransition(arc.FromId, arc.ToId);
                }
                else
                {
                    AddArcFromTransition(arc.FromId, arc.ToId);
                }
            }
        }

        private void ImportPlaces(IEnumerable<Tuple<int, string, int>> places)
        {
#if USING_CONTRACTS
            Contract.Requires(places != null);
            Contract.Requires(places.Count() > 0);
            Contract.Ensures(Places.Count == places.Count());
#endif
            places.Foreach(x => Places.Add(x.Item1, x.Item2));
            CollectionExtensions.Foreach<Tuple<int, string, int>>(places.Where(p => p.Item3 > 0), x => SetMarking(x.Item1, x.Item3));
        }

        private void ImportTransitions(IEnumerable<Tuple<int, string>> transitions)
        {
            transitions.Foreach(x => Transitions.Add(x.Item1, x.Item2));
        }
        #endregion

        #region graph model and state data
        public string Id { get; set; }

        public Dictionary<int, string> Places = new Dictionary<int, string>();
        public Dictionary<int, string> Transitions = new Dictionary<int, string>();

        public Dictionary<int, int> Markings = new Dictionary<int, int>();
        public Dictionary<int, List<InArc>> InArcs = new Dictionary<int, List<InArc>>();
        public Dictionary<int, List<OutArc>> OutArcs = new Dictionary<int, List<OutArc>>();
        public Dictionary<int, List<OutArc>> PlaceOutArcs = new Dictionary<int, List<OutArc>>();

        public Dictionary<int, List<Action<int>>> TransitionFunctions = new Dictionary<int, List<Action<int>>>();
        #endregion

        #region graph construction
        public void AddArcFromTransition(int placeId, int transitionId)
        {
            if (!OutArcs.ContainsKey(transitionId))
            {
                OutArcs[transitionId] = new List<OutArc>();
            }

            OutArcs[transitionId].Add(new OutArc(placeId)
                                          {
                                              Weight = 1
                                          });
        }

        public void AddArcIntoTransition(int placeId, int transitionId)
        {
            if (!InArcs.ContainsKey(transitionId))
            {
                InArcs[transitionId] = new List<InArc>();
            }

            InArcs[transitionId].Add(new InArc(placeId)
                                         {
                                             Weight = 1,
                                             IsInhibitor = false
                                         });
        }

        public void AddArcFromPlace(int placeId, int transitionId)
        {
            if (!PlaceOutArcs.ContainsKey(placeId))
            {
                PlaceOutArcs[placeId] = new List<OutArc>();
            }

            PlaceOutArcs[placeId].Add(new OutArc(transitionId)
                                          {
                                              Weight = 1
                                          });
        }

        public void AddLinearTransition(int placeIn, int placeOut, int transitionId)
        {
            AddArcFromPlace(placeIn, transitionId);
            AddArcIntoTransition(placeIn, transitionId);
            AddArcFromTransition(placeOut, transitionId);
        }

        public void CreateTransitions(IEnumerable<Tuple<int, int, int>> transitions)
        {
            foreach (var t in transitions)
            {
                AddLinearTransition(t.Item1, t.Item2, t.Item3);
            }
        }

        #endregion

        #region accessors
        public int GetMarking(int placeId)
        {
#if USING_CONTRACTS
            Contract.Requires(Places.ContainsKey(placeId));
#endif
            if (!Markings.ContainsKey(placeId))
            {
                return 0;
            }

            return Markings[placeId];
        }

        internal IEnumerable<InArc> InhibitorsIntoTransition(int transitionId)
        {
            if (InArcs.ContainsKey(transitionId))
            {
                return from a in InArcs[transitionId]
                       where a.IsInhibitor
                       select a;
            }
            return new InArc[] {};
        }

        internal IEnumerable<InArc> GetInArcs(int transitionId)
        {
            if (!InArcs.ContainsKey(transitionId))
                return new InArc[] { };
            return InArcs[transitionId];
        }

        internal IEnumerable<OutArc> GetOutArcs(int transitionId)
        {
            if (!OutArcs.ContainsKey(transitionId))
                return new OutArc[] { };
            return OutArcs[transitionId];
        }

        internal IEnumerable<Action<int>> GetTransitionFunctions(int transitionId)
        {
            if (!TransitionFunctions.ContainsKey(transitionId))
                return new Action<int>[] { };
            return TransitionFunctions[transitionId];
        }

        internal IEnumerable<InArc> NonInhibitorsIntoTransition(int transitionId)
        {
            if (!InArcs.ContainsKey(transitionId))
                return new InArc[] { };
            return GetInArcs(transitionId).Except(InhibitorsIntoTransition(transitionId));
        }

        internal IEnumerable<int> EnabledTransitions()
        {
            return (from t in Transitions
                    where IsEnabled(t.Key)
                    select t.Key);
        }

        internal List<OutArc> AllSourcePlaces()
        {
            List<OutArc> result = new List<OutArc>();
            foreach (var p in OutArcs)
            {
                result.Concat(OutArcs[p.Key]);
            }
            return result;
        }

        internal List<InArc> AllDestinationPlaces()
        {
            List<InArc> result = new List<InArc>();
            foreach (var p in InArcs)
            {
                result.Concat(InArcs[p.Key]);
            }
            return result;
        }

        internal IEnumerable<int> AllPlaces()
        {
            var result = AllSourcePlaces()
                .Select(x => x.Target)
                .Concat(AllDestinationPlaces()
                            .Select(y => y.Source)).ToList();
            return result;
        }

        internal IEnumerable<int> AllMarkedPlaces()
        {
            var result = (from p in Markings select p.Key).ToList();
            return result;
        }

        internal IEnumerable<int> PlacesFeedingIntoTransitions()
        {
            var result = (from p in PlaceOutArcs select p.Key).ToList();
            return result;
        }

        #endregion

        #region extensibility
        public void RegisterFunction(int transitionId, Action<int> fn)
        {
            if (!TransitionFunctions.ContainsKey(transitionId))
            {
                TransitionFunctions[transitionId] = new List<Action<int>>();
            }
            TransitionFunctions[transitionId].Add(fn);
        }
        #endregion

        #region net execution
        bool AllInhibitorsAreFromEmptyPlaces(int transitionId)
        {
            return InhibitorsIntoTransition(transitionId).All(ia => Markings[ia.Source] == 0);
        }

        bool AllInArcPlacesHaveMoreTokensThanTheArcWeight(int transitionId)
        {
            var arcs = NonInhibitorsIntoTransition(transitionId);
            var result = arcs.All(ia => Markings[ia.Source] >= ia.Weight);
            return result;
        }

        bool IsEmptyTransition(int transitionId)
        {
            return (!InArcs.ContainsKey(transitionId) || InArcs[transitionId].Count == 0);
        }

        public bool IsEnabled(int transitionId)
        {
            var a = IsEmptyTransition(transitionId);
            var b = AllInhibitorsAreFromEmptyPlaces(transitionId);
            var c = AllInArcPlacesHaveMoreTokensThanTheArcWeight(transitionId);
            // if all inhibitors are empty and all non inhibitors have as many tokens in their origin place as their weight
            return (a || (b && c));
        }

        public void SetMarking(int placeId, int marking)
        {
            if (marking < 0)
            {
                throw new ArgumentOutOfRangeException("marking");
            }

            Markings[placeId] = marking;
        }

        public virtual void Fire()
        {
            // pick a transition to fire at random
            var enabledTransitions = EnabledTransitions();
            var count = enabledTransitions.Count();

            if (count == 0)
                return; // perhaps we should thrown an exception here?

            var r = new Random();
            var tran = enabledTransitions.ElementAt(r.Next(count));

            foreach (var place in GetInArcs(tran).Where(x => x.IsInhibitor == false))
                Markings[place.Source] = Markings.SafeGet(place.Source) - 1;

            foreach (var arc in GetOutArcs(tran))
                Markings[arc.Target] = Markings.SafeGet(arc.Target) + arc.Weight;

            if (TransitionFunctions.ContainsKey(tran))
                TransitionFunctions[tran].ForEach(a => a(tran));
        }
        #endregion

#if USING_CONTRACTS
        [ContractInvariantMethod]
        protected void ObjectInvariant()
        {
            Contract.Invariant(!string.IsNullOrEmpty(Id), "No empty petri net ID");
            Contract.Invariant(Places != null, "no null places");
            Contract.Invariant(Transitions != null, "no null transitions");
            Contract.Invariant(Markings != null, "no null markings");
            Contract.Invariant(InArcs != null, "no null InArcs");
            Contract.Invariant(OutArcs != null, "no null OutArcs");
            Contract.Invariant(PlaceOutArcs != null, "no null PlaceOutArcs");
            Contract.Invariant(TransitionFunctions != null, "no null TransitionFunctions");
        }
#endif
    }
}