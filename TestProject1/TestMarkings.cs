using System;
using NUnit.Framework;
using PetriNetCore;

namespace TestProject1
{
    [TestFixture]
    public class TestMarkings
    {

        [Test]
        public void TestCreateMarking()
        {
            var m = new Marking(1);
            Assert.IsNotNull(m);
            Assert.AreEqual(0, m[0]); 
        }

        [Test, ExpectedException(typeof(Exception))]
        public void TestCreateMarkingWithInvalidSize()
        {
            var m = new Marking(0);
        }

        [Test, ExpectedException(typeof(Exception))]
        public void TestCreateMarkingWithInvalidSize2()
        {
            var m = new Marking(-1);
        }

        [Test, ExpectedException(typeof(Exception))]
        public void TestCreateMarkingWithInvalidSize3()
        {
            var m = new Marking(int.MinValue);
        }

        [Test, ExpectedException(typeof(Exception))]
        public void TestCreateMarkingWithInvalidSize4()
        {
            var m = new Marking(int.MaxValue);
        }

        [Test, ExpectedException(typeof(Exception))]
        public void TestCreateMarkingWithInvalidSize5()
        {
            var m = new Marking(GraphPetriNet.MaxSize + 1);
        }

        [Test]
        public void TestCreateMarkingWithMaxSize()
        {
            var m = new Marking(GraphPetriNet.MaxSize);
        }
  
  
    }
}
