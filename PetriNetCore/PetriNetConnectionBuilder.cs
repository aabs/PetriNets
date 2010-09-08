namespace PetriNetCore
{
    public class PetriNetConnectionBuilder
    {
        public CreatePetriNet Builder { get; set; }

        public PetriNetConnectionBuilder(CreatePetriNet builder,
                                         string transition)
        {
            Builder = builder;
            TransitionName = transition;
            _weight = 1;
        }

        public string TransitionName { get; set; }
        public string[] Args { get; set; }
        protected bool IsIntoTransition { get; set; } // i.e. are they InArcs, or OutArcs
        protected bool CreateInhibitors { get; set; }
        protected int _weight { get; set; }

        public PetriNetConnectionBuilder FedBy(params string[] placeNames)
        {
            Args = placeNames;
            IsIntoTransition = true;
            return this;
        }

        public PetriNetConnectionBuilder Feeding(params string[] placeNames)
        {
            Args = placeNames;
            IsIntoTransition = false;
            return this;
        }

        public PetriNetConnectionBuilder AsInhibitor()
        {
            CreateInhibitors = true;
            return this;
        }

        public CreatePetriNet Done()
        {
            return And();
        }

        public CreatePetriNet And()
        {
            foreach (var arg in Args)
            {
                if (IsIntoTransition)
                {
                    Builder.AddInArc(arg, TransitionName, CreateInhibitors, _weight);
                }
                else
                {
                    Builder.AddOutArc(TransitionName, arg, _weight);
                }
            }
            return Builder;
        }

        internal PetriNetConnectionBuilder Weight(int weight)
        {
            _weight = weight;
            return this;
        }
    }
}