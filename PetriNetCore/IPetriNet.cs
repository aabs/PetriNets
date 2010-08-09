using System.Collections.Generic;

namespace PetriNetCore
{
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
}