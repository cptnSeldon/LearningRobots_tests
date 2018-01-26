using System.Drawing;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace P3_LearningRobots.Tests
{
    [TestClass]
    public class UtilsTest
    {
        [TestMethod]
        public void TestCalculateDistance()
        {

            /**
                x

            ---------

                .

            ---------
             
             */
            PointF left = new PointF(1, 1);
            PointF right = new PointF(1, 0);

            Rectangle r = new Rectangle(0, 2, 2, 2);

            Assert.AreEqual(2, Utils.CalculateDistance(left, right, r)[0]);
            Assert.AreEqual(3, Utils.CalculateDistance(left, right, r)[1]);

        }

        [TestMethod]
        public void TestCalculateTotalDistance()
        {
            PointF left = new PointF(1, 1);
            PointF right = new PointF(1, 0);

            Rectangle r = new Rectangle(0, 2, 2, 2);

            Assert.AreEqual(2, Utils.GetTotalDistances(left, right, new List<Rectangle>() {r})[0]);
            Assert.AreEqual(3, Utils.GetTotalDistances(left, right, new List<Rectangle>() {r})[1]);

            Assert.AreEqual(6, Utils.GetTotalDistances(left, right, new List<Rectangle>() { r, r })[1]);
        }

        [TestMethod]
        public void TestGetPercentage()
        {
            PointF left = new PointF(1, 1);
            PointF right = new PointF(1, 0);

            Rectangle r = new Rectangle(0, 2, 2, 2);

            Assert.AreEqual(1, Utils.GetPercentage(left, right, new List<Rectangle>() { r })[0]);
            Assert.AreEqual(1, Utils.GetPercentage(left, right, new List<Rectangle>() { r })[1]);

            Assert.AreEqual(0.5, Utils.GetPercentage(left, right, new List<Rectangle>() { r, r })[1]);

            Assert.AreEqual(0.25, Utils.GetPercentage(left, right, new List<Rectangle>() { r, r, r, r })[1]);

        }
    }
}
