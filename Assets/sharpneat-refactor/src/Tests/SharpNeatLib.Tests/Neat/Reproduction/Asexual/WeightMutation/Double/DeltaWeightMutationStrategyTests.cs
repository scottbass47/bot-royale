﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Redzen.Numerics;
using Redzen.Random;
using SharpNeat.Neat.Reproduction.Asexual.WeightMutation.Double;
using SharpNeat.Tests.Neat.Reproduction.Asexual.WeightMutation.Selection;

namespace SharpNeat.Tests.Neat.Reproduction.Asexual.WeightMutation.Double
{
    [TestClass]
    public class DeltaWeightMutationStrategyTests
    {
        #region Test Methods

        [TestMethod]
        [TestCategory("DeltaWeightMutationStrategy")]
        public void TestUniformDelta()
        {
            double weightScale = 5.0;
            var strategy = DeltaWeightMutationStrategy.CreateUniformDeltaStrategy(
                new SelectAllStrategy(),
                weightScale);

            IRandomSource rng = RandomDefaults.CreateRandomSource(0);

            int iters = 10_000;
            double[] weightArr = new double[iters];
            for (int i = 0; i < iters; i++) {
                weightArr[i] = 1000.0;
            }

            strategy.Invoke(weightArr, rng);

            // Construct a histogram on the array of weights.
            HistogramData hist = NumericsUtils.BuildHistogramData(weightArr, 8);

            // We expect samples to be approximately evenly distributed over the histogram buckets.
            for (int i = 0; i < hist.FrequencyArray.Length; i++) {
                Assert.IsTrue(hist.FrequencyArray[i] > (iters / 8) * 0.8);
            }

            // We expect min and max to be close to 1000-weightScale and 1000+weightScale respectively.
            Assert.IsTrue(hist.Max <= (1000 + weightScale) && hist.Max > (1000 + weightScale)-0.1);
            Assert.IsTrue(hist.Min >= (1000 - weightScale) && hist.Min < (1000 - weightScale)+0.1);
        }

        [TestMethod]
        [TestCategory("DeltaWeightMutationStrategy")]
        public void TestGaussianDelta()
        {
            var strategy = DeltaWeightMutationStrategy.CreateGaussianDeltaStrategy(
                new SelectAllStrategy(),
                1.0);

            IRandomSource rng = RandomDefaults.CreateRandomSource(0);

            int iters = 100_000;
            double[] weightArr = new double[iters];
            for (int i = 0; i < iters; i++) {
                weightArr[i] = 1000.0;
            }

            strategy.Invoke(weightArr, rng);

            // Construct a histogram on the array of weights.
            HistogramData hist = NumericsUtils.BuildHistogramData(weightArr, 8);

            // We expect min and max to be close to be about -995.5 and +1004.5 respectively 
            // (but they could be further from the mean of 1000, with no bound).
            Assert.IsTrue(hist.Max >= 1002.0);
            Assert.IsTrue(hist.Min <= 998.0);

            TestMean(weightArr, 1000.0);
        }

        #endregion

        #region Private Static Methods

        private static void TestMean(double[] sampleArr, double expectedMean)
        {
            double mean = sampleArr.Average();
            Assert.IsTrue(Math.Abs(mean)-expectedMean < 0.1);
        }

        private static void TestStandardDeviation(double[] sampleArr)
        {
            double mean = sampleArr.Average();

            double sqrSum = 0.0;
            for(int i=0; i < sampleArr.Length; i++)
            {
                double x = sampleArr[i] - mean;
                sqrSum += x*x;
            }

            double var = sqrSum / sampleArr.Length;
            double stdDev = Math.Sqrt(var);
            Assert.IsTrue(Math.Abs(stdDev-1.0) < 0.1);
        }

        #endregion
    }
}
