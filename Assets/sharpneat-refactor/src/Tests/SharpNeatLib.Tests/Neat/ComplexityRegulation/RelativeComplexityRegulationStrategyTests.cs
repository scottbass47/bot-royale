﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpNeat.EvolutionAlgorithm;
using SharpNeat.Neat.ComplexityRegulation;

namespace SharpNeatLib.Tests.Neat.ComplexityRegulation
{
    [TestClass]
    public class RelativeComplexityRegulationStrategyTests
    {
        #region Test Methods

        [TestMethod]
        [TestCategory("RelativeComplexityRegulationStrategyTests")]
        public void TestInitialisation()
        {
            var strategy = new RelativeComplexityRegulationStrategy(10, 10.0);

            var eaStats = new EvolutionAlgorithmStatistics();
            var popStats = new PopulationStatistics();

            // The strategy should initialise to, and remain in, Complexifying mode.
            for(int i=0; i < 100; i++)
            {
                eaStats.Generation = i;
                ComplexityRegulationMode mode = strategy.UpdateMode(eaStats, popStats);
                Assert.AreEqual(ComplexityRegulationMode.Complexifying, mode);
            }
        }

        [TestMethod]
        [TestCategory("RelativeComplexityRegulationStrategyTests")]
        public void TestTransitionToSimplifying()
        {
            var strategy = new RelativeComplexityRegulationStrategy(10, 10.0);

            var eaStats = new EvolutionAlgorithmStatistics();
            var popStats = new PopulationStatistics();
            ComplexityRegulationMode mode;

            // The strategy should initialise to, and remain in, Complexifying mode 
            // while mean population complexity is below the threshold.
            for (int i = 0; i < 11; i++)
            {
                eaStats.Generation = i;
                popStats.MeanComplexity = i;
                popStats.MeanComplexityHistory.Enqueue(i);
                mode = strategy.UpdateMode(eaStats, popStats);
                Assert.AreEqual(ComplexityRegulationMode.Complexifying, mode);
            }

            // The strategy should switch to simplifying mode when mean complexity
            // rises above the threshold.
            eaStats.Generation = 11;
            popStats.MeanComplexity = 10.01;
            popStats.MeanComplexityHistory.Enqueue(10.01);
            mode = strategy.UpdateMode(eaStats, popStats);
            Assert.AreEqual(ComplexityRegulationMode.Simplifying, mode);
        }

        [TestMethod]
        [TestCategory("RelativeComplexityRegulationStrategyTests")]
        public void TestTransitionToComplexifying()
        {
            var strategy = new RelativeComplexityRegulationStrategy(10, 10.0);

            var eaStats = new EvolutionAlgorithmStatistics();
            var popStats = new PopulationStatistics();
            ComplexityRegulationMode mode;

            // Cause an immediate switch to into simplifying mode.
            int generation = 0;
            eaStats.Generation = generation++;
            popStats.MeanComplexity = 11.0;
            popStats.MeanComplexityHistory.Enqueue(11.0);
            mode = strategy.UpdateMode(eaStats, popStats);
            Assert.AreEqual(ComplexityRegulationMode.Simplifying, mode);

            // Reset the buffer that the moving average is calculated from;
            // This allows us to change the mean to below the threshold and for the
            // moving average to start rising immediately; that would ordinarily cause 
            // an immediate switch back to complexifying mode, but that is prevented by
            // {minSimplifcationGenerations} being set to 10.
            popStats.MeanComplexityHistory.Clear();

            for (int i = 0; i < 10; i++)
            {
                eaStats.Generation = generation++;
                popStats.MeanComplexity = 2.0;
                popStats.MeanComplexityHistory.Enqueue(2.0);
                mode = strategy.UpdateMode(eaStats, popStats);
                Assert.AreEqual(ComplexityRegulationMode.Simplifying, mode);
            }

            // Now that {minSimplifcationGenerations} have passed, the strategy should switch
            // back to complexifying mode.
            eaStats.Generation = generation++;
            popStats.MeanComplexity = 2.0;
            popStats.MeanComplexityHistory.Enqueue(2.0);
            mode = strategy.UpdateMode(eaStats, popStats);
            Assert.AreEqual(ComplexityRegulationMode.Complexifying, mode);

            // The threshold should have been set relative to the popStats.MeanComplexity (to 12).
            eaStats.Generation = generation++;
            popStats.MeanComplexity = 11.9;
            popStats.MeanComplexityHistory.Enqueue(11.9);
            mode = strategy.UpdateMode(eaStats, popStats);
            Assert.AreEqual(ComplexityRegulationMode.Complexifying, mode);

            eaStats.Generation = generation++;
            popStats.MeanComplexity = 12.01;
            popStats.MeanComplexityHistory.Enqueue(12.01);
            mode = strategy.UpdateMode(eaStats, popStats);
            Assert.AreEqual(ComplexityRegulationMode.Simplifying, mode);
        }

        #endregion
    }
}
