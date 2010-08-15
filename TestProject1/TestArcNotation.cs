using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using PetriNetCore;

namespace TestProject1
{
    [TestFixture]
    public class TestArcNotation
    {
        private void RunFullTestCase(string spec,
                                 int inArcs,
                                 int outArcs,
                                 int numInhibitors)
        {
            CreatePetriNet pnb = CreatePetriNet.Parse(spec);
            if (inArcs  == 0)
            {
                Assert.That(pnb.InArcs, Is.Null);
            }
            else
            {
                Assert.That(pnb.InArcs.SelectMany(x => x.Value).Count(), Is.EqualTo(inArcs));
                Assert.That(pnb.InArcs.SelectMany(x => x.Value).Where(x => x.IsInhibitor).Count(), Is.EqualTo(numInhibitors));
            }

            if (outArcs  == 0)
            {
                Assert.That(pnb.OutArcs, Is.Null);
            }
            else
            {
                Assert.That(pnb.OutArcs.SelectMany(x => x.Value).Count(), Is.EqualTo(outArcs));
            }

        }


        [Test]
        [TestCaseSource(typeof (TestDataGenerator), "GoodData")]
        [TestCaseSource(typeof(TestDataGenerator), "BadData")]
        [TestCaseSource(typeof(TestDataGenerator), "FullSpecs")]
        public void AllTests(string spec,
                            int inArcs,
                            int outArcs,
                            int numInhibitors)
        {
            RunFullTestCase(spec,
                            inArcs,
                            outArcs,
                            numInhibitors);
        }
    }

    public static class TestDataGenerator
    {
        private const string contractException = "System.Diagnostics.Contracts.__ContractsRuntime+ContractException";

        public static IEnumerable FullSpecs
        {
            get
            {
                yield return Good("PetriNet mynet: {p1,p2})-[t1; t1]-(p3; p3)-[t2; t2]-({p1,p2}; ", 3, 3, 0).SetName("F.1");
                yield return Good("PetriNet mynet: {p1,p2})-[{t1}; t1]-(p3; p3)-[t2; t2]-({p1,p2}; ", 3, 3,0).SetName("F.2");
            }
        }

        public static IEnumerable GoodData
        {
            get
            {
                yield return Good("PetriNet mynet: p1)[t1; ",1,0,0).SetName("G.1"); // (spec, weight, isInhibitor
                yield return Good("PetriNet mynet: p1)-[t1; ",1,0,0).SetName("G.2"); // (spec, weight, isInhibitor
                yield return Good("PetriNet mynet: p1)--[t1; ",1,0,0).SetName("G.3"); // (spec, weight, isInhibitor
                yield return Good("PetriNet mynet: p1)---------------------[t1; ",1,0,0).SetName("G.4"); // (spec, weight, isInhibitor
                yield return Good("PetriNet mynet: p1)o[t1; ",1,0,1).SetName("G.5");
                yield return Good("PetriNet mynet: p1)-o[t1; ",1,0,1).SetName("G.6");
                yield return Good("PetriNet mynet: p1)-o-[t1; ",1,0,1).SetName("G.7");
                yield return Good("PetriNet mynet: p1)----o----[t1; ",1,0,1).SetName("G.8");

                yield return Good("PetriNet mynet: p1)2[t1; ",1,0,0).SetName("G.9");
                yield return Good("PetriNet mynet: p1)2-[t1; ",1,0,0).SetName("G.10");
                yield return Good("PetriNet mynet: p1)-2[t1; ",1,0,0).SetName("G.11");
                yield return Good("PetriNet mynet: p1)--2--[t1; ",1,0,0).SetName("G.12");
                yield return Good("PetriNet mynet: p1)------------------2--[t1; ",1,0,0).SetName("G.13");
                yield return Good("PetriNet mynet: p1)2------------------[t1; ",1,0,0).SetName("G.14");

                yield return Good("PetriNet mynet: p1)2o[t1; ",1,0,1).SetName("G.15");
                yield return Good("PetriNet mynet: p1)-2o[t1; ",1,0,1).SetName("G.16");
                yield return Good("PetriNet mynet: p1)2-o[t1; ",1,0,1).SetName("G.17");
                yield return Good("PetriNet mynet: p1)2o-[t1; ",1,0,1).SetName("G.18");
                yield return Good("PetriNet mynet: p1)-2-o[t1; ",1,0,1).SetName("G.19");
                yield return Good("PetriNet mynet: p1)-2o-[t1; ",1,0,1).SetName("G.20");
                yield return Good("PetriNet mynet: p1)-2-o-[t1; ",1,0,1).SetName("G.21");

                yield return Good("PetriNet mynet: p1)-2o[t1; ",1,0,1).SetName("G.22");
                yield return Good("PetriNet mynet: p1)2--o[t1; ",1,0,1).SetName("G.23");
                yield return Good("PetriNet mynet: p1)2o--[t1; ",1,0,1).SetName("G.24");
                yield return Good("PetriNet mynet: p1)--2--o[t1; ",1,0,1).SetName("G.25");
                yield return Good("PetriNet mynet: p1)--2o--[t1; ",1,0,1).SetName("G.26");
                yield return Good("PetriNet mynet: p1)--2--o--[t1; ",1,0,1).SetName("G.27");

                yield return Good("PetriNet mynet: t1](p1; ",0,1,0).SetName("G.28");
                yield return Good("PetriNet mynet: t1]-(p1; ",0,1,0).SetName("G.29");
                yield return Good("PetriNet mynet: t1]------(p1; ",0,1,0).SetName("G.30");

                yield return Good("PetriNet mynet: t1]2(p1; ",0,1,0).SetName("G.31");
                yield return Good("PetriNet mynet: t1]-2(p1; ",0,1,0).SetName("G.32");
                yield return Good("PetriNet mynet: t1]2-(p1; ",0,1,0).SetName("G.33");
                yield return Good("PetriNet mynet: t1]-2-(p1; ",0,1,0).SetName("G.34");
                yield return Good("PetriNet mynet: t1]--2(p1; ",0,1,0).SetName("G.35");
                yield return Good("PetriNet mynet: t1]2--(p1; ",0,1,0).SetName("G.36");
                yield return Good("PetriNet mynet: t1]--2--(p1; ",0,1,0).SetName("G.37");
            }
        }

        public static IEnumerable BadData
        {
            get
            {
                #region duplicate names

                yield return Bad("PetriNet mynet: p1)[p1; ",1,1,1).SetName("B.1");
                yield return Bad("PetriNet mynet: p1)-[p1; ",1,1,1).SetName("B.2");
                yield return Bad("PetriNet mynet: p1)--[p1; ",1,1,1).SetName("B.3");
                yield return Bad("PetriNet mynet: p1)-2-[p1; ",1,1,1).SetName("B.4");
                yield return Bad("PetriNet mynet: p1)-2-o-[p1; ",1,1,1).SetName("B.5");
                yield return Bad("PetriNet mynet: t1]-(t1; ",1,1,1).SetName("B.6");

                #endregion

                #region duplicate types

                yield return Bad("PetriNet mynet: t1)(p1; ",1,1,1).SetName("B.7");
                yield return Bad("PetriNet mynet: t1)-(p1; ",1,1,1).SetName("B.8");
                yield return Bad("PetriNet mynet: t1)--(p1; ",1,1,1).SetName("B.9");
                yield return Bad("PetriNet mynet: t1)-2-(p1; ",1,1,1).SetName("B.10");
                yield return Bad("PetriNet mynet: t1)-2-o-(p1; ",1,1,1).SetName("B.11");

                yield return Bad("PetriNet mynet: t1][p1; ",1,1,1).SetName("B.12");
                yield return Bad("PetriNet mynet: t1]-[p1; ",1,1,1).SetName("B.13");
                yield return Bad("PetriNet mynet: t1]--[p1; ",1,1,1).SetName("B.14");
                yield return Bad("PetriNet mynet: t1]-2-[p1; ",1,1,1).SetName("B.15");
                yield return Bad("PetriNet mynet: t1]-2-o-[p1; ",1,1,1).SetName("B.16");

                #endregion

                #region missing origin

                yield return Bad("PetriNet mynet: )[t1; ",1,1,1).SetName("B.17");
                yield return Bad("PetriNet mynet: )-[t1; ",1,1,1).SetName("B.18");
                yield return Bad("PetriNet mynet: )--[t1; ",1,1,1).SetName("B.19");
                yield return Bad("PetriNet mynet: )---------------------[t1; ",1,1,1).SetName("B.20");
                yield return Bad("PetriNet mynet: )o[t1; ",1,1,1).SetName("B.21");
                yield return Bad("PetriNet mynet: )-o[t1; ",1,1,1).SetName("B.22");
                yield return Bad("PetriNet mynet: )-o-[t1; ",1,1,1).SetName("B.23");
                yield return Bad("PetriNet mynet: )----o----[t1; ",1,1,1).SetName("B.24");
                yield return Bad("PetriNet mynet: )2[t1; ",1,1,1).SetName("B.25");
                yield return Bad("PetriNet mynet: )2-[t1; ",1,1,1).SetName("B.26");
                yield return Bad("PetriNet mynet: )-2[t1; ",1,1,1).SetName("B.27");
                yield return Bad("PetriNet mynet: )--2--[t1; ",1,1,1).SetName("B.28");
                yield return Bad("PetriNet mynet: )------------------2--[t1; ",1,1,1).SetName("B.29");
                yield return Bad("PetriNet mynet: )2------------------[t1; ",1,1,1).SetName("B.30");
                yield return Bad("PetriNet mynet: )2o[t1; ",1,1,1).SetName("B.31");
                yield return Bad("PetriNet mynet: )-2o[t1; ",1,1,1).SetName("B.32");
                yield return Bad("PetriNet mynet: )2-o[t1; ",1,1,1).SetName("B.33");
                yield return Bad("PetriNet mynet: )2o-[t1; ",1,1,1).SetName("B.34");
                yield return Bad("PetriNet mynet: )-2-o[t1; ",1,1,1).SetName("B.35");
                yield return Bad("PetriNet mynet: )-2o-[t1; ",1,1,1).SetName("B.36");
                yield return Bad("PetriNet mynet: )-2-o-[t1; ",1,1,1).SetName("B.37");

                yield return Bad("PetriNet mynet: )-2o[t1; ",1,1,1).SetName("B.38");
                yield return Bad("PetriNet mynet: )2--o[t1; ",1,1,1).SetName("B.39");
                yield return Bad("PetriNet mynet: )2o--[t1; ",1,1,1).SetName("B.40");
                yield return Bad("PetriNet mynet: )--2--o[t1; ",1,1,1).SetName("B.41");
                yield return Bad("PetriNet mynet: )--2o--[t1; ",1,1,1).SetName("B.42");
                yield return Bad("PetriNet mynet: )--2--o--[t1; ",1,1,1).SetName("B.43");

                yield return Bad("PetriNet mynet: ](p1; ",1,1,1).SetName("B.44");
                yield return Bad("PetriNet mynet: ]-(p1; ",1,1,1).SetName("B.45");
                yield return Bad("PetriNet mynet: ]------(p1; ",1,1,1).SetName("B.46");

                yield return Bad("PetriNet mynet: ]2(p1; ",1,1,1).SetName("B.47");
                yield return Bad("PetriNet mynet: ]-2(p1; ",1,1,1).SetName("B.48");
                yield return Bad("PetriNet mynet: ]2-(p1; ",1,1,1).SetName("B.49");
                yield return Bad("PetriNet mynet: ]-2-(p1; ",1,1,1).SetName("B.50");
                yield return Bad("PetriNet mynet: ]--2(p1; ",1,1,1).SetName("B.51");
                yield return Bad("PetriNet mynet: ]2--(p1; ",1,1,1).SetName("B.52");
                yield return Bad("PetriNet mynet: ]--2--(p1; ",1,1,1).SetName("B.53");

                #endregion

                #region missing destination

                yield return Bad("PetriNet mynet: p1)[; ",1,1,1).SetName("B.54");
                yield return Bad("PetriNet mynet: p1)-[; ",1,1,1).SetName("B.55");
                yield return Bad("PetriNet mynet: p1)--[; ",1,1,1).SetName("B.56");
                yield return Bad("PetriNet mynet: p1)---------------------[; ",1,1,1).SetName("B.57");
                yield return Bad("PetriNet mynet: p1)o[; ",1,1,1).SetName("B.58");
                yield return Bad("PetriNet mynet: p1)-o[; ",1,1,1).SetName("B.59");
                yield return Bad("PetriNet mynet: p1)-o-[; ",1,1,1).SetName("B.60");
                yield return Bad("PetriNet mynet: p1)----o----[; ",1,1,1).SetName("B.61");

                yield return Bad("PetriNet mynet: p1)2[; ",1,1,1).SetName("B.62");
                yield return Bad("PetriNet mynet: p1)2-[; ",1,1,1).SetName("B.63");
                yield return Bad("PetriNet mynet: p1)-2[; ",1,1,1).SetName("B.64");
                yield return Bad("PetriNet mynet: p1)--2--[; ",1,1,1).SetName("B.65");
                yield return Bad("PetriNet mynet: p1)------------------2--[; ",1,1,1).SetName("B.66");
                yield return Bad("PetriNet mynet: p1)2------------------[; ",1,1,1).SetName("B.67");

                yield return Bad("PetriNet mynet: p1)2o[; ",1,1,1).SetName("B.68");
                yield return Bad("PetriNet mynet: p1)-2o[; ",1,1,1).SetName("B.69");
                yield return Bad("PetriNet mynet: p1)2-o[; ",1,1,1).SetName("B.70");
                yield return Bad("PetriNet mynet: p1)2o-[; ",1,1,1).SetName("B.71");
                yield return Bad("PetriNet mynet: p1)-2-o[; ",1,1,1).SetName("B.72");
                yield return Bad("PetriNet mynet: p1)-2o-[; ",1,1,1).SetName("B.73");
                yield return Bad("PetriNet mynet: p1)-2-o-[; ",1,1,1).SetName("B.74");

                yield return Bad("PetriNet mynet: p1)-2o[; ",1,1,1).SetName("B.75");
                yield return Bad("PetriNet mynet: p1)2--o[; ",1,1,1).SetName("B.76");
                yield return Bad("PetriNet mynet: p1)2o--[; ",1,1,1).SetName("B.77");
                yield return Bad("PetriNet mynet: p1)--2--o[; ",1,1,1).SetName("B.78");
                yield return Bad("PetriNet mynet: p1)--2o--[; ",1,1,1).SetName("B.79");
                yield return Bad("PetriNet mynet: p1)--2--o--[; ",1,1,1).SetName("B.80");

                yield return Bad("PetriNet mynet: t1](; ",1,1,1).SetName("B.81");
                yield return Bad("PetriNet mynet: t1]-(; ",1,1,1).SetName("B.82");
                yield return Bad("PetriNet mynet: t1]------(; ",1,1,1).SetName("B.83");

                yield return Bad("PetriNet mynet: t1]2(; ",1,1,1).SetName("B.84");
                yield return Bad("PetriNet mynet: t1]-2(; ",1,1,1).SetName("B.85");
                yield return Bad("PetriNet mynet: t1]2-(; ",1,1,1).SetName("B.86");
                yield return Bad("PetriNet mynet: t1]-2-(; ",1,1,1).SetName("B.87");
                yield return Bad("PetriNet mynet: t1]--2(; ",1,1,1).SetName("B.88");
                yield return Bad("PetriNet mynet: t1]2--(; ",1,1,1).SetName("B.89");
                yield return Bad("PetriNet mynet: t1]--2--(; ",1,1,1).SetName("B.90");

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
