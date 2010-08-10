using System;
using System.Collections.Generic;
using System.Linq;

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
        }

        public string TransitionName { get; set; }
        public string[] Args { get; set; }
        protected bool IsIntoTransition { get; set; } // i.e. are they InArcs, or OutArcs
        protected bool CreateInhibitors { get; set; }

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
                    Builder.AddInArc(placeName:arg, transitionName:TransitionName, isInhibitor:CreateInhibitors);
                }
                else
                {
                    Builder.AddOutArc(TransitionName, arg);
                }
            }
            return Builder;
        }
    }
    public class PetriNetEventBuilder
    {
        private CreatePetriNet _builder;
        private string _transitionName;
        List<Action<GraphPetriNet>> tasks = new List<Action<GraphPetriNet>>();

        public PetriNetEventBuilder(CreatePetriNet createPetriNet, string transitionName)
        {
            this._builder = createPetriNet;
            this._transitionName = transitionName;
        }

        public PetriNetEventBuilder Run(Action<GraphPetriNet> f)
        {
            tasks.Add(f);
            return this;
        }

        public CreatePetriNet And()
        {
            return Complete();
        }

        public CreatePetriNet Complete()
        {
            foreach (var task in tasks)
            {
                _builder.AddEvent(_transitionName,
                                  task);
            }
            return _builder;
        }
    }
    public class CreatePetriNet
    {
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
            if (Places != null)
            {
                throw new ApplicationException("Places has already been initialised");
            }
            if (placeNames == null)
            {
                throw new ArgumentException("placeNames");
            }

            if (placeNames.Count() == 0)
            {
                throw new ArgumentException("placeNames");
            }

            if (placeNames.Any(s1 => string.IsNullOrWhiteSpace(s1)))
            {
                throw new ArgumentException("placeNames");
            }

            if (!placeNames.All(s1 => s1.All(Char.IsLetterOrDigit)))
            {
                throw new ArgumentException("placeNames");
            }

            Places = placeNames.Select((s,
                                        i) => Tuple.Create(i,
                                                           s)).ToDictionary(tuple => tuple.Item1,
                                                                            tuple1 => tuple1.Item2);
            return this;
        }
        public CreatePetriNet AndPlaces(params string[] placeNames) { return WithPlaces(placeNames); }
        public CreatePetriNet WithTransitions(params string[] transitionNames)
        {
            if (Transitions != null)
            {
                throw new ApplicationException("Places has already been initialised");
            }
            if (transitionNames == null)
            {
                throw new ArgumentException("placeNames");
            }

            if (transitionNames.Count() == 0)
            {
                throw new ArgumentException("placeNames");
            }

            if (transitionNames.Any(s1 => string.IsNullOrWhiteSpace(s1)))
            {
                throw new ArgumentException("placeNames");
            }

            if (!transitionNames.All(s1 => s1.All(Char.IsLetterOrDigit)))
            {
                throw new ArgumentException("placeNames");
            }

            Transitions = transitionNames.Select((s,
                                        i) => Tuple.Create(i,
                                                           s)).ToDictionary(tuple => tuple.Item1,
                                                                            tuple1 => tuple1.Item2);
            return this;
        }
        public CreatePetriNet AndTransitions(params string[] transitionNames) { return WithTransitions(transitionNames); }
        public PetriNetConnectionBuilder With(string transitionName)
        {
            var result = new PetriNetConnectionBuilder(this, transitionName);
            return result;
        }

        public GraphPetriNet CreateNet()
        {
            return new GraphPetriNet(
                Name,
                Places,
                Transitions, 
                InArcs,
                OutArcs);
            throw new NotImplementedException();
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
        int TransitionIndex(string name)
        {
            return Transitions.Where(pair => pair.Value == name).Select(valuePair => valuePair.Key).First();
        }
        int PlaceIndex(string name)
        {
            return Places.Where(pair => pair.Value == name).Select(valuePair => valuePair.Key).First();
        }
        public void AddInArc(string placeName,
                             string transitionName,
                             bool isInhibitor)
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
            InArcs[fromIndex].Add(new InArc(toIndex, isInhibitor));
        }

        public void AddOutArc(string transitionName, string placeName)
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
            OutArcs[fromIndex].Add(new OutArc(toIndex));
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