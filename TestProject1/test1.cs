/*
 * Created by SharpDevelop.
 * User: Andrew Matthews
 * Date: 3/08/2009
 * Time: 7:41 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using PetriNetCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class TestClass2
    {
    	/*
    	 * Test scenarios
    	 * 1. one to one
    	 * 2. multiple to one
    	 * 3. one to multiple
    	 * 4. multi to multi
    	 * 5. inhibitor to one
    	 * 6. inhibitor to multi
    	 * 7. multi inhibit to one
    	 * 8. multi plus inhibit to one
    	 * 9. one weighted to one
    	 * 10. one to one weighted
    	 * 11. loop tran
    	 * 12. mutex
    	 * 13. 
    	 * 14. 
    	 * 15. 
    	 * */
    	
        [TestMethod]
        public void Test1()
        {
            var p = CreatePNTwoInOneOut();
            AssertMarkings(p, new Dictionary<int, int>{ 
                { 0, 1 }, 
                { 1, 1 },
                { 2, 0 } });
            p.Fire();
            AssertMarkings(p, new Dictionary<int, int>{ 
                { 0, 0 }, 
                { 1, 0 },
                { 2, 1 } });
        }

        [TestMethod]
        public void Test2()
        {
            var p = CreatePNTwoInTwoOut();
            AssertMarkings(p, new Dictionary<int, int>{ 
                { 0, 1 }, 
                { 1, 1 },
                { 2, 0 },
                { 3, 0 } });
            p.Fire();
            AssertMarkings(p, new Dictionary<int, int>{ 
                { 0, 0 }, 
                { 1, 0 },
                { 2, 1 },
                { 3, 1 } });
        }

        [TestMethod]
        public void TestInhibition()
        {
            var p = CreatePNInhibited();
            AssertMarkings(p, new Dictionary<int, int>{ 
                { 0, 1 }, 
                { 1, 1 },
                { 2, 0 } });
            p.Fire();
            AssertMarkings(p, new Dictionary<int, int>{ 
                { 0, 1 }, 
                { 1, 1 },
                { 2, 0 } });
        }

        [TestMethod]
        public void TestInhibition2()
        {
            var p = CreatePNInhibited();
            p.SetMarking(1,0);
            AssertMarkings(p, new Dictionary<int, int>{ 
                { 0, 1 }, 
                { 1, 0 },
                { 2, 0 } });
            p.Fire();
            AssertMarkings(p, new Dictionary<int, int>{ 
                { 0, 0 }, 
                { 1, 0 },
                { 2, 1 } });
        }
        
        [TestMethod]
        public void TestOutgoingWeight()
        {
            var p = new PetriNet(
                "p",
                new Dictionary<int, string> {
                    {0, "p0"},
                    {1, "p1"}
                },
                new Dictionary<int, int> { { 0, 1 }, { 1, 0 } },
                new Dictionary<int, string> { { 0, "t0" } },
                new Dictionary<int, List<InArc>>(){
                	{0, new List<InArc>(){new InArc(0)}}
                },
                new Dictionary<int, List<OutArc>>(){
                	{0, new List<OutArc>(){new OutArc(1){Weight=5}}}
                }
              );
            AssertMarkings(p, new Dictionary<int, int>{ 
                { 0, 1 }, 
                { 1, 0 }});        
        	p.Fire();
            AssertMarkings(p, new Dictionary<int, int>{ 
                { 0, 0 }, 
                { 1, 5 }});        
        }
        
        private static PetriNet CreatePNInhibited()
        {
            var p = new PetriNet(
                "p",
                new Dictionary<int, string> {
                    {0, "p0"},
                    {1, "p1"},
                    {2, "p2"}
                },
                new Dictionary<int, int> { { 0, 1 }, { 1, 1 } },
                new Dictionary<int, string> { { 0, "t0" } },
                new Dictionary<int, List<InArc>>(){
                    {0, new List<InArc>(){new InArc(0),new InArc(1, true)}}
                },
                new Dictionary<int, List<OutArc>>(){
                    {0, new List<OutArc>(){new OutArc(2)}}
                }
              );
            return p;
        }

        private static PetriNet CreatePNTwoInTwoOut()
        {
            var p = new PetriNet(
                "p",
                new Dictionary<int, string> {
                    {0, "p0"},
                    {1, "p1"},
                    {2, "p2"},
                    {3, "p3"}
                },
                new Dictionary<int, int> { { 0, 1 }, { 1, 1 } },
                new Dictionary<int, string> { { 0, "t0" } },
                new Dictionary<int, List<InArc>>(){
                    {0, new List<InArc>(){new InArc(0),new InArc(1)}}
                },
                new Dictionary<int, List<OutArc>>(){
                    {0, new List<OutArc>(){new OutArc(2),new OutArc(3)}}
                }
              );
            return p;
        }

        private static PetriNet CreatePNTwoInOneOut()
        {
            var p = new PetriNet(
                "p",
                new Dictionary<int, string> {
                    {0, "p0"},
                    {1, "p1"},
                    {2, "p2"}
                },
                new Dictionary<int, int> { { 0, 1 }, { 1, 1 } },
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
        public void AssertMarkings(PetriNet p, Dictionary<int, int> markingsExpected)
        {
            foreach (var marking in markingsExpected)
            {
                Assert.AreEqual(marking.Value, p.GetMarking(marking.Key));
            }
        }
    }

    public class BasePNTester
    {
        public void AssertMarkings(PetriNet p, Dictionary<int, int> markingsExpected)
        {
            foreach (var marking in markingsExpected)
            {
                Assert.AreEqual(marking.Value, p.GetMarking(marking.Key));
            }
        }

    }
}