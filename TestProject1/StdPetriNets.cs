using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestProject1
{
    public static class StdPetriNets
    {
        public const string TwoInOneOut = @"PetriNet mynet: {p1,p2})-[t1; t1]-(p3; ";
        public const string OneInOneOut = @"PetriNet mynet: p1)-[t1; t1]-(p2; ";


        /*
         * This model is a petri net in this shape
         * 
         * P1 --> T1 --> P2 --> T2 --> P3
         *                ^
         *                |
         *                T3
         *                ^
         *                |
         *                P4
         * */
        public const string MultiEnabled = @"
PetriNet mynet: 
    p1)-[t1; 
    {t1, t3}]-(p2; 
    p4)-[t3;
    p2)-[t2;
    t2]-(p3;
";
        public static readonly string Bifurcation = @"
PetriNet Bifurcating: 
    p1)-[t1; 
    t1]-({p2, p3}; 
";
        public static readonly string SelfTransition = @"
PetriNet SelfTransition: 
    p1)-[t1; 
    t1]-(p1; 
";
        public static readonly string DoubleSelfTransition = @"
PetriNet DoubleSelfTransition: 
    p1)-[{t1, t2}; 
    {t1, t2}]-(p1; 
";
    }
}
