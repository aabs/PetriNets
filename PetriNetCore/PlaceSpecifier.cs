using System.Diagnostics.Contracts;

namespace PetriNetCore
{
    public class PlaceSpecifier
    {
        private readonly CreatePetriNet _createPetriNet;
        private readonly string _placeName;
        private int _capacity = int.MaxValue;
        private int _tokens;

        public PlaceSpecifier(CreatePetriNet createPetriNet, string placeName)
        {
            Contract.Requires(createPetriNet != null);
            Contract.Requires(createPetriNet.PlaceCapacities != null);
            Contract.Requires(createPetriNet.PlaceMarkings != null);
            _createPetriNet = createPetriNet;
            _placeName = placeName;
        }

        public PlaceSpecifier HavingMarking(int tokens)
        {
            Contract.Requires(tokens >= 0);
            _tokens = tokens;
            return this;
        }

        public PlaceSpecifier HavingCapacity(int capacity)
        {
            Contract.Requires(capacity >= 0);
            _capacity = capacity;
            return this;
        }

        public CreatePetriNet Done()
        {
            return And();
        }

        public CreatePetriNet And()
        {
            _createPetriNet.PlaceMarkings[_placeName] = _tokens;
            _createPetriNet.PlaceCapacities[_placeName] = _capacity;
            return _createPetriNet;
        }
    }
}