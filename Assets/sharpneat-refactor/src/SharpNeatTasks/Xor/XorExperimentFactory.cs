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
using System.Text.Json;
using SharpNeat.Experiments;
using SharpNeat.NeuralNet;

namespace SharpNeat.Tasks.Xor
{
    /// <summary>
    /// A factory for creating instances of <see cref="INeatExperiment{T}"/> for the XOR task.
    /// </summary>
    public class XorExperimentFactory : INeatExperimentFactory<double>
    {
        const ActivationFunctionId __DefaultActivationFunctionName = ActivationFunctionId.LeakyReLU;

        /// <summary>
        /// Create a new instance of <see cref="INeatExperiment{T}"/>.
        /// </summary>
        /// <param name="configElem">Experiment config in json form.</param>
        /// <returns>A new instance of <see cref="INeatExperiment{T}"/>.</returns>
        public INeatExperiment<double> CreateExperiment(JsonElement configElem)
        {
            // Create an evaluation scheme object for the XOR task.
            var evalScheme = new XorEvaluationScheme();


            // Create a NeatExperiment object with the evaluation scheme,
            // and assign some default settings (these can be overridden by config).
            var experiment = new NeatExperiment<double>("XOR (Exclusive OR)", evalScheme)
            {
                IsAcyclic = true,
                ActivationFnName = __DefaultActivationFunctionName.ToString()
            };

            // Read standard neat experiment json config and use it configure the experiment.
            NeatExperimentJsonReader<double>.Read(experiment, configElem);
            return experiment;
        }
    }
}
