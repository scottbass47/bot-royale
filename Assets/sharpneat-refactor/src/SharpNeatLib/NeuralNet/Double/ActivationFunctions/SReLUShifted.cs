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

namespace SharpNeat.NeuralNet.Double.ActivationFunctions
{
    /// <summary>
    /// S-shaped rectified linear activation unit (SReLU). Shifted on the x-axis so that x=0 gives y=0.5, in keeping with the logistic sigmoid.
    /// From:
    ///    https://en.wikipedia.org/wiki/Activation_function
    ///    https://arxiv.org/abs/1512.07030 [Deep Learning with S-shaped Rectified Linear Activation Units]
    ///    
    /// </summary>
    public sealed class SReLUShifted : IActivationFunction<double>
    {
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public double Fn(double x)
        {
            const double tl = 0.001; // threshold (left).
            const double tr = 0.999; // threshold (right).
            const double a = 0.00001;
            const double offset = 0.5;

            double y;
            if(x + offset > tl && x + offset < tr) {
                y = x + offset;
            }
            else if(x + offset <= tl) {
                y = tl + ((x + offset) - tl) * a;
            }
            else {
                y = tr + ((x + offset) - tr) * a;
            }

            return y;
        }

        public void Fn(double[] v)
        {
            // Naive implementation.
            for(int i=0; i < v.Length; i++) {
                v[i] = Fn(v[i]);
            }
        }

        public void Fn(double[] v, int startIdx, int endIdx)
        {
            // Naive implementation.
            for(int i=startIdx; i < endIdx; i++) {
                v[i] = Fn(v[i]);
            }
        }

        public void Fn(double[] v, double[] w, int startIdx, int endIdx)
        {
            // Naive implementation.
            for(int i=startIdx; i < endIdx; i++) {
                w[i] = Fn(v[i]);
            }
        }
    }
}
