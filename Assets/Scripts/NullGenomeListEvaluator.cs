using SharpNeat.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NullGenomeListEvaluator<TGenome, TPhenome> : IGenomeListEvaluator<TGenome>
    where TGenome : class, IGenome<TGenome>
    where TPhenome : class
{
    private ulong evalCount = 0;
    public ulong EvaluationCount => evalCount;

    public bool StopConditionSatisfied => true;

    public IEnumerator Evaluate(IList<TGenome> genomeList)
    {
        foreach(var genome in genomeList)
        {
            evalCount++;
            genome.EvaluationInfo.SetFitness(0.0);
            genome.EvaluationInfo.AuxFitnessArr = null;
        }
        yield break;
    }

    public void Reset()
    {
        evalCount = 0;
    }
}
