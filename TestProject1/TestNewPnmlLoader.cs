using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PetriNetCore;

namespace TestProject1
{
    [TestFixture]
    public class TestNewPnmlLoader
    {
        private string modelPathRoot = @"C:\dev\PetriNets\PNML\standard test nets\";

        [Test, Category("Regression")]
        public void TestLoad1()
        {
            var loader = new NewPnmlLoader<GraphPetriNet>();
            var model = loader.Load(modelPathRoot + "p0).xml");
        }

        [Test, Category("Regression")]
        [TestCaseSource(typeof(PnmlLoaderDataGenerator), "FullSpecs")]
        public void TestLoadPnmlFile(string path, IEnumerable<string> markingProgression)
        {
            var loader = new NewPnmlLoader<GraphPetriNet>();
            var netlist = loader.Load(path);
            Assert.AreEqual(1, netlist.Count());
            var net = netlist.ElementAt(0);
            foreach (string strMarking in markingProgression)
            {

            }
        }
    }
    public static class PnmlLoaderDataGenerator
    {
        private const string contractException = "System.Diagnostics.Contracts.__ContractsRuntime+ContractException";

        public static IEnumerable FullSpecs
        {
            get
            {
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\p0)-2-[t0]-2-(p0.xml").SetName("p0)-2-[t0]-2-(p0.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\double-self-transition.xml").SetName("double-self-transition.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\p0)-2-[t0]-2-(p0.xml").SetName("p0)-2-[t0]-2-(p0.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\p0)-[t0.xml").SetName("p0)-[t0.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\p0)-[t0]-(p0.xml").SetName("p0)-[t0]-(p0.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\p0)-[t0]-({p1,p2,p3}.xml").SetName("p0)-[t0]-({p1,p2,p3}.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\p0)-[t0]-({p1,p2}.xml").SetName("p0)-[t0]-({p1,p2}.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\p0)-[{t0.1, t1.2, t2.3}.xml").SetName("p0)-[{t0.1, t1.2, t2.3}.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\p0)-[{t0.1, t1.2}.xml").SetName("p0)-[{t0.1, t1.2}.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\p0)-[{t0.1,t1.1}.xml").SetName("p0)-[{t0.1,t1.1}.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\t0]-({p0,p1})-[t2.xml").SetName("t0]-({p0,p1})-[t2.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\t3]-(p0)-[{t0.1,t1.2,t2.3}.xml").SetName("t3]-(p0)-[{t0.1,t1.2,t2.3}.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\two-step-cycle.xml").SetName("two-step-cycle.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\{P0,P1})-[T0]-({P2,P3}.xml").SetName("{P0,P1})-[T0]-({P2,P3}.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\{p0, p1})-[t0.xml").SetName("{p0, p1})-[t0.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\{p0,p1,p2})-[t0]-(p3.xml").SetName("{p0,p1,p2})-[t0]-(p3.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\{p0,p1})-[t0]-(p2.xml").SetName("{p0,p1})-[t0]-(p2.xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\p0).xml").SetName("p0).xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\t0].xml").SetName("t0].xml");
                yield return Good(@"C:\dev\PetriNets\PNML\standard test nets\{t0,t1.2}]-(p0.xml").SetName("{t0,t1.2}]-(p0.xml");
            }
        }
/*
        public static IEnumerable FailingSpecs
        {
            get
            {
            }
        }
*/

        private static TestCaseData Good(params string[] args)
        {
            return new TestCaseData(args[0], args.Skip(1));
        }

        private static TestCaseData Bad(params object[] args)
        {
            return new TestCaseData(args).Throws(contractException);
        }
    }
}
