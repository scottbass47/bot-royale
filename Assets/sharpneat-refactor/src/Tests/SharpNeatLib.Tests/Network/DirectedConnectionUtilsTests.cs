﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpNeat.Network;

namespace SharpNeat.Tests.Neat.Genome
{
    [TestClass]
    public class DirectedConnectionUtilsTests
    {
        #region Test Methods

        [TestMethod]
        [TestCategory("ConnectionGeneUtils")]
        public void TestGetConnectionIndexBySourceNodeId()
        {
            var connArr = new DirectedConnection[9];
            connArr[0] = new DirectedConnection(1, 70);
            connArr[1] = new DirectedConnection(4, 14);
            connArr[2] = new DirectedConnection(6, 23);
            connArr[3] = new DirectedConnection(7, 36);
            connArr[4] = new DirectedConnection(10, 18);
            connArr[5] = new DirectedConnection(10, 31);
            connArr[6] = new DirectedConnection(14, 2);
            connArr[7] = new DirectedConnection(20, 24);
            connArr[8] = new DirectedConnection(25, 63);

            int idx = DirectedConnectionUtils.GetConnectionIndexBySourceNodeId(connArr, 10);
            Assert.AreEqual(4, idx);

            idx = DirectedConnectionUtils.GetConnectionIndexBySourceNodeId(connArr, 25);
            Assert.AreEqual(8, idx);

            idx = DirectedConnectionUtils.GetConnectionIndexBySourceNodeId(connArr, 26);
            Assert.AreEqual(~9, idx);

            idx = DirectedConnectionUtils.GetConnectionIndexBySourceNodeId(connArr, 1);
            Assert.AreEqual(0, idx);

            idx = DirectedConnectionUtils.GetConnectionIndexBySourceNodeId(connArr, 0);
            Assert.AreEqual(~0, idx);
        }
        
        #endregion
    }
}
