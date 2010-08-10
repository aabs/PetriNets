using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PetriNetCore;

namespace PetriNetCore
{
}
namespace TestProject1
{
    [TestClass]
    public class TestPetriNetCreator
    {
        #region test create petri net builder
        [TestMethod]
        public void TestCreatePetriNetBuilder()
        {
            var pn = CreatePetriNet.Called("p1");
            Assert.IsNotNull(pn);
        }
        #endregion

        #region test name
        [TestMethod]
        public void TestNameIsInitialised()
        {
            var pn = CreatePetriNet.Called("p1");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(pn.Name));
            Assert.AreEqual("p1", pn.Name);
        }


        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestNameCannotBeInitdWithJunk1()
        {
            var pn = CreatePetriNet.Called("");
            Assert.IsTrue(!string.IsNullOrWhiteSpace(pn.Name));
            Assert.AreEqual("", pn.Name);
        }


        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestNameCannotBeInitdWithJunk2()
        {
            var pn = CreatePetriNet.Called("\0");
        }
        #endregion

        #region test places
        [TestMethod]
        public void TestCreatePlaces()
        {
            var pn = CreatePetriNet.Called("net").WithPlaces("p1", "p2", "p3");
            Assert.IsNotNull(pn.Places);
            Assert.IsTrue(pn.Places.Count == 3);
            Assert.AreEqual(pn.Places[0], "p1");
            Assert.AreEqual(pn.Places[1], "p2");
            Assert.AreEqual(pn.Places[2], "p3");
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCreateWithBadNames1()
        {
            var pn = CreatePetriNet.Called("net").WithPlaces();

        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCreateWithBadNames2()
        {
            var pn = CreatePetriNet.Called("net").WithPlaces(null);

        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCreateWithBadNames3()
        {
            var pn = CreatePetriNet.Called("net").WithPlaces("p1", "", "p2");
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCreateWithBadNames4()
        {
            var pn = CreatePetriNet.Called("net").WithPlaces("p1", null, "p2");
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCreateWithBadNames5()
        {
            var pn = CreatePetriNet.Called("net").WithPlaces("p1", "\0", "p2");
        }
        #endregion

        #region test transitions
        [TestMethod]
        public void TestCreateTransitions()
        {
            var pn = CreatePetriNet
                .Called("net")
                .WithPlaces("p1", "p2", "p3")
                .WithTransitions("t1", "t2", "t3");
            Assert.IsNotNull(pn.Places);
            Assert.IsTrue(pn.Places.Count == 3);
            Assert.AreEqual(pn.Places[0], "p1");
            Assert.AreEqual(pn.Places[1], "p2");
            Assert.AreEqual(pn.Places[2], "p3");
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCreateTransitionsWithBadNames1()
        {
            var pn = CreatePetriNet
                .Called("net")
                .WithPlaces("p1", "p2", "p3").WithTransitions();

        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCreateTransitionsWithBadNames2()
        {
            var pn = CreatePetriNet
                .Called("net")
                .WithPlaces("p1", "p2", "p3").WithTransitions(null);

        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCreateTransitionsWithBadNames3()
        {
            var pn = CreatePetriNet
                .Called("net")
                .WithPlaces("p1", "p2", "p3").WithTransitions("p1", "", "p2");
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCreateTransitionsWithBadNames4()
        {
            var pn = CreatePetriNet
                .Called("net")
                .WithPlaces("p1", "p2", "p3").WithTransitions("p1", null, "p2");
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestCreateTransitionsWithBadNames5()
        {
            var pn = CreatePetriNet
                .Called("net")
                .WithPlaces("p1", "p2", "p3").WithTransitions("p1", "\0", "p2");
        }
        #endregion

        #region prevent multiple calls

        [TestMethod, ExpectedException(typeof(ApplicationException))]
        public void TestCallPlacesTwice()
        {
            var pn = CreatePetriNet
                .Called("net")
                .WithPlaces("p1")
                .WithPlaces("p1")
                .WithTransitions("p1");
        }

        [TestMethod, ExpectedException(typeof(ApplicationException))]
        public void TestCalltransitionsTwice()
        {
            var pn = CreatePetriNet
                .Called("net")
                .WithPlaces("p1")
                .WithTransitions("p1")
                .WithTransitions("p1");
        }

        [TestMethod, ExpectedException(typeof(ApplicationException))]
        public void TestCallPlacesTwiceOutOfOrder()
        {
            var pn = CreatePetriNet
                .Called("net")
                .WithPlaces("p1")
                .WithTransitions("p1")
                .WithPlaces("p1");
        }

        [TestMethod, ExpectedException(typeof(ApplicationException))]
        public void TestCalltransitionsTwiceOutOfOrder()
        {
            var pn = CreatePetriNet
                .Called("net")
                .WithTransitions("p1")
                .WithPlaces("p1")
                .WithTransitions("p1");
        }

        #endregion

        #region create connections (positive test)
        public void AssertMarkings<T1, T2>(Marking m, Dictionary<T1, T2> markingsExpected)
        {
            foreach (var marking in markingsExpected)
            {
                Assert.AreEqual(marking.Value, m[Convert.ToInt32(marking.Key)]);
            }
        }

        [TestMethod]
        public void TestFullMontySetup()
        {
            var pnb = CreatePetriNet
                .Called("net")
                .WithPlaces("p1", "p2", "p3")
                .AndTransitions("t1", "t2", "t3")

                .With("t1").FedBy("p1", "p2")
                .And().With("t2").FedBy("p3").AsInhibitor()
                .And().With("t1").Feeding("p2")
                .And().With("t2").Feeding("p1")

                .And().WhenFiring("t1")
                    .Run(x => Console.WriteLine("Fired"))
                    .Run(x => Console.WriteLine("Fired"))
                    .Run(x => Console.WriteLine("Fired"))

                .And().WhenFiring("t2")
                    .Run(x => Console.WriteLine("Fired"))
                    .Run(x => Console.WriteLine("Fired"))
                    .Run(x => Console.WriteLine("Fired"))
                .Complete()
                ;
            var pn = pnb.CreateNet();
            Assert.IsNotNull(pn);
            var m = pnb.CreateMarking();
        }

        [TestMethod]
        public void TestCompareWithOldConstructionTechnique()
        {
            var m = new Marking(3,
                    new Dictionary<int, int> 
                    { 
                        { (int)Places.p1, 0 }, 
                        { (int)Places.p2, 0 }, 
                        { (int)Places.p3, 0 } 
                    });
            var p = new GraphPetriNet("p",
                new Dictionary<int, string> {
                    {(int)Places.p1, "p1"},
                    {(int)Places.p2, "p2"},
                    {(int)Places.p3, "p3"}
                },
                new Dictionary<int, string> 
                    { 
                        { (int)Transitions.t1, "t1" }
                    },
                new Dictionary<int, List<InArc>>(){
                    {(int)Transitions.t1, new List<InArc>(){new InArc((int)Places.p1)}}
                },
                new Dictionary<int, List<OutArc>>(){
                    {(int)Transitions.t1, new List<OutArc>(){new OutArc((int)Places.p2),
                                                             new OutArc((int)Places.p3)}}
                });
            
            var pnc = CreatePetriNet.Called("p")
                .WithPlaces("p1",
                            "p2",
                            "p3")
                .AndTransitions("t1")
                .With("t1").FedBy("p1")
                .And().With("t1").Feeding("p2",
                                          "p3")
                .Done();
            var p2 = pnc.CreateNet();

            m[(int)Places.p1] = 1;
            
            var m2 = p.Fire(m);
            var m3 = p2.Fire(m);

            AssertMarkings(m2, new Dictionary<int, double>{ 
                { (int)Places.p1, 0 }, 
                { (int)Places.p2, 1 },
                { (int)Places.p3, 1 } });
            AssertMarkings(m3, new Dictionary<int, double>{ 
                { (int)Places.p1, 0 }, 
                { (int)Places.p2, 1 },
                { (int)Places.p3, 1 } });

        }

        #endregion
    }
}
