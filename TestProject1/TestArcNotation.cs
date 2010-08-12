using System.Collections;
using System.Linq;
using NUnit.Framework;
using PetriNetCore;

namespace TestProject1
{
    [TestFixture]
    public class TestArcNotation
    {
        private void RunTestCase(string spec,
                                 int weight,
                                 bool isInhibitor)
        {
            CreatePetriNet pnb = CreatePetriNet.Called("pn");
            pnb.WithArcs(spec);
            Assert.IsTrue(pnb.Places.ContainsValue("p1"));
            Assert.IsTrue(pnb.Transitions.ContainsValue("t1"));
            int tIdx = pnb.TransitionIndex("t1");
            int pIdx = pnb.PlaceIndex("p1");
            if (spec.Contains("]")) // if is OutArc
            {
                Assert.IsTrue(pnb.OutArcs.ContainsKey(tIdx));
                Assert.IsTrue(pnb.OutArcs[tIdx].Any(arc => arc.Target.Equals(pIdx)));
                Assert.AreEqual(weight,
                                pnb.OutArcs[tIdx].Where(outArc => outArc.Target.Equals(pIdx)).Single().Weight);
            }
            else
            {
                Assert.IsTrue(pnb.InArcs.ContainsKey(tIdx));
                Assert.IsTrue(pnb.InArcs[tIdx].Any(arc => arc.Source.Equals(pIdx)));
                Assert.AreEqual(weight,
                                pnb.InArcs[tIdx].Where(outArc => outArc.Source.Equals(pIdx)).Single().Weight);
                Assert.AreEqual(isInhibitor,
                                pnb.InArcs[tIdx].Where(outArc => outArc.Source.Equals(pIdx)).Single().IsInhibitor);
            }
        }

        [Test, TestCaseSource(typeof (TestDataGenerator), "GoodData"),
         TestCaseSource(typeof (TestDataGenerator), "BadData")]
        public void AllTests(string spec,
                             int weight,
                             bool isInhibitor)
        {
            RunTestCase(spec,
                        weight,
                        isInhibitor);
        }
    }

    public static class TestDataGenerator
    {
        private const string contractException = "System.Diagnostics.Contracts.__ContractsRuntime+ContractException";

        public static IEnumerable GoodData
        {
            get
            {
                yield return Good("p1)[t1", 1, false).SetName("G.1"); // (spec, weight, isInhibitor
                yield return Good("p1)-[t1", 1, false).SetName("G.2"); // (spec, weight, isInhibitor
                yield return Good("p1)--[t1", 1, false).SetName("G.3"); // (spec, weight, isInhibitor
                yield return Good("p1)---------------------[t1", 1, false).SetName("G.4"); // (spec, weight, isInhibitor
                yield return Good("p1)o[t1", 1, true).SetName("G.5");
                yield return Good("p1)-o[t1", 1, true).SetName("G.6");
                yield return Good("p1)-o-[t1", 1, true).SetName("G.7");
                yield return Good("p1)----o----[t1", 1, true).SetName("G.8");

                yield return Good("p1)2[t1", 2, false).SetName("G.9");
                yield return Good("p1)2-[t1", 2, false).SetName("G.10");
                yield return Good("p1)-2[t1", 2, false).SetName("G.11");
                yield return Good("p1)--2--[t1", 2, false).SetName("G.12");
                yield return Good("p1)------------------2--[t1", 2, false).SetName("G.13");
                yield return Good("p1)2------------------[t1", 2, false).SetName("G.14");

                yield return Good("p1)2o[t1", 2, true).SetName("G.15");
                yield return Good("p1)-2o[t1", 2, true).SetName("G.16");
                yield return Good("p1)2-o[t1", 2, true).SetName("G.17");
                yield return Good("p1)2o-[t1", 2, true).SetName("G.18");
                yield return Good("p1)-2-o[t1", 2, true).SetName("G.19");
                yield return Good("p1)-2o-[t1", 2, true).SetName("G.20");
                yield return Good("p1)-2-o-[t1", 2, true).SetName("G.21");

                yield return Good("p1)-2o[t1", 2, true).SetName("G.22");
                yield return Good("p1)2--o[t1", 2, true).SetName("G.23");
                yield return Good("p1)2o--[t1", 2, true).SetName("G.24");
                yield return Good("p1)--2--o[t1", 2, true).SetName("G.25");
                yield return Good("p1)--2o--[t1", 2, true).SetName("G.26");
                yield return Good("p1)--2--o--[t1", 2, true).SetName("G.27");

                yield return Good("t1](p1", 1, false).SetName("G.28");
                yield return Good("t1]-(p1", 1, false).SetName("G.29");
                yield return Good("t1]------(p1", 1, false).SetName("G.30");

                yield return Good("t1]2(p1", 2, false).SetName("G.31");
                yield return Good("t1]-2(p1", 2, false).SetName("G.32");
                yield return Good("t1]2-(p1", 2, false).SetName("G.33");
                yield return Good("t1]-2-(p1", 2, false).SetName("G.34");
                yield return Good("t1]--2(p1", 2, false).SetName("G.35");
                yield return Good("t1]2--(p1", 2, false).SetName("G.36");
                yield return Good("t1]--2--(p1", 2, false).SetName("G.37");
            }
        }

        public static IEnumerable BadData
        {
            get
            {
                #region duplicate names

                yield return Bad("p1)[p1", 1, false).SetName("B.1");
                yield return Bad("p1)-[p1", 1, false).SetName("B.2");
                yield return Bad("p1)--[p1", 1, false).SetName("B.3");
                yield return Bad("p1)-2-[p1", 1, false).SetName("B.4");
                yield return Bad("p1)-2-o-[p1", 1, false).SetName("B.5");
                yield return Bad("t1]-(t1", 1, false).SetName("B.6");

                #endregion

                #region duplicate types

                yield return Bad("t1)(p1", 1, false).SetName("B.7");
                yield return Bad("t1)-(p1", 1, false).SetName("B.8");
                yield return Bad("t1)--(p1", 1, false).SetName("B.9");
                yield return Bad("t1)-2-(p1", 1, false).SetName("B.10");
                yield return Bad("t1)-2-o-(p1", 1, false).SetName("B.11");

                yield return Bad("t1][p1", 1, false).SetName("B.12");
                yield return Bad("t1]-[p1", 1, false).SetName("B.13");
                yield return Bad("t1]--[p1", 1, false).SetName("B.14");
                yield return Bad("t1]-2-[p1", 1, false).SetName("B.15");
                yield return Bad("t1]-2-o-[p1", 1, false).SetName("B.16");

                #endregion

                #region missing origin

                yield return Bad(")[t1",1,false).SetName("B.17");
                yield return Bad(")-[t1",1,false).SetName("B.18");
                yield return Bad(")--[t1",1,false).SetName("B.19");
                yield return Bad(")---------------------[t1",1,false).SetName("B.20");
                yield return Bad(")o[t1",1,true).SetName("B.21");
                yield return Bad(")-o[t1",1,true).SetName("B.22");
                yield return Bad(")-o-[t1",1,true).SetName("B.23");
                yield return Bad(")----o----[t1",1,true).SetName("B.24");
                yield return Bad(")2[t1",2,false).SetName("B.25");
                yield return Bad(")2-[t1",2,false).SetName("B.26");
                yield return Bad(")-2[t1",2,false).SetName("B.27");
                yield return Bad(")--2--[t1",2,false).SetName("B.28");
                yield return Bad(")------------------2--[t1",2,false).SetName("B.29");
                yield return Bad(")2------------------[t1",2,false).SetName("B.30");
                yield return Bad(")2o[t1",2,true).SetName("B.31");
                yield return Bad(")-2o[t1", 2, true).SetName("B.32");
                yield return Bad(")2-o[t1", 2, true).SetName("B.33");
                yield return Bad(")2o-[t1", 2, true).SetName("B.34");
                yield return Bad(")-2-o[t1", 2, true).SetName("B.35");
                yield return Bad(")-2o-[t1", 2, true).SetName("B.36");
                yield return Bad(")-2-o-[t1", 2, true).SetName("B.37");

                yield return Bad(")-2o[t1", 2, true).SetName("B.38");
                yield return Bad(")2--o[t1", 2, true).SetName("B.39");
                yield return Bad(")2o--[t1", 2, true).SetName("B.40");
                yield return Bad(")--2--o[t1", 2, true).SetName("B.41");
                yield return Bad(")--2o--[t1", 2, true).SetName("B.42");
                yield return Bad(")--2--o--[t1", 2, true).SetName("B.43");

                yield return Bad("](p1", 1, false).SetName("B.44");
                yield return Bad("]-(p1", 1, false).SetName("B.45");
                yield return Bad("]------(p1", 1, false).SetName("B.46");

                yield return Bad("]2(p1", 2, false).SetName("B.47");
                yield return Bad("]-2(p1", 2, false).SetName("B.48");
                yield return Bad("]2-(p1", 2, false).SetName("B.49");
                yield return Bad("]-2-(p1", 2, false).SetName("B.50");
                yield return Bad("]--2(p1", 2, false).SetName("B.51");
                yield return Bad("]2--(p1", 2, false).SetName("B.52");
                yield return Bad("]--2--(p1", 2, false).SetName("B.53");

                #endregion

                #region missing destination

                yield return Bad("p1)[", 1, false).SetName("B.54");
                yield return Bad("p1)-[", 1, false).SetName("B.55");
                yield return Bad("p1)--[", 1, false).SetName("B.56");
                yield return Bad("p1)---------------------[", 1, false).SetName("B.57");
                yield return Bad("p1)o[", 1, true).SetName("B.58");
                yield return Bad("p1)-o[", 1, true).SetName("B.59");
                yield return Bad("p1)-o-[", 1, true).SetName("B.60");
                yield return Bad("p1)----o----[", 1, true).SetName("B.61");

                yield return Bad("p1)2[", 2, false).SetName("B.62");
                yield return Bad("p1)2-[", 2, false).SetName("B.63");
                yield return Bad("p1)-2[", 2, false).SetName("B.64");
                yield return Bad("p1)--2--[", 2, false).SetName("B.65");
                yield return Bad("p1)------------------2--[", 2, false).SetName("B.66");
                yield return Bad("p1)2------------------[", 2, false).SetName("B.67");

                yield return Bad("p1)2o[", 2, true).SetName("B.68");
                yield return Bad("p1)-2o[", 2, true).SetName("B.69");
                yield return Bad("p1)2-o[", 2, true).SetName("B.70");
                yield return Bad("p1)2o-[", 2, true).SetName("B.71");
                yield return Bad("p1)-2-o[", 2, true).SetName("B.72");
                yield return Bad("p1)-2o-[", 2, true).SetName("B.73");
                yield return Bad("p1)-2-o-[", 2, true).SetName("B.74");

                yield return Bad("p1)-2o[", 2, true).SetName("B.75");
                yield return Bad("p1)2--o[", 2, true).SetName("B.76");
                yield return Bad("p1)2o--[", 2, true).SetName("B.77");
                yield return Bad("p1)--2--o[", 2, true).SetName("B.78");
                yield return Bad("p1)--2o--[", 2, true).SetName("B.79");
                yield return Bad("p1)--2--o--[", 2, true).SetName("B.80");

                yield return Bad("t1](", 1, false).SetName("B.81");
                yield return Bad("t1]-(", 1, false).SetName("B.82");
                yield return Bad("t1]------(", 1, false).SetName("B.83");

                yield return Bad("t1]2(", 2, false).SetName("B.84");
                yield return Bad("t1]-2(", 2, false).SetName("B.85");
                yield return Bad("t1]2-(", 2, false).SetName("B.86");
                yield return Bad("t1]-2-(", 2, false).SetName("B.87");
                yield return Bad("t1]--2(", 2, false).SetName("B.88");
                yield return Bad("t1]2--(", 2, false).SetName("B.89");
                yield return Bad("t1]--2--(", 2, false).SetName("B.90");

                #endregion
            }
        }

        private static TestCaseData Good(params object[] args)
        {
            return new TestCaseData(args);
        }

        private static TestCaseData Bad(params object[] args)
        {
            return new TestCaseData(args).Throws(contractException);
        }
    }
}
