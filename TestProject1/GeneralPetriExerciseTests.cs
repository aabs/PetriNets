using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetriNetCore;
using NUnit.Framework;

namespace TestProject1
{
    enum Places
    {
        p1 = 0, p2 = 1, p3 = 2, p4 = 3
    }
    enum Transitions
    {
        t1 = 0, t2 = 1, t3 = 2
    }

    [TestFixture]
    public class TestGraphPetriNet
    {
        [Test]
        public void TestAccessMarking()
        {
            var m = new Marking(3, new Dictionary<int, int> { { 0, 1 }, { 1, 1 }, { 2, 0 } });
            Assert.AreEqual(1, m[1]);
        }

        [Test]
        public void TestMarkingTransitionEnabled()
        {
            var m = new Marking(2, new Dictionary<int, int> { { 0, 0 }, { 1, 0 } });
            var p = CreatePetriNet.Parse(StdPetriNets.OneInOneOut).CreateNet<GraphPetriNet>();
            m[0] = 1;
            Assert.AreEqual(true, p.IsEnabled(0, m));
        }

        [Test]
        public void TestMarkingAffectsEnablement()
        {
            var m = new Marking(2, new Dictionary<int, int> { { 0, 1 }, { 1, 1 } });
            var p = CreatePetriNet.Parse(StdPetriNets.TwoInOneOut).CreateNet<GraphPetriNet>();

            Assert.AreEqual(true, p.IsEnabled(0, m));
            m[0] = 0;
            Assert.AreEqual(false, p.IsEnabled(0, m));
        }

        [Test]
        public void TestFireCreatesModifiedMarking()
        {
            var m = new Marking(2,
                new Dictionary<int, int> { { 0, 1 }, { 1, 0 } });
            var p = CreatePetriNet.Parse(StdPetriNets.OneInOneOut).CreateNet<GraphPetriNet>();
            Assert.AreEqual(1, m[0]);
            Assert.AreEqual(0, m[1]);
            Assert.AreEqual(true, p.IsEnabled(0, m));
            m = p.Fire(m);
            Assert.AreEqual(0, m[0]);
            Assert.AreEqual(1, m[1]);
        }

        [Test]
        public void TestFireCreatesModifiedMarking2()
        {
            var m = new Marking(3,
                new Dictionary<int, int> { { 0, 1 }, { 1, 0 }, { 2, 0 } });
            var p = CreatePetriNet.Parse(StdPetriNets.TwoInOneOut).CreateNet<GraphPetriNet>();
            Assert.IsFalse(p.IsEnabled(0, m));
            m[1] = 1;
            Assert.IsTrue(p.IsEnabled(0, m));
            m = p.Fire(m);
            Assert.AreEqual(0, m[0]);
            Assert.AreEqual(0, m[1]);
            Assert.AreEqual(1, m[2]);
        }

        [Test]
        public void TestMultiEnabledPetriNet()
        {
            var m = new Marking(4,
                new Dictionary<int, int> 
                    { 
                        { (int)Places.p1, 0 }, 
                        { (int)Places.p2, 0 }, 
                        { (int)Places.p3, 0 }, 
                        { (int)Places.p4, 0 } 
                    });

            var p = CreatePetriNet.Parse(StdPetriNets.MultiEnabled).CreateNet<GraphPetriNet>();
            m.Assert(p.Places, new Dictionary<string, int>
                    {{ "p1", 0 }, { "p2", 0 },
                     { "p3", 0 }, { "p4", 0 } });

            m.Set(p.Places, "p1", 1);
            m.Assert(p.Places, new Dictionary<string, int>
                    {{ "p1", 1 }, { "p2", 0 },
                     { "p3", 0 }, { "p4", 0 } });

            m = p.Fire(m);
            m.Assert(p.Places, new Dictionary<string, int>
                    {{ "p1", 0 }, { "p2", 1 },
                     { "p3", 0 }, { "p4", 0 } });

            m = p.Fire(m);
            m.Assert(p.Places, new Dictionary<string, int>
                    {{ "p1", 0 }, { "p2", 0 },
                     { "p3", 1 }, { "p4", 0 } });

            m.Set(p.Places, "p4", 1);
            m.Assert(p.Places, new Dictionary<string, int>
                    {{ "p1", 0 }, { "p2", 0 },
                     { "p3", 1 }, { "p4", 1 } });

            m = p.Fire(m);
            m.Assert(p.Places, new Dictionary<string, int>
                    {{ "p1", 0 }, { "p2", 1 },
                     { "p3", 1 }, { "p4", 0 } });

            m = p.Fire(m);
            m.Assert(p.Places, new Dictionary<string, int>
                    {{ "p1", 0 }, { "p2", 0 },
                     { "p3", 2 }, { "p4", 0 } });
        }

        [Test]
        public void TestBifurcatingTransition() // a bifurcating transition
        {
            var m = new Marking(3,
                new Dictionary<int, int> 
                    { 
                        { (int)Places.p1, 0 }, 
                        { (int)Places.p2, 0 }, 
                        { (int)Places.p3, 0 } 
                    });
            var p = CreatePetriNet.Parse(StdPetriNets.Bifurcation).CreateNet<GraphPetriNet>();

            m.Assert(p.Places, new Dictionary<string, int>{ 
                { "p1", 0 }, { "p2", 0 }, { "p3", 0 } });

            m.Set(p.Places, "p1", 1);
            m.Assert(p.Places, new Dictionary<string, int>{ 
                { "p1", 1 }, { "p2", 0 }, { "p3", 0 } });

            m = p.Fire(m);
            m.Assert(p.Places, new Dictionary<string, int>{ 
                { "p1", 0 }, { "p2", 1 }, { "p3", 1 } });
        }

        [Test]
        public void TestSelfTransition()
        {
            var m = new Marking(1, new Dictionary<int, int> 
                    { 
                        { (int)Places.p1, 0 } 
                    });
            var p = CreatePetriNet.Parse(StdPetriNets.SelfTransition).CreateNet<GraphPetriNet>();
            m.Set(p.Places, "p1", 1);
            Assert.AreEqual(1, m.Get(p.Places,"p1"));
            m = p.Fire(m);
            Assert.AreEqual(1, m.Get(p.Places, "p1"));
        }

        [Test]
        public void TestDoubleSelfTransition()
        {
            var m = new Marking(1,
                new Dictionary<int, int> 
                    { 
                        { (int)Places.p1, 1 } 
                    });
            var p = CreatePetriNet.Parse(StdPetriNets.DoubleSelfTransition).CreateNet<GraphPetriNet>();
            Assert.AreEqual(1, m.Get(p.Places, "p1"));
            m = p.Fire(m);
            Assert.AreEqual(1, m.Get(p.Places, "p1"));
        }

        [Test]
        public void TestLoadPnmlFile()
        {
            var path = @"C:\shared.datastore\repository\personal\dev\prototypes\Automata\PetriNet\pnml.ex1.xml";
            var pnSeq = PnmlModelLoader.Load(path).ToList();
            var markings = PnmlModelLoader.LoadMarkings(path, pnSeq).ToList();
            Assert.IsNotNull(pnSeq);
            Assert.AreEqual(2, pnSeq.Count());
            Assert.AreEqual("n2", pnSeq.ElementAt(0).Id);
            Assert.AreEqual("n1", pnSeq.ElementAt(1).Id);
            var pn1 = pnSeq[0];
            Assert.AreEqual(2, pn1.Places.Count());
            Assert.AreEqual(1, pn1.Transitions.Count());
            Assert.AreEqual(1, pn1.PlaceOutArcs.Count);
            Assert.AreEqual(1, pn1.InArcs.Count);
            Assert.AreEqual(1, pn1.OutArcs.Count);
            Assert.AreEqual(2, markings.Count);
            pn1.Fire(markings.First().Value);
        }

        [Test]
        public void TestTransitionFunctionExecution()
        {
            var m = new Marking(2,
                new Dictionary<int, int> 
                    { 
                        { 0, 2 } ,
                        { 1, 0 } 
                    });
            var p = new GraphPetriNet("p",
                new Dictionary<int, string> {
                    {0, "p0"},
                    {1, "p1"}
                },
                new Dictionary<int, string> 
                    { 
                        { 0, "t0" }
                    },
                new Dictionary<int, List<InArc>>(){
                    {0, new List<InArc>(){new InArc(0)}}
                },
                new Dictionary<int, List<OutArc>>(){
                    {0, new List<OutArc>(){new OutArc(1)}}
                });
            Assert.AreEqual(2, m[0]);
            var someLocal = 0;
            m[0] = 1;
            p.RegisterFunction(0, (t) => someLocal += 1);
            p.Fire(m);
            Assert.AreEqual(1, someLocal);
        }


        [Test]
        public void TestFireConflictingTransitions()
        {
            var m = new Marking(3, new Dictionary<int, int> 
                    { 
                        { 0, 1 } ,
                        { 2, 0 } ,
                        { 1, 0 } 
                    });
            var p = new MatrixPetriNet("p",
                new Dictionary<int, string> {
                    {0, "p0"},
                    {1, "p1"},
                    {2, "p2"}
                },
                new Dictionary<int, string> 
                    { 
                        { 0, "t1" }
                    },
                new Dictionary<int, List<InArc>>(){
                    {0, new List<InArc>(){new InArc(0),new InArc(2)}}
                },
                new Dictionary<int, List<OutArc>>(){
                    {0, new List<OutArc>(){new OutArc(1)}}
                });

            Assert.AreEqual(false, p.IsEnabled(0, m));
            m[2] = 1;
            Assert.AreEqual(true, p.IsEnabled(0, m));
        }

        [Test]
        public void TestInputTransition()
        {
            var m = new Marking(1,
                new Dictionary<int, int> 
                    { 
                        { 0, 0 } 
                    });
            var p = new GraphPetriNet("p",
                new Dictionary<int, string> {
                    {0, "p0"}
                },
                new Dictionary<int, string> 
                    { 
                        { 0, "Ti" }
                    },
                new Dictionary<int, List<InArc>>() { },
                new Dictionary<int, List<OutArc>>(){
                    {0, new List<OutArc>(){new OutArc(0)}}
                });
            Assert.IsTrue(p.IsEnabled(0, m));
            m = p.Fire(m);
            Assert.AreEqual(1, m[0]);
            Assert.IsTrue(p.IsEnabled(0, m));
            m = p.Fire(m);
            Assert.AreEqual(2, m[0]);
            Assert.IsTrue(p.IsEnabled(0, m));
        }

        [Test]
        public void TestDrainTransition()
        {
            var m = new Marking(1, new Dictionary<int, int> 
                    { 
                        { 0, 5 } 
                    });
            var p = new GraphPetriNet("p",
                new Dictionary<int, string> {
                    {0, "p0"}
                },
                new Dictionary<int, string> 
                    { 
                        { 0, "Ti" }
                    },
                new Dictionary<int, List<InArc>>() { 
                    {0, new List<InArc>(){new InArc(0)}}
                },
                new Dictionary<int, List<OutArc>>() { });

            for (int i = 5; i >= 0; i--)
            {
                Assert.AreEqual(i, m[0]);
                Assert.AreEqual(i > 0, p.IsEnabled(0, m));
                m = p.Fire(m);
            }
        }

        [Test]
        public void TestPrioritySetup()
        {
            var p = new GraphPetriNet("p",
               new Dictionary<int, string> {
                    {0, "p0"},
                    {1, "p1"},
                    {2, "p2"}
                },
               new Dictionary<int, string> 
                    { 
                        { 0, "t1" },
                        { 1, "t2" }
                    },
               new Dictionary<int, List<InArc>>(){
                    {0, new List<InArc>(){new InArc(0),new InArc(1)}},
                    {1, new List<InArc>(){new InArc(1),new InArc(2)}}
                },
               new Dictionary<int, List<OutArc>>() { },
               new Dictionary<int, int>() { { 0, 1 }, { 1, 2 } });
            Assert.AreEqual(1, p.GetTransitionPriority(0));
            Assert.AreEqual(2, p.GetTransitionPriority(1));
        }

        [Test]
        public void TestConflictDetection()
        {
            var m = new Marking(3,
               new Dictionary<int, int> 
                    { 
                        { 0, 1 } ,
                        { 1, 1 } ,
                        { 2, 1 } 
                    });
            var p = new GraphPetriNet("p",
               new Dictionary<int, string> {
                    {0, "p0"},
                    {1, "p1"},
                    {2, "p2"}
                },
               new Dictionary<int, string> 
                    { 
                        { 0, "t1" },
                        { 1, "t2" }
                    },
               new Dictionary<int, List<InArc>>(){
                    {0, new List<InArc>(){new InArc(0),new InArc(1)}},
                    {1, new List<InArc>(){new InArc(1),new InArc(2)}}
                },
               new Dictionary<int, List<OutArc>>() { },
               new Dictionary<int, int>() { { 0, 1 }, { 1, 2 } });
            var enabledTransitions = p.AllEnabledTransitions(m);
            Assert.AreEqual(2, enabledTransitions.Count());
            Assert.IsTrue(enabledTransitions.Contains(0));
            Assert.IsTrue(enabledTransitions.Contains(1));
            Assert.IsTrue(p.IsConflicted(m));
        }

        [Test]
        public void TestPrioritySelection()
        {
            var m = new Marking(3,
               new Dictionary<int, int> 
                    { 
                        { 0, 1 } ,
                        { 1, 1 } ,
                        { 2, 1 } 
                    });
            var p = new GraphPetriNet("p",
               new Dictionary<int, string> {
                    {0, "p0"},
                    {1, "p1"},
                    {2, "p2"}
                },
               new Dictionary<int, string> 
                    { 
                        { 0, "t1" },
                        { 1, "t2" }
                    },
               new Dictionary<int, List<InArc>>(){
                    {0, new List<InArc>(){new InArc(0),new InArc(1)}},
                    {1, new List<InArc>(){new InArc(1),new InArc(2)}}
                },
               new Dictionary<int, List<OutArc>>() { },
               new Dictionary<int, int>() { { 1, 1 } }); // t0 will be baseline at 0 and t1 will be 1, therefore next enabled transition should always be 1
            int? transId = p.GetNextTransitionToFire(m);
            Assert.IsTrue(transId.HasValue);
            Assert.AreEqual(1, transId.Value);
        }
        [Test]
        public void TestPrioritySelection2()
        {
            var m = new Marking(4,
              new Dictionary<int, int> 
                    { 
                        { 0, 1 } ,
                        { 1, 1 } ,
                        { 2, 1 } ,
                        { 3, 1 } 
                    });
            var p = new GraphPetriNet("p",
               new Dictionary<int, string> {
                    {0, "p0"},
                    {1, "p1"},
                    {2, "p2"},
                    {3, "p3"}
                },
               new Dictionary<int, string> 
                    { 
                        { 0, "t1" },
                        { 1, "t2" },
                        { 2, "t3" }
                    },
               new Dictionary<int, List<InArc>>(){
                    {0, new List<InArc>(){new InArc(0),new InArc(1)}},
                    {1, new List<InArc>(){new InArc(1),new InArc(2)}},
                    {2, new List<InArc>(){new InArc(1),new InArc(2),new InArc(3)}}
                },
               new Dictionary<int, List<OutArc>>() { },
               new Dictionary<int, int>() 
               {
                    { 0, 1 } ,
                    { 1, 1 } ,
                    { 2, 4 } 
               }); // t0 will be baseline at 0 and t1 will be 1, therefore next enabled transition should always be 1
            int? transId = p.GetNextTransitionToFire(m);
            Assert.IsTrue(transId.HasValue);
            Assert.AreEqual(2, transId.Value);
        }
    }
}
