using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PetriNetCore;

namespace Tests
{
    [TestClass]
    public class Test3 : BasePNTester
    {
        [TestMethod]
        public void TestSolution3()
        {
            var pn = CreateTestNet();
            AssertMarkings(pn, new Dictionary<int, int> { { 0, 1 }, { 1, 0 }, { 2, 1 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 } });
            pn.Fire();
            AssertMarkings(pn, new Dictionary<int, int> { { 0, 1 }, { 1, 0 }, { 2, 1 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 } });
            pn.SetMarking(4, 1);
            pn.Fire();
            AssertMarkings(pn, new Dictionary<int, int> { { 0, 1 }, { 1, 0 }, { 2, 1 }, { 3, 0 }, { 4, 0 }, { 5, 1 }, { 6, 1 } });
        }

        public MatrixPetriNet CreateTestNet()
        {
            var pn = new MatrixPetriNet("p",
                new Dictionary<int, string> { { 0, "s1" }, { 1, "s2" }, { 2, "s3" }, { 3, "s4" }, { 4, "pi" }, { 5, "pi1" }, { 6, "pi2" } },
                new Dictionary<int, int> { { 0, 1 }, { 1, 0 }, { 2, 1 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 } },
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
