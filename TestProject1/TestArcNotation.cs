using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PetriNetCore;

namespace TestProject1
{
    [TestFixture]
    public class TestArcNotation
    {

        [Test, Category("Regression")]
        [TestCase("p1)-[t1", 1, false)] // (spec, weight, isInhibitor
        [TestCase("t1]-(p1", 1, false)]
        [TestCase("p1)-o[t1", 1, true)]
        [TestCase("t1]-2-(p1", 2, false)]
        [TestCase("p1)-2-[t1", 2, false)]
        [TestCase("p1)-2-o[t1", 2, true)]
        public void TestCreateSingleArc(string spec, int weight, bool isInhibitor)
        {
            CreatePetriNet pnb = CreatePetriNet.Called("pn");
            pnb.WithArcs(spec);
            Assert.IsTrue(pnb.Places.ContainsValue("p1"));
            Assert.IsTrue(pnb.Transitions.ContainsValue("t1"));
            var tIdx = pnb.TransitionIndex("t1");
            var pIdx = pnb.PlaceIndex("p1");
            if (spec.Contains("]")) // if is OutArc
            {
                Assert.IsTrue(pnb.OutArcs.ContainsKey(tIdx));
                Assert.IsTrue(pnb.OutArcs[tIdx].Any(arc => arc.Target.Equals(pIdx)));
                Assert.AreEqual(weight, pnb.OutArcs[tIdx].Where(outArc => outArc.Target.Equals(pIdx)).Single().Weight);
            }
            else
            {
                Assert.IsTrue(pnb.InArcs.ContainsKey(tIdx));
                Assert.IsTrue(pnb.InArcs[tIdx].Any(arc => arc.Source.Equals(pIdx)));
                Assert.AreEqual(weight, pnb.InArcs[tIdx].Where(outArc => outArc.Source.Equals(pIdx)).Single().Weight);
                Assert.AreEqual(isInhibitor, pnb.InArcs[tIdx].Where(outArc => outArc.Source.Equals(pIdx)).Single().IsInhibitor);
            }
        }
    }
}
