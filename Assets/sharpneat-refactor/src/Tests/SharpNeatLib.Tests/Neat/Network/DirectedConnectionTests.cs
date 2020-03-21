﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpNeat.Network;

namespace SharpNeat.Tests.Neat.Network
{
    [TestClass]
    public class DirectedConnectionTests
    {
        [TestMethod]
        [TestCategory("DirectedConnection")]
        public void TestDirectedConnection_Equals()
        {
            Assert.IsTrue(new DirectedConnection(10,20).Equals(new DirectedConnection(10,20)));
            Assert.IsTrue(new DirectedConnection(10,20) ==  new DirectedConnection(10,20));

            Assert.IsFalse(!new DirectedConnection(10,20).Equals(new DirectedConnection(10,20)));
            Assert.IsFalse(new DirectedConnection(10,20) !=  new DirectedConnection(10,20));

            Assert.IsFalse(new DirectedConnection(10,20).Equals(new DirectedConnection(10,21)));
            Assert.IsFalse(new DirectedConnection(10,20) ==  new DirectedConnection(10,21));

            Assert.IsFalse(new DirectedConnection(10,20).Equals(new DirectedConnection(11,20)));
            Assert.IsFalse(new DirectedConnection(10,20) ==  new DirectedConnection(11,20));
        }

        [TestMethod]
        [TestCategory("DirectedConnection")]
        public void TestDirectedConnection_LessThan()
        {
            Assert.IsTrue(new DirectedConnection(10,20) < (new DirectedConnection(10,21)));
            Assert.IsTrue(new DirectedConnection(10,20) < (new DirectedConnection(11,20)));

            Assert.IsFalse(new DirectedConnection(10,20) < (new DirectedConnection(10,20)));
            Assert.IsFalse(new DirectedConnection(10,20) < (new DirectedConnection(9,20)));
            Assert.IsFalse(new DirectedConnection(10,20) < (new DirectedConnection(10,19)));
            Assert.IsFalse(new DirectedConnection(10,20) < (new DirectedConnection(9,19)));
        }

        [TestMethod]
        [TestCategory("DirectedConnection")]
        public void TestDirectedConnection_GreaterThan()
        {
            Assert.IsTrue(new DirectedConnection(10,21) > (new DirectedConnection(10,20)));
            Assert.IsTrue(new DirectedConnection(11,20) > (new DirectedConnection(10,20)));

            Assert.IsFalse(new DirectedConnection(10,20) > (new DirectedConnection(10,20)));
            Assert.IsFalse(new DirectedConnection(9,20) > (new DirectedConnection(10,20)));
            Assert.IsFalse(new DirectedConnection(10,19) > (new DirectedConnection(10,20)));
            Assert.IsFalse(new DirectedConnection(9,19) > (new DirectedConnection(10,20)));
        }

        [TestMethod]
        [TestCategory("DirectedConnection")]
        public void TestDirectedConnection_CompareTo()
        {
            Assert.AreEqual(0, new DirectedConnection(10,20).CompareTo(new DirectedConnection(10,20)));

            Assert.AreEqual(1, new DirectedConnection(10,21).CompareTo(new DirectedConnection(10,20)));
            Assert.AreEqual(1, new DirectedConnection(11,20).CompareTo(new DirectedConnection(10,20)));
            Assert.AreEqual(1, new DirectedConnection(11,21).CompareTo(new DirectedConnection(10,20)));

            Assert.AreEqual(-1, new DirectedConnection(10,20).CompareTo(new DirectedConnection(10,21)));
            Assert.AreEqual(-1, new DirectedConnection(10,20).CompareTo(new DirectedConnection(11,20)));
            Assert.AreEqual(-1, new DirectedConnection(10,20).CompareTo(new DirectedConnection(11,21)));

            Assert.IsTrue(new DirectedConnection(0,0).CompareTo(new DirectedConnection(0,int.MaxValue)) < 0);
            Assert.IsTrue(new DirectedConnection(0,0).CompareTo(new DirectedConnection(int.MaxValue,0)) < 0);
            Assert.IsTrue(new DirectedConnection(0,0).CompareTo(new DirectedConnection(int.MaxValue,int.MaxValue)) < 0);

            Assert.IsTrue(new DirectedConnection(0,int.MaxValue).CompareTo(new DirectedConnection(0,0)) > 0);
            Assert.IsTrue(new DirectedConnection(int.MaxValue,0).CompareTo(new DirectedConnection(0,0)) > 0);
            Assert.IsTrue(new DirectedConnection(int.MaxValue,int.MaxValue).CompareTo(new DirectedConnection(0,0)) > 0);

            Assert.IsTrue(new DirectedConnection(0,int.MaxValue).CompareTo(new DirectedConnection(0,int.MaxValue)) == 0);
            Assert.IsTrue(new DirectedConnection(int.MaxValue,0).CompareTo(new DirectedConnection(int.MaxValue,0)) == 0);
            Assert.IsTrue(new DirectedConnection(int.MaxValue,int.MaxValue).CompareTo(new DirectedConnection(int.MaxValue,int.MaxValue)) == 0);
        }
    }
}
