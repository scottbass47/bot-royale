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
using System.Diagnostics;
using SharpNeat.Network;

namespace SharpNeat.Neat.Genome
{
    /// <summary>
    /// Connections genes represented as a structure of arrays (see https://en.wikipedia.org/wiki/AOS_and_SOA).
    /// Element i of each array represents a value relating to the i'th gene.
    /// </summary>
    /// <typeparam name="T">Connection weight data type.</typeparam>
    public class ConnectionGenes<T>
        where T : struct
    {        
        /// <summary>
        /// Array of directed connections; this describes the network structure.
        /// </summary>
        public readonly DirectedConnection[] _connArr;
        /// <summary>
        /// Array of connection weights.
        /// </summary>
        public readonly T[] _weightArr;

        #region Constructors

        /// <summary>
        /// Construct uninitialised arrays with the given length.
        /// </summary>
        /// <param name="length">Number of connection genes to allocate space for.</param>
        public ConnectionGenes(int length)
        {
            _connArr = new DirectedConnection[length];
            _weightArr = new T[length];
        }

        /// <summary>
        /// Construct with the pre-built arrays.
        /// </summary>
        /// <param name="connArr">Array of directed connections; this described the network structure.</param>
        /// <param name="weightArr">Array of connection weights.</param>
        public ConnectionGenes(
            DirectedConnection[] connArr,
            T[] weightArr)
        {
            Debug.Assert(null != connArr);
            Debug.Assert(null != weightArr);
            Debug.Assert(connArr.Length == weightArr.Length);

            _connArr = connArr;
            _weightArr = weightArr;
        }

        #endregion

        #region Properties / Indexers

        /// <summary>
        /// Gets the number of connection genes.
        /// </summary>
        public int Length => _connArr.Length;

        /// <summary>
        /// Connection gene indexer.
        /// </summary>
        /// <param name="idx">Index of the gene to get or set.</param>
        public (int srcIdx, int tgtIdx, T weight) this[int idx]
        {
            get
            {
                return (_connArr[idx].SourceId, _connArr[idx].TargetId, _weightArr[idx]);
            }
            set
            {
                _connArr[idx] = new DirectedConnection(value.srcIdx, value.tgtIdx);
                _weightArr[idx] = value.weight;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sort the connection genes by sourceID then targetID.
        /// </summary>
        public void Sort()
        {
            Array.Sort(_connArr, _weightArr);
        }

        #endregion
    }
}
