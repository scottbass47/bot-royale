/* ***************************************************************************
 * This file is part of SharpNEAT - Evolution of Neural Networks.
 * 
 * Copyright 2004-2016 Colin Green (sharpneat@gmail.com)
 *
 * SharpNEAT is free software; you can redistribute it and/or modify
 * it under the terms of The MIT License (MIT).
 *
 * You should have received a copy of the MIT License
 * along with SharpNEAT; if not, see https://opensource.org/licenses/MIT.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using log4net;
using SharpNeat.Core;
using System.Threading.Tasks;
using UnityEngine;

// Disable missing comment warnings for non-private variables.
#pragma warning disable 1591

namespace SharpNeat.EvolutionAlgorithms
{
    /// <summary>
    /// Abstract class providing some common/baseline data and methods for implementations of IEvolutionAlgorithm.
    /// </summary>
    /// <typeparam name="TGenome">The genome type that the algorithm will operate on.</typeparam>
    public abstract class AbstractGenerationalAlgorithm<TGenome> : MonoBehaviour, IEvolutionAlgorithm<TGenome>
        where TGenome : class, IGenome<TGenome>
    {
        private static readonly ILog __log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Instance Fields

        protected IGenomeListEvaluator<TGenome> _genomeListEvaluator;
        protected IGenomeFactory<TGenome> _genomeFactory;
        protected List<TGenome> _genomeList;
        protected int _populationSize;
        protected TGenome _currentBestGenome;

        // Algorithm state data.
        volatile RunState _runState = RunState.NotReady;
        protected volatile uint _currentGeneration;

        // Update event scheme / data.
        UpdateScheme _updateScheme;
        uint _prevUpdateGeneration;
        long _prevUpdateTimeTick;

        // Misc working variables.
        bool _terminateFlag = false;
        IEnumerator algorithmCoroutine;

        #endregion

        #region Events

        /// <summary>
        /// Notifies listeners that some state change has occurred.
        /// </summary>
        public event EventHandler UpdateEvent;
        /// <summary>
        /// Notifies listeners that the algorithm has paused.
        /// </summary>
        public event EventHandler PausedEvent;
        /// <summary>
        /// Notifies listeners that a generation has completed 
        /// </summary>
        public event EventHandler<uint> GenerationEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current generation.
        /// </summary>
        public uint CurrentGeneration
        {
            get { return _currentGeneration; }
        }

        #endregion

        #region IEvolutionAlgorithm<TGenome> Members

        /// <summary>
        /// Gets or sets the algorithm's update scheme.
        /// </summary>
        public UpdateScheme UpdateScheme 
        {
            get { return _updateScheme; }
            set { _updateScheme = value; }
        }

        /// <summary>
        /// Gets the current execution/run state of the IEvolutionAlgorithm.
        /// </summary>
        public RunState RunState
        {
            get { return _runState; }
        }

        /// <summary>
        /// Gets the population's current champion genome.
        /// </summary>
        public TGenome CurrentChampGenome 
        {
            get { return _currentBestGenome; }
        }

        /// <summary>
        /// Gets a value indicating whether some goal fitness has been achieved and that the algorithm has therefore stopped.
        /// </summary>
        public bool StopConditionSatisfied 
        { 
            get { return _genomeListEvaluator.StopConditionSatisfied; }
        }

        /// <summary>
        /// Initializes the evolution algorithm with the provided IGenomeListEvaluator, IGenomeFactory
        /// and an initial population of genomes.
        /// </summary>
        /// <param name="genomeListEvaluator">The genome evaluation scheme for the evolution algorithm.</param>
        /// <param name="genomeFactory">The factory that was used to create the genomeList and which is therefore referenced by the genomes.</param>
        /// <param name="genomeList">An initial genome population.</param>
        public virtual void Initialize(IGenomeListEvaluator<TGenome> genomeListEvaluator,
                                       IGenomeFactory<TGenome> genomeFactory,
                                       List<TGenome> genomeList)
        {
            _currentGeneration = 0;
            _genomeListEvaluator = genomeListEvaluator;
            _genomeFactory = genomeFactory;
            _genomeList = genomeList;
            _populationSize = _genomeList.Count;
            _runState = RunState.Ready;
            _updateScheme = new UpdateScheme(new TimeSpan(0, 0, 1));
        }

        /// <summary>
        /// Initializes the evolution algorithm with the provided IGenomeListEvaluator
        /// and an IGenomeFactory that can be used to create an initial population of genomes.
        /// </summary>
        /// <param name="genomeListEvaluator">The genome evaluation scheme for the evolution algorithm.</param>
        /// <param name="genomeFactory">The factory that was used to create the genomeList and which is therefore referenced by the genomes.</param>
        /// <param name="populationSize">The number of genomes to create for the initial population.</param>
        public virtual void Initialize(IGenomeListEvaluator<TGenome> genomeListEvaluator,
                                       IGenomeFactory<TGenome> genomeFactory,
                                       int populationSize)
        {
            _currentGeneration = 0;
            _genomeListEvaluator = genomeListEvaluator;
            _genomeFactory = genomeFactory;
            _genomeList = genomeFactory.CreateGenomeList(populationSize, _currentGeneration);
            _populationSize = populationSize;
            _runState = RunState.Ready;
            _updateScheme = new UpdateScheme(new TimeSpan(0, 0, 1));
        }

        /// <summary>
        /// Starts the algorithm running. The algorithm will switch to the Running state from either
        /// the Ready or Paused states.
        /// </summary>
        public void StartContinue()
        {
            // RunState must be Ready or Paused.
            if(RunState.Ready == _runState)
            {   // If the coroutine hasn't been started, then start it 
                _runState = RunState.Running;
                if(algorithmCoroutine == null)
                {
                    algorithmCoroutine = AlgorithmThreadMethod();
                    StartCoroutine(algorithmCoroutine);
                }
                OnUpdateEvent();
            }
            else if(RunState.Paused == _runState)
            {   // Coroutine is paused. Resume execution.
                _runState = RunState.Running;
                OnUpdateEvent();
            }
            else if(RunState.Running == _runState)
            {   // Already running. Log a warning.
                __log.Warn("StartContinue() called but algorithm is already running.");
            }
            else {
                throw new SharpNeatException($"StartContinue() call failed. Unexpected RunState [{_runState}]");
            }
        }

        /// <summary>
        /// Alias for RequestPause().
        /// </summary>
        public void Stop()
        {
            RequestPause();
        }

        /// <summary>
        /// Requests that the algorithm pauses but doesn't wait for the algorithm thread to stop.
        /// The algorithm thread will pause when it is next convenient to do so, and will notify
        /// listeners via an UpdateEvent.
        /// </summary>
        public void RequestPause()
        {
            if(RunState.Running == _runState) {
                _runState = RunState.Paused;
            }
            else {
                __log.Warn("RequestPause() called but algorithm is not running.");
            }
        }

        /// <summary>
        /// Request that the algorithm pause and waits for the algorithm to do so. The algorithm
        /// thread will pause when it is next convenient to do so and notifies any UpdateEvent 
        /// listeners prior to returning control to the caller. Therefore it's generally a bad idea 
        /// to call this method from a GUI thread that also has code that may be called by the
        /// UpdateEvent - doing so will result in deadlocked threads.
        /// </summary>
        public void RequestPauseAndWait()
        {
            if(RunState.Running == _runState) {
                _runState = RunState.Paused;
            }
            else {
                __log.Warn("RequestPause() called but algorithm is not running.");
            }
        }

        public void RequestTerminateAndWait()
        {
            if(RunState.Running == _runState) 
            {   
                // Signal worker thread to terminate.
                _terminateFlag = true;
            }
        }

        public void Dispose()
        {
            RequestTerminateAndWait();
        }

        #endregion

        #region Private/Protected Methods [Evolution Algorithm]

        private IEnumerator AlgorithmThreadMethod()
        {
            _prevUpdateGeneration = 0;
            _prevUpdateTimeTick = DateTime.Now.Ticks;

            for(;;)
            {
                _currentGeneration++;
                PerformOneGeneration();
                OnGenerationEvent();

                if(UpdateTest())
                {
                    _prevUpdateGeneration = _currentGeneration;
                    _prevUpdateTimeTick = DateTime.Now.Ticks;
                     OnUpdateEvent();
                }


                // Check if a pause has been requested. 
                // Access to the flag is not thread synchronized, but it doesn't really matter if
                // we miss it being set and perform one other generation before pausing.
                if (_genomeListEvaluator.StopConditionSatisfied)
                {
                    _runState = RunState.Terminated;
                    yield break;
                }

                yield return new WaitWhile(() => _runState != RunState.Running);
            }
        }

        /// <summary>
        /// Returns true if it is time to raise an update event.
        /// </summary>
        private bool UpdateTest()
        {
            if(UpdateMode.Generational == _updateScheme.UpdateMode) {
                return (_currentGeneration - _prevUpdateGeneration) >= _updateScheme.Generations;
            }
            
            return (DateTime.Now.Ticks - _prevUpdateTimeTick) >= _updateScheme.TimeSpan.Ticks;
        }

        private void OnUpdateEvent()
        {
            if(null != UpdateEvent)
            {
                // Catch exceptions thrown by even listeners. This prevents listener exceptions from terminating the algorithm thread.
                try {
                    UpdateEvent(this, EventArgs.Empty);
                }
                catch(Exception ex) {
                    Debug.LogError("UpdateEvent listener threw exception");
                    Debug.LogError(ex.ToString());
                }
            }
        }

        private void OnPausedEvent()
        {
            if(null != PausedEvent)
            {
                // Catch exceptions thrown by even listeners. This prevents listener exceptions from terminating the algorithm thread.
                try {
                    PausedEvent(this, EventArgs.Empty);
                }
                catch(Exception ex) {
                    Debug.LogError("PausedEvent listener threw exception");
                    Debug.LogError(ex.ToString());
                }
            }
        }

        private void OnGenerationEvent()
        {
            if(null != GenerationEvent)
            {
                // Catch exceptions thrown by even listeners. This prevents listener exceptions from terminating the algorithm thread.
                try {
                    GenerationEvent(this, _currentGeneration);
                }
                catch(Exception ex) {
                    Debug.LogError("GenerationEvent listener threw exception");
                    Debug.LogError(ex.ToString());
                }
            }
        }


        /// <summary>
        /// Progress forward by one generation. Perform one generation/cycle of the evolution algorithm.
        /// </summary>
        protected abstract void PerformOneGeneration();

        #endregion
    }
}
