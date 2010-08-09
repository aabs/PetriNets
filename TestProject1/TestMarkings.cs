using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PetriNetCore;

namespace TestProject1
{
    [TestClass]
    public class TestMarkings
    {

        [TestMethod]
        public void TestCreateMarking()
        {
            var m = new Marking(1);
            Assert.IsNotNull(m);
            Assert.AreEqual(0, m[0]); 
        }

        [TestMethod, ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void TestCreateMarkingWithInvalidSize()
        {
            var m = new Marking(0);
        }

        [TestMethod, ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void TestCreateMarkingWithInvalidSize2()
        {
            var m = new Marking(-1);
        }

        [TestMethod, ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void TestCreateMarkingWithInvalidSize3()
        {
            var m = new Marking(int.MinValue);
        }

        [TestMethod, ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void TestCreateMarkingWithInvalidSize4()
        {
            var m = new Marking(int.MaxValue);
        }

        [TestMethod, ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void TestCreateMarkingWithInvalidSize5()
        {
            var m = new Marking(GraphPetriNet.MaxSize + 1);
        }

        [TestMethod]
        public void TestCreateMarkingWithMaxSize()
        {
            var m = new Marking(GraphPetriNet.MaxSize);
        }
  
  
    }
}
