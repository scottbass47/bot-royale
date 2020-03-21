﻿/* ***************************************************************************
 * This file is part of SharpNEAT - Evolution of Neural Networks.
 * 
 * Copyright 2004-2019 Colin Green (sharpneat@gmail.com)
 *
 * SharpNEAT is free software; you can redistribute it and/or modify
 * it under the terms of The MIT License (MIT).
 *
 * You should have received a copy of the MIT License
 * along with SharpNEAT; if not, see https://opensource.org/licenses/MIT.
 */
using System;
using System.Collections.Generic;
using SharpNeat.BlackBox;
using SharpNeat.Evaluation;
using SharpNeat.Tasks.FunctionRegression;

namespace SharpNeat.Tasks.GenerativeFunctionRegression
{
    /// <summary>
    /// Evaluation scheme for the function regression task.
    /// </summary>
    public class GenerativeFnRegressionEvaluationScheme : IBlackBoxEvaluationScheme<double>
    {
        #region Instance Fields

        readonly ParamSamplingInfo _paramSamplingInfo;
        readonly double _gradientMseWeight;

        // Expected/correct response arrays.
        readonly double[] _yArrTarget;
        readonly double[] _gradientArrTarget;

        readonly IBlackBoxProbe _blackBoxProbe;

        #endregion

        #region Auto Properties [IBlackBoxEvaluationScheme]

        /// <summary>
        /// The number of black box inputs expected/required by the black box evaluation scheme.
        /// </summary>
        /// <remarks>
        /// A single bias input (input zero).
        /// </remarks>
        public int InputCount => 1;

        /// <summary>
        /// The number of black box inputs expected/required by the black box evaluation scheme.
        /// </summary>
        public int OutputCount => 1;

        /// <summary>
        /// Indicates if the evaluation scheme is deterministic, i.e. will always return the same fitness score for a given genome.
        /// </summary>
        /// <remarks>
        /// An evaluation scheme that has some random/stochastic characteristics may give a different fitness score at each invocation 
        /// for the same genome, such a scheme is non-deterministic.
        /// </remarks>
        public bool IsDeterministic => true;

        /// <summary>
        /// Gets a fitness comparer for the scheme.
        /// </summary>
        /// <remarks>
        /// Typically there is a single fitness score and a higher score is considered better/fitter. However, if there are multiple 
        /// fitness values assigned to a genome (e.g. where multiple measures of fitness are in use) then we need a task specific 
        /// comparer to determine the relative fitness between two instances of <see cref="FitnessInfo"/>.
        /// </remarks>
        public IComparer<FitnessInfo> FitnessComparer => PrimaryFitnessInfoComparer.Singleton;

        /// <summary>
        /// Represents the zero or null fitness for the task. I.e. e.g. for genomes that utterly fail at the task, or genomes that
        /// fail even to decode (not possible in NEAT).
        /// </summary>
        public FitnessInfo NullFitness => FitnessInfo.DefaultFitnessInfo;

        /// <summary>
        /// Indicates if the evaluators created by <see cref="CreateEvaluator"/> have state.
        /// </summary>
        /// <remarks>
        /// If an evaluator has no state then it is sufficient to create a single instance and to use that evaluator concurrently on multiple threads.
        /// If an evaluator has state then concurrent use requires the creation of one evaluator instance per thread.
        /// </remarks>
        public bool EvaluatorsHaveState => true;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="fn">The target function.</param>
        /// <param name="paramSamplingInfo">Sampling (defines the x range and sampling density).</param>
        /// <param name="gradientMseWeight">The fitness weighting to assign to the gradient mean squared error (MSE) score.</param>
        public GenerativeFnRegressionEvaluationScheme(
            Func<double,double> fn,
            ParamSamplingInfo paramSamplingInfo,
            double gradientMseWeight)
        {
            _paramSamplingInfo = paramSamplingInfo;
            _gradientMseWeight = gradientMseWeight;

            // Alloc arrays.
            int sampleCount = _paramSamplingInfo.SampleResolution;
            _yArrTarget = new double[sampleCount];
            _gradientArrTarget = new double[sampleCount];

            // Calculate the target responses (the expected/correct responses).
            FuncRegressionUtils.Probe(fn, paramSamplingInfo, _yArrTarget);
            FuncRegressionUtils.CalcGradients(paramSamplingInfo, _yArrTarget, _gradientArrTarget);

            // Create blackbox probe.
            _blackBoxProbe = CreateBlackBoxProbe(fn, paramSamplingInfo);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Create a new evaluator object.
        /// </summary>
        /// <returns>A new instance of <see cref="IPhenomeEvaluator{T}"/>.</returns>
        public IPhenomeEvaluator<IBlackBox<double>> CreateEvaluator()
        {
            return new FuncRegressionEvaluator(
                _paramSamplingInfo,
                _gradientMseWeight,
                _yArrTarget,
                _gradientArrTarget,
                _blackBoxProbe);
        }

        /// <summary>
        /// Accepts a <see cref="FitnessInfo"/>, which is intended to be from the fittest genome in the population, and returns a boolean
        /// that indicates if the evolution algorithm can stop, i.e. because the fitness is the best that can be achieved (or good enough).
        /// </summary>
        /// <param name="fitnessInfo">The fitness info object to test.</param>
        /// <returns>Returns true if the fitness is good enough to signal the evolution algorithm to stop.</returns>
        public bool TestForStopCondition(FitnessInfo fitnessInfo)
        {
            return (fitnessInfo.PrimaryFitness >= 100_000.0);
        }

        #endregion

        #region Private Static Methods

        private static GenerativeBlackBoxProbe CreateBlackBoxProbe(
            Func<double,double> fn,
            ParamSamplingInfo paramSamplingInfo)
        {
            // Determine the mid output value of the function (over the specified sample points) and a scaling factor
            // to apply the to neural network response for it to be able to recreate the function (because the neural net
            // output range is [0,1] when using the logistic function as the neuron activation function).
            FuncRegressionUtils.CalcFunctionMidAndScale(
                fn, paramSamplingInfo,
                out double mid,
                out double scale);

            return new GenerativeBlackBoxProbe(
                paramSamplingInfo.SampleResolution,
                mid, scale);
        }

        #endregion
    }
}
