using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PetriNetCore;

namespace Tests
{
    [TestFixture]
    public class Test3 : BasePNTester
    {
        [Test]
        public void TestSolution3()
        {
            var pn = CreateTestNet();
            var m = new Marking(pn.AllPlaces().Count(),
                new Dictionary<int, int> { { 0, 1 }, { 1, 0 }, { 2, 1 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 } });
            AssertMarkings(m, new Dictionary<int, int> { { 0, 1 }, { 1, 0 }, { 2, 1 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 } });
            var m2 = pn.Fire(m);
            AssertMarkings(m2, new Dictionary<int, int> { { 0, 1 }, { 1, 0 }, { 2, 1 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 } });
            m2[4] = 1;
            var m3 = pn.Fire(m2);
            AssertMarkings(m3, new Dictionary<int, int> { { 0, 1 }, { 1, 0 }, { 2, 1 }, { 3, 0 }, { 4, 0 }, { 5, 1 }, { 6, 1 } });
        }

        public static Marking CreateMarking7()
        {
            return new Marking(7, new Dictionary<int, int> { { 0, 1 }, { 1, 0 }, { 2, 1 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 } });
        }
        public GraphPetriNet CreateTestNet()
        {
            var pn = new GraphPetriNet("p",
                new Dictionary<int, string> { { 0, "s1" }, { 1, "s2" }, { 2, "s3" }, { 3, "s4" }, { 4, "pi" }, { 5, "pi1" }, { 6, "pi2" } },
                new Dictionary<int, string> { { 0, "t1" }, { 1, "t2" }, { 2, "ti" }, { 3, "te1" }, { 4, "te2" } },
                new Dictionary<int, List<InArc>>
                {
                    {0, new []{0,5}.ToInAdj()}, 
                    {1, new []{2,6}.ToInAdj()}, 
                    {2, new []{4}.ToInAdj()}, 
                    {3, new []{5}.ToInAdj()}, 
                    {4, new []{6}.ToInAdj()} 
                },
                new Dictionary<int, List<OutArc>>
                {
                    {0, new []{1}.ToOutAdj()}, 
                    {1, new []{3}.ToOutAdj()}, 
                    {2, new []{5,6}.ToOutAdj()}, 
                    {3, new List<OutArc>{}}, 
                    {4, new List<OutArc>{}} 
                });
            return pn;
        }
    }

    public static class ArrayExtensions
    {
        public static List<InArc> ToInAdj(this int[] places)
        {
            return places.Select(i => new InArc(i)).ToList();
        }
        public static List<OutArc> ToOutAdj(this int[] places)
        {
            return places.Select(i => new OutArc(i)).ToList();
        }
    }
}
