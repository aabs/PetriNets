using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PetriNetCore;

namespace TestProject1
{
    [TestClass]
    public class TestClass2
    {

        [TestMethod]
        public void TestTest1()
        {
            var p = CreatePNTwoInOneOut();
            var m = new Marking(3, new Dictionary<int, int>{ 
                                                               { 0, 1 }, 
                                                               { 1, 1 },
                                                               { 2, 0 } });
            AssertMarkings(m, new Dictionary<int, double>{ 
                                                             { 0, 1 }, 
                                                             { 1, 1 },
                                                             { 2, 0 } });
            var m2 = p.Fire(m);
            AssertMarkings(m2, new Dictionary<int, double>{ 
                                                              { 0, 0 }, 
                                                              { 1, 0 },
                                                              { 2, 1 } });

        }

        private static Marking CreateMarking2()
        {
            return new Marking(2, new Dictionary<int, int> { { 0, 1 }, { 1, 1 } });
        }

        public static GraphPetriNet CreatePNTwoInOneOut()
        {
            var p = new GraphPetriNet(
                "p",
                new Dictionary<int, string> {
                                                {0, "p0"},
                                                {1, "p1"},
                                                {2, "p2"}
                                            },
                //                new Dictionary<int, int> { { 0, 1 }, { 1, 1 }, { 2, 0 } },
                new Dictionary<int, string> { { 0, "t0" } },
                new Dictionary<int, List<InArc>>(){
                                                      {0, new List<InArc>(){new InArc(0),new InArc(1)}}
                                                  },
                new Dictionary<int, List<OutArc>>(){
                                                       {0, new List<OutArc>(){new OutArc(2)}}
                                                   }
                );
            return p;
        }

        public void AssertMarkings<T1, T2>(Marking p, Dictionary<T1, T2> markingsExpected)
        {
            foreach (var marking in markingsExpected)
            {
                Assert.AreEqual(marking.Value, p[Convert.ToInt32(marking.Key)]);
            }
        }


    }
}