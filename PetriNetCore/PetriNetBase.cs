using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnAnalytics.LinearAlgebra;
using System.Diagnostics;

namespace PetriNetCore
{
    public class Marking : SparseVector 
    {
        public Marking(int size):base(size){}
        public Marking(int size, IDictionary<int, int> markings):base(size)
        {
            Debug.Assert(markings.Keys.Max() < size);
            Debug.Assert(markings.Keys.Min() >= 0);
            markings.Foreach(x => { this[x.Key] = x.Value; });
        }
        public Marking(Marking m) : base(m) { }
    }

    public interface IPetriNet
    {
        Marking CreateInitialMarking();
        IEnumerable<int> AllPlaces();
        IEnumerable<int> InhibitorsIntoTransition(int transitionId);
        IEnumerable<int> NonInhibitorsIntoTransition(int transitionId);
        int GetWeight(int placeid, int transid);
        IEnumerable<int> GetPlaceOutArcs(int placeId);
        bool IsEmptyTransition(int transitionId);
    }

    public abstract class PetriNetBase
    {
        public abstract IEnumerable<int> AllPlaces();
        public abstract IEnumerable<int> InhibitorsIntoTransition(int transitionId);
        public abstract IEnumerable<int> NonInhibitorsIntoTransition(int transitionId);
        public abstract int GetWeight(int placeid, int transid);
        public abstract IEnumerable<int> GetPlaceOutArcs(int placeId);
        public abstract bool IsEmptyTransition(int transitionId);

        public Marking CreateInitialMarking()
        {
            return new Marking(AllPlaces().Count());
        }
        public bool IsConflicted(Marking m)
        {
            return AllPlaces().Any(pid => PlaceIsConflicted(pid, m));
        }
        public bool PlaceIsConflicted(int placeId, Marking m)
        {
            return GetEnabledTransitionsAdjacentToPlace(placeId, m).Count() > 1;
        }
        public IEnumerable<int> GetEnabledTransitionsAdjacentToPlace(int placeId, Marking m)
        {
            var q = (from outArc in GetPlaceOutArcs(placeId)
                     where IsEnabled(outArc, m)
                     select outArc).ToArray();
            return q;
        }
        public bool IsEnabled(int transitionId, Marking m)
        {
            var a = IsEmptyTransition(transitionId);
            var b = AllInhibitorsAreFromEmptyPlaces(transitionId, m);
            var c = AllInArcPlacesHaveMoreTokensThanTheArcWeight(transitionId, m);
            // if all inhibitors are empty and all non inhibitors have as many tokens in their origin place as their weight
            return (a || (b && c));
        }
        bool AllInhibitorsAreFromEmptyPlaces(int transitionId, Marking m)
        {
            return InhibitorsIntoTransition(transitionId).All(placeid => m[placeid] == 0);
        }
        bool AllInArcPlacesHaveMoreTokensThanTheArcWeight(int transitionId, Marking m)
        {
            var arcs = NonInhibitorsIntoTransition(transitionId);
            var result = arcs.All(placeid => m[placeid] >= GetWeight(placeid, transitionId));
            return result;
        }

    }
}
