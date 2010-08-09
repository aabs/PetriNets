using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using dnAnalytics.LinearAlgebra;

namespace PetriNetCore
{
    public class Marking : SparseVector
    {
        public Marking(int size) : base(size)
        {
            if(size > GraphPetriNet.MaxSize) 
                throw new ArgumentException("Marking size exceeds MaxSize of " + GraphPetriNet.MaxSize);
        }

        public Marking(SparseVector vec) : base(vec)
        {
            Debug.Assert(vec.Count <= GraphPetriNet.MaxSize);
        }

        public Marking(int size,
                       IDictionary<int, int> markings) : base(size)
        {
            Debug.Assert(size <= GraphPetriNet.MaxSize);
            Debug.Assert(markings.Keys.Max() < size);
            Debug.Assert(markings.Keys.Min() >= 0);
            markings.Foreach(x => { this[x.Key] = x.Value; });
        }

        public Marking(Marking m) : base(m)
        {
        }
    }
}