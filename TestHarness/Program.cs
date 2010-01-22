using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetriNetCore;

namespace TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new OrdinaryPetriNet();
            p.SetMarking(0, 2);

        }
    }
}
