using System;
using System.Collections.Generic;

namespace PetriNetCore
{
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
}