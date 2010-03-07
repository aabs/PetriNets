/*
 * Created by SharpDevelop.
 * User: Andrew Matthews
 * Date: 12/08/2009
 * Time: 9:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Linq;
using System.Collections.Generic;
using dnAnalytics.LinearAlgebra;
using System.Diagnostics.Contracts;

namespace PetriNetCore
{
    /// <summary>
    /// Description of MatrixPetriNet.
    /// </summary>
    public class MatrixPetriNet : PetriNetBase, IPetriNet
    {
        #region ctors
        /// <summary>
        /// Create a new Petri Net using Sparse Matrices for arc representation
        /// </summary>
        /// <param name="id">The name of the Petri net</param>
        /// <param name="placeNames">A complete list of the names of the places</param>
        /// <param name="markings">A mapping between the places and integers. The initial marking of the nett</param>
        /// <param name="transitionNames">A zero based contiguous sequence of names for each of the transitions in the net</param>
        /// <param name="inArcs">Arcs coming into transitions</param>
        /// <param name="outArcs">Arcs from transitions into places</param>
        /// <remarks>
        /// <see cref="placeNames"/> and <see cref="transitionNames"/> must contain a contiguous sequence of
        /// identifier for all of the places and transitions. These values are used to calculate
        /// what the dimensions of the matrices are, so the keys of the dictionaries must be zero based and
        /// contiguous.
        /// </remarks>
        [ContractVerification(false)]
        public MatrixPetriNet(string id,
            Dictionary<int, string> placeNames,
            Dictionary<int, string> transitionNames,
            Dictionary<int, List<InArc>> inArcs,
            Dictionary<int, List<OutArc>> outArcs,
            Dictionary<int, int> transitionOrdering)
            : this(id, placeNames, transitionNames, inArcs, outArcs)
        {
            var x = transitionNames.Select(t => t.Key).Except(transitionOrdering.Select(t => t.Key));
            var y = transitionOrdering.Union(x.ToDictionary(t => t, t => 0)); // baseline priority level
            TransitionPriorities = y.ToDictionary(a => a.Key, a => a.Value);
        }

        /// <summary>
        /// Create a new Petri Net using Sparse Matrices for arc representation
        /// </summary>
        /// <param name="id">The name of the Petri net</param>
        /// <param name="placeNames">A complete list of the names of the places</param>
        /// <param name="markings">A mapping between the places and integers. The initial marking of the nett</param>
        /// <param name="transitionNames">A zero based contiguous sequence of names for each of the transitions in the net</param>
        /// <param name="inArcs">Arcs coming into transitions</param>
        /// <param name="outArcs">Arcs from transitions into places</param>
        /// <remarks>
        /// <see cref="placeNames"/> and <see cref="transitionNames"/> must contain a contiguous sequence of
        /// identifier for all of the places and transitions. These values are used to calculate
        /// what the dimensions of the matrices are, so the keys of the dictionaries must be zero based and
        /// contiguous.
        /// </remarks>
        [ContractVerification(false)]
        public MatrixPetriNet(string id,
            Dictionary<int, string> placeNames,
            Dictionary<int, string> transitionNames,
            Dictionary<int, List<InArc>> inArcs,
            Dictionary<int, List<OutArc>> outArcs
            )
        {
            Contract.Requires(!string.IsNullOrEmpty(id), "must provide valid PN ID");
            Contract.Requires(placeNames != null, "must provide a set of place names");
            Contract.Requires(placeNames.Count > 0, "places must be non-empty");
            Contract.Requires(transitionNames != null, "must provide a set of transition names");
            Contract.Requires(transitionNames.Count > 0, "there must be at lerast one transition");
            Contract.Requires(inArcs != null, "inArcs cannot be null");
            Contract.Requires(inArcs.Count > 0, "there must be some inArcs");
            Contract.Requires(outArcs != null, "outArcs cannot be null");
            Contract.Requires(outArcs.Count > 0, "there must be some out arcs");
            /*Contract.Requires(placeNames.All(x => (x.Key >= 0) && (x.Key <= placeNames.Count)), "all place names must be identifiable");
            Contract.Requires(markings.All(x => (x.Key >= 0) && (x.Key <= placeNames.Count)), "all markings must be for known place names");
            Contract.Requires(transitionNames.All(x => (x.Key >= 0) && (x.Key <= transitionNames.Count)), "all transitions must be identifiable");
            Contract.Requires(inArcs.All(x => (x.Key >= 0) && (x.Key <= transitionNames.Count)), "all in arcs must refer to known transitions");
            Contract.Requires(outArcs.All(x => (x.Key >= 0) && (x.Key <= transitionNames.Count)), "all out arcs must refer to known transitions");
            */
            Contract.Ensures(Transitions.Count == transitionNames.Count);
            Contract.Ensures(Places.Count == placeNames.Count);

            Id = id;
            InMatrix = new SparseMatrix(placeNames.Count, transitionNames.Count);
            OutMatrix = new SparseMatrix(placeNames.Count, transitionNames.Count);
            Places = placeNames;
            Transitions = transitionNames;
            inArcs.Foreach(x => x.Value.Foreach(y => InMatrix[y.Source, x.Key] = y.Weight));
            outArcs.Foreach(x => x.Value.Foreach(y => OutMatrix[y.Target, x.Key] = y.Weight));
        }

        #endregion

        #region graph model and state data
        public string Id { get; set; }

        /*
         * A note on how the matrices are constructed
         * 
         * Rows are for places and Columns are for transitions
         * therefore M[i,j] is the adjacency of place i to transition j.
         * M.GetRow(i) gets the adjacencies for place i and
         * M.GetColumn(j) gets the adjacencies for transition j.
         */
        public SparseMatrix InMatrix { get; set; }
        public SparseMatrix OutMatrix { get; set; }
        public SparseMatrix FlowMatrix { get; set; }
        public Dictionary<int, int> TransitionPriorities = new Dictionary<int, int>();
        public Dictionary<int, string> Places = new Dictionary<int, string>();
        public Dictionary<int, string> Transitions = new Dictionary<int, string>();
        public Dictionary<int, List<Action<int>>> TransitionFunctions = new Dictionary<int, List<Action<int>>>();
        #endregion

        #region graph construction
        public void AddArcFromTransition(int placeId, int transitionId)
        {
            Contract.Requires(Places.ContainsKey(placeId));
            Contract.Requires(Transitions.ContainsKey(transitionId));
            Contract.Ensures(OutMatrix[placeId, transitionId] == Contract.OldValue(OutMatrix[placeId, transitionId]) + 1);

            OutMatrix[placeId, transitionId]++;
        }

        public void AddArcIntoTransition(int placeId, int transitionId)
        {
            Contract.Requires(Places.ContainsKey(placeId));
            Contract.Requires(Transitions.ContainsKey(transitionId));
            Contract.Ensures(InMatrix[placeId, transitionId] == Contract.OldValue(InMatrix[placeId, transitionId]) + 1);

            InMatrix[placeId, transitionId]++;
        }

        #endregion

        #region accessors
        public override int GetWeight(int placeid, int transid)
        {
            return (int)InMatrix[placeid, transid];
        }

        //[Pure]
        internal bool ArcIsInhibitor(int placeId, int transitionId)
        {
            Contract.Requires(Places.ContainsKey(placeId));
            Contract.Requires(Transitions.ContainsKey(transitionId));

            return InMatrix[placeId, transitionId] == double.NaN;
        }

        //[Pure]
        public override IEnumerable<int> InhibitorsIntoTransition(int transitionId)
        {
            Contract.Requires(Transitions.ContainsKey(transitionId));

            for (int i = 0; i < InMatrix.GetColumn(transitionId).Count; i++)
            {
                if (ArcIsInhibitor(i, transitionId))
                    yield return i;
            }
        }

        //[Pure]
        public override IEnumerable<int> NonInhibitorsIntoTransition(int transitionId)
        {
            Contract.Requires(Transitions.ContainsKey(transitionId));

            for (int i = 0; i < InMatrix.GetColumn(transitionId).Count; i++)
            {
                if (InMatrix[i, transitionId] != 0.0 && !ArcIsInhibitor(i, transitionId))
                    yield return i;
            }
        }
        #endregion

        #region extensibility
        public void RegisterFunction(int transitionId, Action<int> fn)
        {
            Contract.Requires(Transitions.ContainsKey(transitionId));
            Contract.Requires(fn != null);
            Contract.Ensures(TransitionFunctions.ContainsKey(transitionId));
            Contract.Ensures(TransitionFunctions[transitionId].Contains(fn));

            if (!TransitionFunctions.ContainsKey(transitionId))
            {
                TransitionFunctions[transitionId] = new List<Action<int>>();
            }
            TransitionFunctions[transitionId].Add(fn);
        }
        #endregion

        #region net execution
        bool AllInhibitorsAreFromEmptyPlaces(int transitionId, Marking m)
        {
            Contract.Requires(Transitions.ContainsKey(transitionId));

            return InhibitorsIntoTransition(transitionId).All(ia => m[ia] == 0);
        }

        bool AllInArcPlacesHaveMoreTokensThanTheArcWeight(int transitionId, Marking m)
        {
            Contract.Requires(Transitions.ContainsKey(transitionId));

            var arcs = NonInhibitorsIntoTransition(transitionId);
            var result = arcs.All(ia => m[ia] >= InMatrix[ia, transitionId]);
            return result;
        }

        public override bool IsEmptyTransition(int transitionId)
        {
            Contract.Requires(Transitions.ContainsKey(transitionId));

            return InMatrix.GetColumn(transitionId).Sum() == 0;
        }

        public IEnumerable<int> GetEnabledTransitions(Marking m)
        {
            // subtract the weights from the markings then sum each element in the resulting vectir
            // if the result is greater than or equal to zero then the transition is enabled
            for (int i = 0; i < OutMatrix.Columns; i++)
            {
                if (IsEnabled(i,m))
                    yield return i;
            }
        }

        SparseVector CreateFiringPlan(Marking m)
        {
            SparseVector result = new SparseVector(Transitions.Count);
            if (IsConflicted(m))
            {
                var next = GetNextTransitionToFire(m);
                if (next.HasValue)
                    result[next.Value] = 1.0;
            }
            else
            {
                foreach (var transId in GetEnabledTransitions(m))
                {
                    result[transId] = 1;
                }
            }
            return result;
        }

        public int? GetNextTransitionToFire(Marking m)
        {
            var ets = GetEnabledTransitions(m);
            return (from t in ets
                    orderby GetTransitionPriority(t) descending
                    select t).FirstOrDefault();
        }

        public virtual Marking Fire(Marking m)
        {
            var firingTransitions = GetEnabledTransitions(m).ToList(); // no laziness here, since enabled trans will change after the flow equation has been evaluated
            var result = new Marking(m + (OutMatrix - InMatrix) * CreateFiringPlan(m));

            foreach (var transId in firingTransitions)
            {
                if (TransitionFunctions.ContainsKey(transId))
                    TransitionFunctions[transId].ForEach(a => a(transId));
            }
            return result;
        }
        #endregion
        [ContractInvariantMethod]
        protected void ObjectInvariant()
        {
            Contract.Invariant(Places != null, "Places can never be null");
            Contract.Invariant(Transitions != null, "Transitions must never be null");
            Contract.Invariant(!string.IsNullOrEmpty(Id), "the petri net must have a valid ID");
            Contract.Invariant(InMatrix != null, "the in matrix must be a valid matrix");
            Contract.Invariant(InMatrix.Columns == Transitions.Count, "in matrix is the wrong width");
            Contract.Invariant(InMatrix.Rows == Places.Count, "in matrix is the wrong height");
            Contract.Invariant(OutMatrix != null, "the out matrix must be a valid matrix");
            Contract.Invariant(OutMatrix.Columns == Transitions.Count, "out matrix is the wrong width");
            Contract.Invariant(OutMatrix.Rows == Places.Count, "out matrix is the wrong height");
        }

        public int GetTransitionPriority(int t)
        {
            return TransitionPriorities.ContainsKey(t) ? TransitionPriorities[t] : 0;
        }
        public override IEnumerable<int> GetPlaceOutArcs(int placeId)
        {
            foreach (var item in InMatrix.GetRow(placeId).GetIndexedEnumerator())
            {
                if (item.Value > 0)
                    yield return item.Key;
            }
        }

        public override IEnumerable<int> AllPlaces()
        {
            return Places.Count() > 0 ? Places.Keys.AsEnumerable() : new int[] { };
        }

        public IEnumerable<ConflictSet> GetConflictingTransitions()
        {
            throw new NotImplementedException();
        }
    }

    public static class VectorExtensions
    {
        public static double Sum(this SparseVector vector)
        {
            var result = 0.0;
            foreach (var val in vector)
            {
                result += val;
            }
            return result;
        }

    }
}

